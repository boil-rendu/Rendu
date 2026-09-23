using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RD.Core;
using RD.Init.Editor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 链接 SLN 文件命令
    /// </summary>
    public class LinkSlnCommand : CommandBase
    {
        public override void Execute()
        {
            UnityEngine.Debug.Log("[LinkSln] ========== 开始链接 SLN 文件 ==========");

            string slnPath = Path.Combine(Directory.GetCurrentDirectory(), "Rendu.sln");
            if (File.Exists(slnPath))
            {
                File.Delete(slnPath);
                UnityEngine.Debug.Log($"[LinkSln] 删除旧的 SLN 文件: {slnPath}");
            }

            List<string> slns = new List<string>();
            FileHelper.GetAllFiles(slns, "./Packages", "Rendu.sln");

            if (slns.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[LinkSln] 在 Packages 中未找到 Rendu.sln，请等待 Rider 插件生成。");
                return;
            }

            UnityEngine.Debug.Log($"[LinkSln] 找到 SLN 文件: {slns[0]}");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-c New-Item -ItemType HardLink -Target {slns[0]} ./Rendu.sln",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                Process process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                UnityEngine.Debug.Log($"[LinkSln] SLN 链接输出:\n{output}");
                UnityEngine.Debug.Log($"[LinkSln] SLN 链接完成: {slnPath}");
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[LinkSln] SLN 链接失败: {ex.Message}");
            }

            // link xml
            string xmlFile = Path.Combine(Path.GetDirectoryName(slns[0]), "link.xml");
            if (File.Exists(xmlFile))
            {
                UnityEngine.Debug.Log($"[LinkSln] 找到 link.xml: {xmlFile}");

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "pwsh",
                        Arguments = $"-c New-Item -ItemType HardLink -Target {xmlFile} ./Assets/link.xml",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    Process process2 = Process.Start(psi);
                    string output2 = process2.StandardOutput.ReadToEnd();
                    UnityEngine.Debug.Log($"[LinkSln] link.xml 链接输出:\n{output2}");
                    UnityEngine.Debug.Log($"[LinkSln] link.xml 链接完成: ./Assets/link.xml");
                    process2.WaitForExit();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LinkSln] link.xml 链接失败: {ex.Message}");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[LinkSln] 未找到 link.xml 文件");
            }

            UnityEngine.Debug.Log("[LinkSln] ========== SLN 文件链接完成 ==========");
        }
    }
}
