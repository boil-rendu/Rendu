using System.Diagnostics;
using System.IO;
using RD.Init.Editor;
using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 启动服务器命令
    /// </summary>
    public class StartServerCommand : CommandBase
    {
        private readonly string _sceneName;
        private readonly int _processCount;
        private readonly string _startConfig;
        private readonly bool _console;

        public StartServerCommand(string sceneName, int processCount, string startConfig, bool console)
        {
            _sceneName = sceneName;
            _processCount = processCount;
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
                UnityEngine.Debug.LogError($"[StartServer] 找不到服务器 DLL 文件！\n路径: {appPath}");
                EditorUtility.DisplayDialog("错误", "找不到服务器 DLL 文件！\n请先编译服务器项目。", "确定");
                return;
            }

            UnityEngine.Debug.Log("[StartServer] 使用 dotnet 运行 DLL: " + appPath);
            string arguments =
                $"{appPath} --SceneName={_sceneName} --Process={_processCount} --StartConfig=StartConfig/{_startConfig} --Console={_console}";
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
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                UnityEngine.Debug.Log($"[StartServer] 启动进程: {psi.FileName} {psi.Arguments}");
                UnityEngine.Debug.Log($"[StartServer] 工作目录: {psi.WorkingDirectory}");
                UnityEngine.Debug.Log($"[StartServer] UseShellExecute: {psi.UseShellExecute}");

                Process process = Process.Start(psi);
                if (process != null)
                {
                    UnityEngine.Debug.Log($"[StartServer] 进程已启动，PID: {process.Id}");
                }
                else
                {
                    UnityEngine.Debug.LogError("[StartServer] 进程启动失败");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[StartServer] 启动进程失败: {ex.Message}\n{ex.StackTrace}");
                EditorUtility.DisplayDialog("错误", $"启动服务器失败！\n{ex.Message}", "确定");
            }
        }
    }
}