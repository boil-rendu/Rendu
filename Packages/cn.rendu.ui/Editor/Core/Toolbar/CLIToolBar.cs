#if UI
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>
    /// 在编辑器工具栏右侧添加 AI CLI 入口按钮
    /// 点击后在项目根目录用 Windows Terminal 打开一个 powershell 会话并执行配置的 CLI 命令
    /// </summary>
    [InitializeOnLoad]
    public static class CLIToolBar
    {
        static CLIToolBar()
        {
            ToolbarExtender.AddRightToolbarGUI(OnCLIToolbarGUI, 1000);
        }

        const string iconPath = "Packages/cn.rendu.ui/Editor/Core/Toolbar/Icon/icon_ai_cli.png";
        private static Texture _cachedIcon;

        private static void OnCLIToolbarGUI()
        {
            _cachedIcon ??= AssetDatabase.LoadAssetAtPath<Texture>(iconPath);
            var openAIName = UIConstHelper.Const.DefaultOpenAIName;
            if (string.IsNullOrEmpty(openAIName))
            {
                openAIName = "droid";
            }

            GUILayout.Space(5);
            GUIContent iconContent = new(string.Empty, _cachedIcon);
            iconContent.tooltip = openAIName;
            if (GUILayout.Button(iconContent))
            {
                var projectPath = System.IO.Path.GetDirectoryName(Application.dataPath);
                StartPowerShellProcess(openAIName, projectPath);
            }

            GUILayout.Space(5);
        }

        /// <summary>启动 wt.exe 打开 powershell 并执行命令；失败时弹窗提示常见排查方向</summary>
        private static void StartPowerShellProcess(string command, string workingDir)
        {
            try
            {
                Process process = new();
                var start = new ProcessStartInfo
                {
                    FileName = "wt.exe",
                    Arguments = $"-d \"{workingDir}\" powershell -NoExit -Command \"{command}\"",
                    CreateNoWindow = false,
                    ErrorDialog = true,
                    UseShellExecute = true,
                    WorkingDirectory = workingDir
                };

                process.StartInfo = start;
                process.Start();
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("执行失败",
                    $"无法执行命令: {command}\n\n错误信息: {ex.Message}\n\n请检查:\n1. 命令是否正确\n2. Windows Terminal是否已安装\n3. wt.exe是否在系统PATH环境变量中", "确定");
            }
        }
    }
}
#endif