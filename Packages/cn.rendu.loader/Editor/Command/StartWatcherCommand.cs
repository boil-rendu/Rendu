using System.Diagnostics;
using System.IO;
using RD.Init.Editor;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 启动监视器命令
    /// </summary>
    public class StartWatcherCommand : CommandBase
    {
        private readonly string _startConfig;
        private readonly bool _console;

        public StartWatcherCommand(string startConfig, bool console = true)
        {
            _startConfig = startConfig;
            _console = console;
        }

        public override void Execute()
        {
            string appPath = Define.AppDllPath;
            string workingDir = Define.AppWorkingDir;

            // 强制使用 dotnet 运行 DLL,避免独立可执行文件的问题
            if (!File.Exists(appPath))
            {
                UnityEngine.Debug.LogError($"[StartWatcher] 找不到监视器 DLL 文件！\n路径: {appPath}");
                EditorUtility.DisplayDialog("错误", "找不到监视器 DLL 文件！\n请先编译服务器项目。", "确定");
                return;
            }

            UnityEngine.Debug.Log("[StartWatcher] 使用 dotnet 运行 DLL: " + appPath);
            string arguments =
                $"{appPath} --SceneName=Watcher --StartConfig=StartConfig/{_startConfig} --Console={_console}";
            RunDotNetProcess(fileName: appPath, arguments: arguments, workingDirectory: workingDir, useDotnet: true);
        }

        private void RunDotNetProcess(string fileName, string arguments, string workingDirectory, bool useDotnet)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = useDotnet ? "dotnet" : fileName,
                    Arguments = useDotnet ? arguments : arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true,  // macOS 上独立可执行文件需要 UseShellExecute = true
                    CreateNoWindow = false
                };

                UnityEngine.Debug.Log($"[StartWatcher] 启动监视器: {psi.FileName} {psi.Arguments}");
                UnityEngine.Debug.Log($"[StartWatcher] UseShellExecute: {psi.UseShellExecute}");

                Process process = Process.Start(psi);
                if (process != null)
                {
                    UnityEngine.Debug.Log($"[StartWatcher] 监视器已启动，PID: {process.Id}");
                }
                else
                {
                    UnityEngine.Debug.LogError("[StartWatcher] 监视器启动失败");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[StartWatcher] 启动监视器失败: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("错误", $"启动监视器失败！\n{ex.Message}", "确定");
            }
        }
    }
}