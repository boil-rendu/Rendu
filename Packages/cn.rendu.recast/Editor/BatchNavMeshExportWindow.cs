using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.Init.Editor;
using UnityEngine.UIElements;

namespace RD.Recast.Editor
{
    /// <summary>
    /// 批量导航网格导出窗口 - UI Toolkit 实现
    /// </summary>
    public class BatchNavMeshExportWindow : EditorWindow
    {
        private VisualElement _root;

        // 场景列表
        private List<SceneConfig> sceneConfigs = new List<SceneConfig>();
        private ListView _sceneListView;

        // 导出配置
        private string exportPath = "";
        private bool keepIntermediateFiles = false;

        // Recast 参数
        private float cellSize = 0.1666667f;
        private float cellHeight = 0.05f;
        private float agentRadius = 0.5f;
        private float agentHeight = 2.0f;
        private float agentMaxClimb = 0.25f;
        private float agentMaxSlope = 45.0f;

        // 导出状态
        private Label _statusLabel;
        private ProgressBar _progressBar;

        public static void ShowWindow()
        {
            var window = GetWindow<BatchNavMeshExportWindow>("批量导出导航网格", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(exportPath))
            {
                var pathConfig = new PathConfig();
                exportPath = pathConfig.RecastExportPath;
            }
        }

        private void CreateGUI()
        {
            var visualTree = UxmlLoader.LoadWindowUXML<BatchNavMeshExportWindow>();
            if (visualTree == null)
            {
                Debug.LogError($"无法加载 UXML 文件: BatchNavMeshExportWindow");
                return;
            }

            _root = visualTree.CloneTree();
            rootVisualElement.Add(_root);

            SetupVisualElements();
            BindEvents();

            ScanScenes();
        }

        private void SetupVisualElements()
        {
            var helpBoxContainer = _root.Q<VisualElement>("helpBoxContainer");
            if (helpBoxContainer != null)
            {
                var helpBox = new HelpBox("自动扫描所有场景文件，勾选需要导出的场景，一键生成导航网格", HelpBoxMessageType.Info);
                helpBoxContainer.Add(helpBox);
            }

            _sceneListView = _root.Q<ListView>("sceneListView");
            _statusLabel = _root.Q<Label>("statusLabel");
            _progressBar = _root.Q<ProgressBar>("progressBar");

            if (_sceneListView != null)
            {
                _sceneListView.makeItem = MakeSceneItem;
                _sceneListView.bindItem = BindSceneItem;
                _sceneListView.itemsSource = sceneConfigs;
            }

            var exportPathField = _root.Q<TextField>("exportPathField");
            if (exportPathField != null)
            {
                exportPathField.value = exportPath;
                exportPathField.RegisterValueChangedCallback(evt => exportPath = evt.newValue);
            }

            var keepIntermediateToggle = _root.Q<Toggle>("keepIntermediateToggle");
            if (keepIntermediateToggle != null)
            {
                keepIntermediateToggle.value = keepIntermediateFiles;
                keepIntermediateToggle.RegisterValueChangedCallback(evt => keepIntermediateFiles = evt.newValue);
            }

            SetupRecastParameterFields();
        }

        private void SetupRecastParameterFields()
        {
            var cellSizeField = _root.Q<FloatField>("cellSizeField");
            if (cellSizeField != null)
            {
                cellSizeField.value = cellSize;
                cellSizeField.RegisterValueChangedCallback(evt => cellSize = evt.newValue);
            }

            var cellHeightField = _root.Q<FloatField>("cellHeightField");
            if (cellHeightField != null)
            {
                cellHeightField.value = cellHeight;
                cellHeightField.RegisterValueChangedCallback(evt => cellHeight = evt.newValue);
            }

            var agentRadiusField = _root.Q<FloatField>("agentRadiusField");
            if (agentRadiusField != null)
            {
                agentRadiusField.value = agentRadius;
                agentRadiusField.RegisterValueChangedCallback(evt => agentRadius = evt.newValue);
            }

            var agentHeightField = _root.Q<FloatField>("agentHeightField");
            if (agentHeightField != null)
            {
                agentHeightField.value = agentHeight;
                agentHeightField.RegisterValueChangedCallback(evt => agentHeight = evt.newValue);
            }

            var agentMaxClimbField = _root.Q<FloatField>("agentMaxClimbField");
            if (agentMaxClimbField != null)
            {
                agentMaxClimbField.value = agentMaxClimb;
                agentMaxClimbField.RegisterValueChangedCallback(evt => agentMaxClimb = evt.newValue);
            }

            var agentMaxSlopeField = _root.Q<FloatField>("agentMaxSlopeField");
            if (agentMaxSlopeField != null)
            {
                agentMaxSlopeField.value = agentMaxSlope;
                agentMaxSlopeField.RegisterValueChangedCallback(evt => agentMaxSlope = evt.newValue);
            }
        }

