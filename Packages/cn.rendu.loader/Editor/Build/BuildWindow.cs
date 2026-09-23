using System.Collections.Generic;
using System.Linq;
using RD.Init.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 构建窗口
    /// </summary>
    public class BuildWindow : EditorWindow
    {
        private BuildViewModel _viewModel;

        private static readonly string[] PlatformNames = { "None", "Android", "iOS", "Windows", "MacOS", "Linux", "WebGL" };

        public static void ShowWindow()
        {
            var window = GetWindow<BuildWindow>("Build Package", true, WindowsDefine.DockedWindowTypes);
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable()
        {
            if (_viewModel != null)
                return;

            var globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            var activePlatform = GetActivePlatformType();

            var settings = new BuildSettings
            {
                GlobalConfig = globalConfig,
                ActivePlatform = activePlatform,
            };

            _viewModel = new BuildViewModel(settings);
        }

        private void OnFocus()
        {
            // 窗口获得焦点时刷新 UI
            RefreshUI();
        }

        private void OnDisable()
        {
            _viewModel?.Dispose();
        }

        public void CreateGUI()
        {
            try
            {
                VisualElement root = this.rootVisualElement;

                // 加载布局文件
                var visualAsset = UxmlLoader.LoadWindowUXML<BuildWindow>();
                if (visualAsset == null)
                    return;

                visualAsset.CloneTree(root);

                // 获取UI元素
                var buildPlatformLabel = root.Q<Label>("BuildPlatformLabel");
                var codeModeLabel = root.Q<Label>("CodeModeLabel");
                var platformDropdown = root.Q<DropdownField>("PlatformDropdown");
                var startSceneLabel = root.Q<Label>("StartSceneLabel");
                var developmentToggle = root.Q<Toggle>("DevelopmentToggle");
                var buildButton = root.Q<Button>("BuildButton");
                var openSettingsButton = root.Q<Button>("OpenSettingsButton");
                var clearCacheButton = root.Q<Button>("ClearCacheButton");

                // 当前构建平台
                if (buildPlatformLabel != null)
                    buildPlatformLabel.text = EditorUserBuildSettings.activeBuildTarget.ToString();

                // CodeMode（来自 GlobalConfig）
                if (codeModeLabel != null)
                {
                    var codeMode = _viewModel?.GlobalConfig?.CodeMode.ToString() ?? "未加载";
                    codeModeLabel.text = codeMode;
                }

                // 起始场景（取 Build Settings 中第一个启用的场景）
                if (startSceneLabel != null)
                {
                    var startScene = EditorBuildSettings.scenes
                        .FirstOrDefault(s => s.enabled);
                    startSceneLabel.text = startScene != null && !string.IsNullOrEmpty(startScene.path)
                        ? startScene.path
                        : "未设置（请在 Build Settings 中添加并启用场景）";
                }

                // 目标平台下拉框
                if (platformDropdown != null && _viewModel != null)
                {
                    platformDropdown.choices = new List<string>(PlatformNames);
                    var defaultPlatformName = PlatformNames[Mathf.Clamp((int)_viewModel.PlatformType, 0, PlatformNames.Length - 1)];
                    platformDropdown.value = defaultPlatformName;
                    platformDropdown.RegisterValueChangedCallback(evt =>
                    {
                        var idx = platformDropdown.choices.IndexOf(evt.newValue);
                        if (idx >= 0)
                            _viewModel.PlatformType = (PlatformType)idx;
                    });
                }

                // Development Build 开关
                if (developmentToggle != null && _viewModel != null)
                {
                    developmentToggle.value = (_viewModel.BuildOptions & BuildOptions.Development) != 0;
                    developmentToggle.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue)
                            _viewModel.BuildOptions |= BuildOptions.Development;
                        else
                            _viewModel.BuildOptions &= ~BuildOptions.Development;
                    });
                }

                // 构建按钮
                if (buildButton != null)
                {
                    buildButton.clicked += () =>
                    {
                        if (EditorUtility.DisplayDialog("确认构建", "确定要开始构建吗？", "确定", "取消"))
                        {
                            _viewModel?.BuildPackage();
                        }
                    };
                }

                if (openSettingsButton != null)
                {
                    openSettingsButton.clicked += OpenBuildSettings;
                }

                if (clearCacheButton != null)
                {
                    clearCacheButton.clicked += () =>
                    {
                        if (EditorUtility.DisplayDialog("清理缓存", "确定要清理构建缓存吗？", "确定", "取消"))
                        {
                            ClearBuildCache();
                        }
                    };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        private void RefreshUI()
        {
            if (rootVisualElement == null)
                return;

            // 刷新当前构建平台
            var buildPlatformLabel = rootVisualElement.Q<Label>("BuildPlatformLabel");
            if (buildPlatformLabel != null)
                buildPlatformLabel.text = EditorUserBuildSettings.activeBuildTarget.ToString();

            // 刷新 CodeMode
            var codeModeLabel = rootVisualElement.Q<Label>("CodeModeLabel");
            if (codeModeLabel != null)
            {
                var codeMode = _viewModel?.GlobalConfig?.CodeMode.ToString() ?? "未加载";
                codeModeLabel.text = codeMode;
            }

            // 刷新起始场景
            var startSceneLabel = rootVisualElement.Q<Label>("StartSceneLabel");
            if (startSceneLabel != null)
            {
                var startScene = EditorBuildSettings.scenes
                    .FirstOrDefault(s => s.enabled);
                startSceneLabel.text = startScene != null && !string.IsNullOrEmpty(startScene.path)
                    ? startScene.path
                    : "未设置（请在 Build Settings 中添加并启用场景）";
            }
        }

        private static PlatformType GetActivePlatformType()
        {
            return EditorUserBuildSettings.activeBuildTarget switch
            {
                BuildTarget.Android => PlatformType.Android,
                BuildTarget.iOS => PlatformType.IOS,
                BuildTarget.StandaloneWindows64 => PlatformType.Windows,
                BuildTarget.StandaloneOSX => PlatformType.MacOS,
                BuildTarget.StandaloneLinux64 => PlatformType.Linux,
                BuildTarget.WebGL => PlatformType.WebGL,
                _ => PlatformType.None,
            };
        }

        private void OpenBuildSettings()
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }

        private static void ClearBuildCache()
        {
            var libraryPath = "Library";
            var cacheDirs = new[] { "Bee", "BuildCache", "PlayerDataCache" };

            foreach (var cacheDir in cacheDirs)
            {
                var cachePath = System.IO.Path.Combine(libraryPath, cacheDir);
                if (System.IO.Directory.Exists(cachePath))
                {
                    try
                    {
                        System.IO.Directory.Delete(cachePath, true);
                        Debug.Log($"已清理构建缓存：{cachePath}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"清理缓存失败 {cachePath}: {ex.Message}");
                    }
                }
            }

            Debug.Log("构建缓存已清理完成");
        }
    }
}
