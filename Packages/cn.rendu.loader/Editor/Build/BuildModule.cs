#if UNITY_EDITOR

using System.Linq;
using RD.Panel;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 构建模块: 选择目标平台(下拉) + 是否 Development Build + 构建/打开 Build Settings/清缓存。
    /// 替代原 BuildWindow + BuildViewModel + BuildSettings + BuildWindow.uxml。
    /// </summary>
    [UIAutoMenu("构建", 500, Scope = "loader")]
    public class BuildModule : BaseToolModule
    {
        private static readonly string[] PlatformNames =
            { "None", "Android", "iOS", "Windows", "MacOS", "Linux", "WebGL" };

        [LabelText("当前构建平台")]
        [ReadOnly]
        [ShowInInspector]
        public string ActivePlatform { get; private set; }

        [LabelText("当前代码模式")]
        [ReadOnly]
        [ShowInInspector]
        public string CurrentCodeMode { get; private set; }

        [LabelText("起始场景")]
        [ReadOnly]
        [ShowInInspector]
        public string StartScene { get; private set; }

        [LabelText("目标平台")]
        [ShowInInspector]
        [ValueDropdown("PlatformChoices")]
        public PlatformType TargetPlatform { get; set; }

        [LabelText("Development Build")]
        [ShowInInspector]
        public bool DevelopmentBuild { get; set; }

        private System.Collections.Generic.List<string> PlatformChoices() => PlatformNames.ToList();

        [Button("构建", 10)]
        [GUIColor(0.53f, 0.95f, 0.72f)]
        public void Build()
        {
            if (TargetPlatform == PlatformType.None)
            {
                Debug.LogError("[Build] 请选择平台!");
                return;
            }

            // CodeMode 必须是 Client
            var codeModeAsset = AssetDatabase.LoadAssetAtPath<GlobalConfig>(GlobalConfigModel.AssetPath);
            if (codeModeAsset == null || codeModeAsset.CodeMode != CodeMode.Client)
            {
                Debug.LogError("[Build] 构建 CodeMode 必须是 CodeMode.Client，请先在「全局配置」中切换");
                return;
            }

            var activePlatform = GetActivePlatformType();
            if (TargetPlatform != activePlatform)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "警告",
                    $"当前平台是 {activePlatform}，切换到 {TargetPlatform} 可能需要较长时间",
                    "切换", "取消", "不切换");
                if (choice == 1) return;
                if (choice == 2) { TargetPlatform = activePlatform; return; }
            }

            var buildOptions = DevelopmentBuild ? BuildOptions.Development : BuildOptions.None;
            new BuildApplicationCommand(TargetPlatform, buildOptions).Execute();
        }

        [Button("打开 Build Settings", 20)]
        public void OpenBuildSettings()
        {
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
        }

        [Button("清理构建缓存 (Bee/BuildCache/PlayerDataCache)", 30)]
        public void ClearBuildCache()
        {
            var libraryPath = "Library";
            var cacheDirs = new[] { "Bee", "BuildCache", "PlayerDataCache" };

            foreach (var cacheDir in cacheDirs)
            {
                var cachePath = System.IO.Path.Combine(libraryPath, cacheDir);
                if (!System.IO.Directory.Exists(cachePath)) continue;
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
            Debug.Log("构建缓存已清理完成");
        }

        public override void Initialize()
        {
            Refresh();
            // 默认从当前激活平台推断
            TargetPlatform = GetActivePlatformType();
            DevelopmentBuild = false;
        }

        public override void OnDestroy()
        {
        }

        public override void SelectionMenu()
        {
            // 每次被选中都刷新一下当前平台/场景, 不然切换构建目标后这里显示的还是上次的
            Refresh();
            base.SelectionMenu();
        }

        private void Refresh()
        {
            ActivePlatform = EditorUserBuildSettings.activeBuildTarget.ToString();

            var codeModeAsset = AssetDatabase.LoadAssetAtPath<GlobalConfig>(GlobalConfigModel.AssetPath);
            CurrentCodeMode = codeModeAsset != null ? codeModeAsset.CodeMode.ToString() : "未加载";

            var startScene = EditorBuildSettings.scenes.FirstOrDefault(s => s.enabled);
            StartScene = startScene != null && !string.IsNullOrEmpty(startScene.path)
                ? startScene.path
                : "未设置（请在 Build Settings 中添加并启用场景）";
        }

        private static PlatformType GetActivePlatformType()
        {
            return EditorUserBuildSettings.activeBuildTarget switch
            {
                BuildTarget.Android            => PlatformType.Android,
                BuildTarget.iOS                => PlatformType.IOS,
                BuildTarget.StandaloneWindows64 => PlatformType.Windows,
                BuildTarget.StandaloneOSX      => PlatformType.MacOS,
                BuildTarget.StandaloneLinux64  => PlatformType.Linux,
                BuildTarget.WebGL              => PlatformType.WebGL,
                _                              => PlatformType.None,
            };
        }
    }
}

#endif