        private void BindEvents()
        {
            var selectAllButton = _root.Q<Button>("selectAllButton");
            if (selectAllButton != null)
            {
                selectAllButton.clicked += SelectAllScenes;
            }

            var deselectAllButton = _root.Q<Button>("deselectAllButton");
            if (deselectAllButton != null)
            {
                deselectAllButton.clicked += DeselectAllScenes;
            }

            var rescanButton = _root.Q<Button>("rescanButton");
            if (rescanButton != null)
            {
                rescanButton.clicked += ScanScenes;
            }

            var browsePathButton = _root.Q<Button>("browsePathButton");
            if (browsePathButton != null)
            {
                browsePathButton.clicked += BrowseExportPath;
            }

            var exportButton = _root.Q<Button>("exportButton");
            if (exportButton != null)
            {
                exportButton.clicked += StartBatchExport;
            }
        }

        private VisualElement MakeSceneItem()
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.paddingTop = 5;
            item.style.paddingBottom = 5;
            item.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

            var toggle = new Toggle();
            toggle.style.width = 30;

            var nameLabel = new Label();
            nameLabel.style.minWidth = 200;

            var pathLabel = new Label();
            pathLabel.style.minWidth = 300;
            pathLabel.style.fontSize = 10;
            pathLabel.style.color = new Color(0.6f, 0.6f, 0.6f);

            item.Add(toggle);
            item.Add(nameLabel);
            item.Add(pathLabel);

            return item;
        }

        private void BindSceneItem(VisualElement element, int index)
        {
            var config = sceneConfigs[index];
            var container = element as VisualElement;

            var toggle = container[0] as Toggle;
            var nameLabel = container[1] as Label;
            var pathLabel = container[2] as Label;

            toggle.value = config.enabled;
            toggle.RegisterValueChangedCallback(evt => config.enabled = evt.newValue);

            nameLabel.text = config.sceneName;
            pathLabel.text = $"路径：{config.scenePath}";
        }

        private void SelectAllScenes()
        {
            foreach (var config in sceneConfigs)
            {
                config.enabled = true;
            }
            _sceneListView.Rebuild();
        }

        private void DeselectAllScenes()
        {
            foreach (var config in sceneConfigs)
            {
                config.enabled = false;
            }
            _sceneListView.Rebuild();
        }

