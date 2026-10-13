using System;
using System.Diagnostics;
using System.IO;
using RD.Init.Editor;
using RD.EditTool.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 运行初始化脚本命令
    /// </summary>
    public class RunInitScriptsCommand : CommandBase
    {
        public override void Execute()
        {
            UnityEngine.Debug.Log("[RunInitScripts] ========== 开始运行初始化脚本 ==========");

            int scriptCount = 0;
            foreach (string directory in Directory.GetDirectories("Packages", "cn.rendu.*"))
            {
                string initScriptPath = Path.Combine(directory, "Init.ps1");

                if (!File.Exists(initScriptPath))
                {
                    UnityEngine.Debug.Log($"[RunInitScripts] 跳过: {initScriptPath} 不存在");
                    continue;
                }

                UnityEngine.Debug.Log($"[RunInitScripts] 运行初始化脚本: {initScriptPath}");
                scriptCount++;

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"-NoExit -ExecutionPolicy Bypass -File {initScriptPath}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process process = Process.Start(psi);
                    string output = process.StandardOutput.ReadToEnd();
                    UnityEngine.Debug.Log($"[RunInitScripts] 脚本输出:\n{output}");
                    UnityEngine.Debug.Log($"[RunInitScripts] 脚本执行完成: {initScriptPath}");
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[RunInitScripts] 脚本执行失败: {initScriptPath}, 错误: {ex.Message}");
                }
            }

            UnityEngine.Debug.Log($"[RunInitScripts] ========== 初始化脚本执行完成，共执行 {scriptCount} 个脚本 ==========");
        }
    }
}
