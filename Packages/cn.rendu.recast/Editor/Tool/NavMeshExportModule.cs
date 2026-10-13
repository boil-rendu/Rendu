//------------------------------------------------------------
// 批量导出导航网格(声明式模块版)
// 与原 BatchNavMeshExportContent(UXML 自绘)等价: 场景勾选表 + 导出设置 + Recast 参数 + 进度条,
// 但不再写 UXML、不手写 Q<T>()/事件绑定, 界面全靠特性描述, 由 cn.rendu.panel 引擎绘制。
//------------------------------------------------------------

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.EditorWindows.Editor;
using RD.Panel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace RD.Recast.Editor
{
    [UIAutoMenu("批量导出导航网格", 100, Scope = "recast")]
    public class NavMeshExportModule : BaseToolModule
    {
        //---------------- 场景列表 ----------------

        [ShowInInspector]
        [TableList]
        [LabelText("场景列表")]
        [ListDrawerSettings(IsReadOnly = true, HideAddButton = true, HideRemoveButton = true,
                            ShowItemCount = true, NumberOfItemsPerPage = 12, PagingHasValue = true)]
        [PropertyOrder(0)]
        private List<SceneExportRow> Scenes = new List<SceneExportRow>();

        [Button("✓ 全选", 1)]
        [HorizontalGroup("SceneOps")]
        [PropertyOrder(1)]
        private void SelectAll()
        {
            foreach (var row in Scenes) row.Enabled = true;
        }

        [Button("✗ 全不选", 2)]
        [HorizontalGroup("SceneOps")]
        [PropertyOrder(2)]
        private void DeselectAll()
        {
            foreach (var row in Scenes) row.Enabled = false;
        }

        [Button("🔄 重新扫描", 3)]
        [HorizontalGroup("SceneOps")]
        [PropertyOrder(3)]
        private void Rescan()
        {
            ScanScenes();
        }

        //---------------- 导出设置 ----------------

        [ShowInInspector]
        [BoxGroup("导出设置")]
        [LabelText("导出路径")]
        [PropertyOrder(10)]
        public string ExportPath = "";

        [Button("...", 11)]
        [BoxGroup("导出设置")]
        [PropertyOrder(11)]
        private void BrowseExportPath()
        {
            var selected = EditorUtility.OpenFolderPanel("选择导出目录", ExportPath, "");
            if (string.IsNullOrEmpty(selected) == false)
            {
                ExportPath = selected;
            }
        }

        [ShowInInspector]
        [BoxGroup("导出设置")]
        [LabelText("保留中间文件")]
        [PropertyOrder(12)]
        public bool KeepIntermediateFiles;

        //---------------- Recast 参数 ----------------

        [ShowInInspector, BoxGroup("Recast 参数"), LabelText("单元格大小"), PropertyOrder(20)]
        public float CellSize = 0.1666667f;

        [ShowInInspector, BoxGroup("Recast 参数"), LabelText("单元格高度"), PropertyOrder(21)]
        public float CellHeight = 0.05f;

        [ShowInInspector, BoxGroup("Recast 参数"), LabelText("角色半径"), PropertyOrder(22)]
        public float AgentRadius = 0.5f;

        [ShowInInspector, BoxGroup("Recast 参数"), LabelText("角色高度"), PropertyOrder(23)]
        public float AgentHeight = 2.0f;

        [ShowInInspector, BoxGroup("Recast 参数"), LabelText("最大攀爬高度"), PropertyOrder(24)]
        public float AgentMaxClimb = 0.25f;

        [ShowInInspector, BoxGroup("Recast 参数"), LabelText("最大斜坡角度"), PropertyOrder(25)]
        public float AgentMaxSlope = 45.0f;

        //---------------- 导出 ----------------

        [Button("🚀 开始批量导出", 30), GUIColor(0.53f, 0.95f, 0.72f)]
        [PropertyOrder(30)]
        private void StartBatchExport()
        {
            var enabled = Scenes.Where(s => s.Enabled).ToList();
            if (enabled.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请至少选择一个场景！", "确定");
                return;
            }

            Status   = "准备导出...";
            Progress = 0f;

            var successCount = 0;
            var failedCount  = 0;

            for (int i = 0; i < enabled.Count; i++)
            {
                var row = enabled[i];
                Status   = $"正在导出：{row.SceneName} ({i + 1}/{enabled.Count})";
                Progress = (float)i / enabled.Count;

                try
                {
                    ExportScene(row);
                    successCount++;
                    Debug.Log($"✅ 场景 [{row.SceneName}] 导出成功");
                }
                catch (Exception e)
                {
                    failedCount++;
                    Debug.LogError($"❌ 场景 [{row.SceneName}] 导出失败：{e.Message}");
                }
            }

            Progress = 1f;
            Status   = $"✅ 批量导出完成！成功：{successCount}, 失败：{failedCount}";
            EditorUtility.DisplayDialog("完成", $"批量导出完成！\n成功：{successCount}\n失败：{failedCount}", "确定");
        }

        //---------------- 进度 ----------------

        [ShowInInspector]
        [ProgressBar(height: 18f)]
        [LabelText("")]
        [PropertyOrder(40)]
        private float Progress;

        [ShowInInspector]
        [DisplayAsString]
        [LabelText("")]
        [PropertyOrder(41)]
        private string Status = "尚未开始";

        //---------------- 生命周期 ----------------

        public override void Initialize()
        {
            if (string.IsNullOrEmpty(ExportPath))
            {
                ExportPath = new PathConfig().RecastExportPath;
            }

            ScanScenes();
        }

        private void ScanScenes()
        {
            Scenes.Clear();

            var projectPath = Directory.GetCurrentDirectory();
            var excludeDirs = new[] { "Library", "Temp", "obj", "Logs" };

            try
            {
                var sceneFiles = Directory.GetFiles(projectPath, "*.unity", SearchOption.AllDirectories);

                foreach (var sceneFile in sceneFiles)
                {
                    var skip = excludeDirs.Any(dir => sceneFile.Contains($"/{dir}/") || sceneFile.Contains($"\\{dir}\\"));
                    if (skip) continue;

                    var relativePath = sceneFile.StartsWith(projectPath)
                        ? sceneFile.Substring(projectPath.Length + 1).Replace("\\", "/")
                        : sceneFile.Replace("\\", "/");

                    Scenes.Add(new SceneExportRow(Path.GetFileNameWithoutExtension(sceneFile), relativePath));
                }

                Debug.Log($"扫描到 {Scenes.Count} 个场景文件");
            }
            catch (Exception e)
            {
                Debug.LogError($"扫描场景失败：{e.Message}");
            }
        }

        private void ExportScene(SceneExportRow row)
        {
            if (SceneManager.GetActiveScene().path != row.ScenePath)
            {
                EditorSceneManager.OpenScene(row.ScenePath);
            }

            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();

            var sceneRootObj = new GameObject("SceneRoot");
            foreach (var rootObj in rootObjects)
            {
                rootObj.transform.SetParent(sceneRootObj.transform, false);
            }

            var tempObj = new GameObject("Temp");

            var exporter = tempObj.AddComponent<RD.Recast.NavMeshExporter>();
            exporter.exportPath             = ExportPath;
            exporter.fileName               = row.SceneName;
            exporter.keepIntermediateFiles  = KeepIntermediateFiles;
            exporter.cellSize               = CellSize;
            exporter.cellHeight             = CellHeight;
            exporter.agentRadius            = AgentRadius;
            exporter.agentHeight            = AgentHeight;
            exporter.agentMaxClimb          = AgentMaxClimb;
            exporter.agentMaxSlope          = AgentMaxSlope;
            exporter.sceneRoot              = sceneRootObj.transform;

            Debug.Log($"开始导出场景 [{row.SceneName}]，根节点数量：{rootObjects.Length}");

            exporter.GenerateNavMesh();

            foreach (var rootObj in rootObjects)
            {
                rootObj.transform.SetParent(null);
            }

            Object.DestroyImmediate(sceneRootObj);
            Object.DestroyImmediate(tempObj);
        }
    }
}

#endif
