using System.Linq;
using RD.Init.Editor;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 构建应用程序命令
    /// </summary>
    public class BuildApplicationCommand : CommandBase
    {
        private readonly PlatformType _platformType;
        private readonly BuildOptions _buildOptions;
        private const string RelativeDirPrefix = "./Release";
        private const string BuildFolder = "./Release/{0}/StreamingAssets/";

        public BuildApplicationCommand(PlatformType platformType, BuildOptions buildOptions)
        {
            _platformType = platformType;
            _buildOptions = buildOptions;
        }

        public override void Execute()
        {
            Build(_platformType, _buildOptions);
        }

        private static void Build(PlatformType type, BuildOptions buildOptions)
        {
            BuildTarget buildTarget = BuildTarget.StandaloneWindows;
            string programName = "RD";
            string exeName = programName;

            switch (type)
            {
                case PlatformType.Windows:
                    buildTarget = BuildTarget.StandaloneWindows64;
                    exeName += ".exe";
                    break;
                case PlatformType.Android:
                    buildTarget = BuildTarget.Android;
                    exeName += ".apk";
                    break;
                case PlatformType.IOS:
                    buildTarget = BuildTarget.iOS;
                    break;
                case PlatformType.MacOS:
                    buildTarget = BuildTarget.StandaloneOSX;
                    break;
                case PlatformType.Linux:
                    buildTarget = BuildTarget.StandaloneLinux64;
                    break;
                case PlatformType.WebGL:
                    buildTarget = BuildTarget.WebGL;
                    break;
            }

            AssetDatabase.Refresh();
            Debug.Log("start build");

            // 从 Build Settings 中获取启用的场景列表
            string[] levels = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (levels.Length == 0)
            {
                Debug.LogError("[Build] 没有启用的场景！请在 Build Settings 中添加并启用场景。");
                return;
            }

            Debug.Log($"[Build] 构建场景列表: {string.Join(", ", levels)}");
            BuildReport report = BuildPipeline.BuildPlayer(levels, $"{RelativeDirPrefix}/{exeName}", buildTarget, buildOptions);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.Log($"BuildResult:{report.summary.result}");
                return;
            }

            Debug.Log("finish build");
            EditorUtility.OpenWithDefaultApp(RelativeDirPrefix);
        }
    }
}
