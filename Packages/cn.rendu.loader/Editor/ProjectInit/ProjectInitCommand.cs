using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目初始化命令层 - 封装 Unity API 和文件操作
    /// </summary>
    public static class ProjectInitCommand
    {
        /// <summary>
        /// 查找包含 Rendu.sln 的包名
        /// </summary>
        public static string FindPackageName()
        {
            foreach (string directory in Directory.GetDirectories("./Packages"))
            {
                if (!File.Exists(Path.Combine(directory, "Rendu.sln")))
                {
                    continue;
                }

                string packageGitPath = Path.Combine(directory, "packagegit.json");
                if (File.Exists(packageGitPath))
                {
                    RD.PackageGit packageGit = RD.PackageGitHelper.Load(packageGitPath);
                    return packageGit.Name;
                }
            }

            throw new Exception("Cannot find package with Rendu.sln");
        }

        /// <summary>
        /// 切换代码模式
        /// </summary>
        public static void ChangeCodeMode(string codeMode)
        {
            var moduleDirs = new[] { "Packages", "Library/PackageCache" };
            var scriptDirs = new[] { "Scripts", "CodeMode" };
            var modelDirs = new[] { "Model", "Hotfix", "ModelView", "HotfixView", "Core", "Loader" };
            var serverDirs = new[] { "Server", "Client", "Share", "ClientServer" };

            var assemblyReferencePaths = new HashSet<string>
            {
                "Client/Scripts/Model/Client", "Client/Scripts/Model/Share", "Client/CodeMode/Model/Client",
                "Client/Scripts/ModelView/Client", "Client/Scripts/ModelView/Share", "Client/CodeMode/ModelView/Client",
                "Client/Scripts/Hotfix/Client", "Client/Scripts/Hotfix/Share", "Client/CodeMode/Hotfix/Client",
                "Client/Scripts/HotfixView/Client", "Client/Scripts/HotfixView/Share", "Client/CodeMode/HotfixView/Client",
                "Client/Scripts/Core/Client", "Client/Scripts/Core/Share", "Client/CodeMode/Core/Client",
                "Client/Scripts/Loader/Client", "Client/Scripts/Loader/Share", "Client/CodeMode/Loader/Client",

                "Server/Scripts/Model/Server", "Server/Scripts/Model/Share", "Server/CodeMode/Model/Server",
                "Server/Scripts/Hotfix/Server", "Server/Scripts/Hotfix/Share", "Server/CodeMode/Hotfix/Server",
                "Server/Scripts/Core/Server", "Server/Scripts/Core/Share", "Server/CodeMode/Core/Server",
                "Server/Scripts/Loader/Server", "Server/Scripts/Loader/Share", "Server/CodeMode/Loader/Server",

                "ClientServer/Scripts/Model/Client", "ClientServer/Scripts/Model/Server", "ClientServer/Scripts/Model/Share",
                "ClientServer/CodeMode/Model/ClientServer",
                "ClientServer/Scripts/ModelView/Client", "ClientServer/Scripts/ModelView/Server", "ClientServer/Scripts/ModelView/Share",
                "ClientServer/CodeMode/ModelView/ClientServer",
                "ClientServer/Scripts/Hotfix/Client", "ClientServer/Scripts/Hotfix/Server", "ClientServer/Scripts/Hotfix/Share",
                "ClientServer/CodeMode/Hotfix/ClientServer",
                "ClientServer/Scripts/HotfixView/Client", "ClientServer/Scripts/HotfixView/Server", "ClientServer/Scripts/HotfixView/Share",
                "ClientServer/CodeMode/HotfixView/ClientServer",
                "ClientServer/Scripts/Core/Client", "ClientServer/Scripts/Core/Share", "ClientServer/Scripts/Core/Server",
                "ClientServer/CodeMode/Core/ClientServer",
                "ClientServer/Scripts/Loader/Client", "ClientServer/Scripts/Loader/Share", "ClientServer/Scripts/Loader/Server",
                "ClientServer/CodeMode/Loader/ClientServer",
            };

            int createdCount = 0;
            int deletedCount = 0;

            Debug.Log($"[ProjectInitCommand] 开始生成 AssemblyReference，当前 CodeMode: {codeMode}");

            foreach (string moduleDir in moduleDirs)
            {
                foreach (string directory in Directory.GetDirectories(moduleDir, "cn.rendu.*"))
                {
                    foreach (string scriptDir in scriptDirs)
                    {
                        string scriptPath = Path.Combine(directory, scriptDir);
                        if (!Directory.Exists(scriptPath))
                        {
                            continue;
                        }

                        foreach (string modelDir in modelDirs)
                        {
                            string modelPath = Path.Combine(scriptPath, modelDir);
                            if (!Directory.Exists(modelPath))
                            {
                                continue;
                            }

                            foreach (string serverDir in serverDirs)
                            {
                                string serverPath = Path.Combine(modelPath, serverDir);
                                if (!Directory.Exists(serverPath))
                                {
                                    continue;
                                }

                                HandleAssemblyReferenceFile(codeMode, directory, scriptDir, modelDir, serverDir, 
                                    assemblyReferencePaths, ref createdCount, ref deletedCount);
                            }
                        }
                    }
                }
            }

            Debug.Log($"[ProjectInitCommand] 完成！创建: {createdCount}，删除: {deletedCount}");
        }

        private static void HandleAssemblyReferenceFile(string codeMode, string moduleDir, string scriptDir, 
            string modelDir, string serverDir, HashSet<string> assemblyReferencePaths, 
            ref int createdCount, ref int deletedCount)
        {
            string path = $"{codeMode}/{scriptDir}/{modelDir}/{serverDir}";
            string filePath = Path.Combine(moduleDir, scriptDir, modelDir, serverDir, "AssemblyReference.asmref");

            bool existed = File.Exists(filePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            if (assemblyReferencePaths.Contains(path))
            {
                string content = $"{{ \"reference\": \"RD.{modelDir}\" }}";
                File.WriteAllText(filePath, content);

                if (!existed)
                {
                    createdCount++;
                    Debug.Log($"[ProjectInitCommand] 创建 AssemblyReference: {path} -> RD.{modelDir}");
                }
            }
            else if (existed)
            {
                deletedCount++;
                Debug.Log($"[ProjectInitCommand] 删除 AssemblyReference: {path}");
            }
        }

        /// <summary>
        /// 检查项目是否已初始化
        /// </summary>
        public static bool CheckIsInited()
        {
            var currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone);
            return currentDefines.Contains("INITED");
        }

        /// <summary>
        /// 刷新资源数据库
        /// </summary>
        public static void RefreshAssetDatabase()
        {
            AssetDatabase.Refresh();
        }
    }
}