        private void BrowseExportPath()
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择导出目录", exportPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                exportPath = selectedPath;
                var exportPathField = _root.Q<TextField>("exportPathField");
                if (exportPathField != null)
                {
                    exportPathField.value = exportPath;
                }
            }
        }

        private void ScanScenes()
        {
            sceneConfigs.Clear();

            string projectPath = Directory.GetCurrentDirectory();
            string[] excludeDirs = { "Library", "Temp", "obj", "Logs" };

            try
            {
                string[] sceneFiles = Directory.GetFiles(projectPath, "*.unity", SearchOption.AllDirectories);

                foreach (string sceneFile in sceneFiles)
                {
                    bool shouldSkip = false;
                    foreach (string excludeDir in excludeDirs)
                    {
                        if (sceneFile.Contains($"/{excludeDir}/") || sceneFile.Contains($"\\{excludeDir}\\"))
                        {
                            shouldSkip = true;
                            break;
                        }
                    }

                    if (shouldSkip)
                        continue;

                    string relativePath;
                    if (sceneFile.StartsWith(projectPath))
                    {
                        relativePath = sceneFile.Substring(projectPath.Length + 1).Replace("\\", "/");
                    }
                    else
                    {
                        relativePath = sceneFile.Replace("\\", "/");
                    }

                    string sceneName = Path.GetFileNameWithoutExtension(sceneFile);

                    sceneConfigs.Add(new SceneConfig
                    {
                        sceneName = sceneName,
                        scenePath = relativePath,
                        enabled = true
                    });
                }

                Debug.Log($"扫描到 {sceneConfigs.Count} 个场景文件");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"扫描场景失败：{e.Message}");
            }

            UpdateSceneListUI();
        }

        private void UpdateSceneListUI()
        {
            var sceneListLabel = _root.Q<Label>("sceneListLabel");
            if (sceneListLabel != null)
            {
                sceneListLabel.text = $"📋 场景列表 (共 {sceneConfigs.Count} 个)";
            }

            if (_sceneListView != null)
            {
                _sceneListView.itemsSource = sceneConfigs;
                _sceneListView.Rebuild();
            }
        }

        private void StartBatchExport()
        {
            var enabledScenes = sceneConfigs.Where(s => s.enabled).ToList();

            if (enabledScenes.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请至少选择一个场景！", "确定");
                return;
            }

            UpdateProgress("准备导出...", 0);

            try
            {
                int successCount = 0;
                int failedCount = 0;

                for (int i = 0; i < enabledScenes.Count; i++)
                {
                    var config = enabledScenes[i];

                    UpdateProgress($"正在导出：{config.sceneName} ({i + 1}/{enabledScenes.Count})", (float)i / enabledScenes.Count);

                    try
                    {
                        ExportScene(config);
                        successCount++;
                        Debug.Log($"✅ 场景 [{config.sceneName}] 导出成功");
                    }
                    catch (System.Exception e)
                    {
                        failedCount++;
                        Debug.LogError($"❌ 场景 [{config.sceneName}] 导出失败：{e.Message}");
                    }
                }

                UpdateProgress($"✅ 批量导出完成！成功：{successCount}, 失败：{failedCount}", 1.0f);

                EditorUtility.DisplayDialog("完成",
                    $"批量导出完成！\n成功：{successCount}\n失败：{failedCount}", "确定");
            }
            catch (System.Exception e)
            {
                UpdateProgress($"❌ 导出失败：{e.Message}", 0);
                EditorUtility.DisplayDialog("错误", e.Message, "确定");
            }
        }

        private void ExportScene(SceneConfig config)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != config.scenePath)
            {
                EditorSceneManager.OpenScene(config.scenePath);
            }

            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] rootObjects = activeScene.GetRootGameObjects();

            var sceneRootObj = new GameObject("SceneRoot");
            foreach (GameObject rootObj in rootObjects)
            {
                rootObj.transform.SetParent(sceneRootObj.transform, false);
            }

            var tempObj = new GameObject("TempNavMeshExporter");
            var exporter = tempObj.AddComponent<NavMeshExporter>();

            exporter.exportPath = exportPath;
            exporter.fileName = config.sceneName;
            exporter.keepIntermediateFiles = keepIntermediateFiles;
            exporter.cellSize = cellSize;
            exporter.cellHeight = cellHeight;
            exporter.agentRadius = agentRadius;
            exporter.agentHeight = agentHeight;
            exporter.agentMaxClimb = agentMaxClimb;
            exporter.agentMaxSlope = agentMaxSlope;
            exporter.sceneRoot = sceneRootObj.transform;

            Debug.Log($"开始导出场景 [{config.sceneName}]，根节点数量：{rootObjects.Length}");

            exporter.GenerateNavMesh();

            foreach (GameObject rootObj in rootObjects)
            {
                rootObj.transform.SetParent(null);
            }
            DestroyImmediate(sceneRootObj);
            DestroyImmediate(tempObj);
        }

        private void UpdateProgress(string status, float progress)
        {
            var progressBox = _root.Q<VisualElement>("progressBox");
            if (progressBox != null)
            {
                progressBox.style.display = DisplayStyle.Flex;
            }

            if (_statusLabel != null)
            {
                _statusLabel.text = status;
            }

            if (_progressBar != null)
            {
                _progressBar.value = progress;
                _progressBar.title = $"{progress * 100:F1}%";
            }
        }

        [System.Serializable]
        private class SceneConfig
        {
            public string sceneName;
            public string scenePath;
            public bool enabled;
        }
    }
}
