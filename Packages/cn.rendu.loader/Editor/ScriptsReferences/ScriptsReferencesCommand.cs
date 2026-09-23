using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.Core;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 脚本引用命令层 - 封装 Unity API 和文件操作
    /// </summary>
    public static class ScriptsReferencesCommand
    {
        /// <summary>
        /// 加载所有包的 packagegit.json
        /// </summary>
        public static List<RD.PackageGit> LoadPackageGits()
        {
            var packageGits = new List<RD.PackageGit>();
            
            foreach (string directory in Directory.GetDirectories("Packages", "cn.rendu.*"))
            {
                string packageGitPath = Path.Combine(directory, "packagegit.json");
                if (!File.Exists(packageGitPath))
                {
                    continue;
                }

                RD.PackageGit packageGit = RD.PackageGitHelper.Load(packageGitPath);
                if (packageGit.ScriptsReferences != null)
                {
                    packageGits.Add(packageGit);
                }
            }

            return packageGits;
        }

        /// <summary>
        /// 查找包含四个程序集定义的目录
        /// </summary>
        public static string FindFourAssemblyDirectory()
        {
            foreach (string directory in Directory.GetDirectories("Packages", "cn.rendu.*"))
            {
                if (File.Exists(Path.Combine(directory, "Runtime/Model/RD.Model.asmdef")))
                {
                    return directory;
                }
            }

            throw new Exception("Cannot find four assembly definition directory");
        }

        /// <summary>
        /// 查找程序集定义文件
        /// </summary>
        public static List<string> FindAssemblyDefinitions(string directory, IEnumerable<string> assNames)
        {
            var result = new List<string>();
            
            foreach (string assName in assNames)
            {
                var findRet = new List<string>();
                FileHelper.GetAllFiles(findRet, directory, $"RD.{assName}.asmdef");
                
                if (findRet.Count > 0)
                {
                    result.AddRange(findRet);
                }
            }

            return result;
        }

        /// <summary>
        /// 加载程序集定义文件
        /// </summary>
        public static AssemblyDefinitionAsset LoadAssemblyDefinition(string asmdefPath)
        {
            if (!File.Exists(asmdefPath))
            {
                Debug.LogError($"Assembly definition file not found: {asmdefPath}");
                return null;
            }

            string json = File.ReadAllText(asmdefPath);
            return JsonUtility.FromJson<AssemblyDefinitionAsset>(json);
        }

        /// <summary>
        /// 保存程序集定义文件
        /// </summary>
        public static void SaveAssemblyDefinition(string asmdefPath, AssemblyDefinitionAsset assemblyDefinition)
        {
            string json = JsonUtility.ToJson(assemblyDefinition, true);
            File.WriteAllText(asmdefPath, json);
        }

        /// <summary>
        /// 保存并刷新资源
        /// </summary>
        public static void SaveAndRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 更新脚本引用
        /// </summary>
        public static void UpdateScriptsReferences()
        {
            Debug.Log("[ScriptsReferences] ========== 开始更新脚本引用 ==========");

            // 收集所有包的引用
            var references = CollectReferences();
            Debug.Log($"[ScriptsReferences] 收集到 {references.Count} 个程序集的引用");

            // 查找包含四个程序集定义的目录
            string fourAssemblyDirectory = FindFourAssemblyDirectory();
            Debug.Log($"[ScriptsReferences] 程序集定义目录: {fourAssemblyDirectory}");

            // 更新程序集定义引用
            UpdateAssemblyDefinitions(references, fourAssemblyDirectory);

            // 刷新资源
            SaveAndRefresh();

            Debug.Log("[ScriptsReferences] ========== 更新脚本引用完成 ==========");
        }

        /// <summary>
        /// 收集所有包的引用
        /// </summary>
        private static Dictionary<string, HashSet<string>> CollectReferences()
        {
            var references = new Dictionary<string, HashSet<string>>
            {
                { "Model", new HashSet<string>() },
                { "Hotfix", new HashSet<string>() },
                { "ModelView", new HashSet<string>() },
                { "HotfixView", new HashSet<string>() }
            };

            var packageGits = LoadPackageGits();

            foreach (var packageGit in packageGits)
            {
                if (packageGit.ScriptsReferences == null)
                {
                    continue;
                }

                foreach (var item in packageGit.ScriptsReferences)
                {
                    string assName = item.Key;
                    string[] refs = item.Value;

                    if (references.ContainsKey(assName))
                    {
                        foreach (string s in refs)
                        {
                            references[assName].Add(s);
                        }
                    }
                }
            }

            return references;
        }

        /// <summary>
        /// 更新程序集定义引用
        /// </summary>
        private static void UpdateAssemblyDefinitions(Dictionary<string, HashSet<string>> references, string fourAssemblyDirectory)
        {
            var assemblyDefinitions = FindAssemblyDefinitions(fourAssemblyDirectory, references.Keys);

            foreach (var item in references)
            {
                string assName = item.Key;
                var refAss = item.Value;

                string asmdefPath = assemblyDefinitions.FirstOrDefault(x => x.Contains($"RD.{assName}.asmdef"));

                if (string.IsNullOrEmpty(asmdefPath))
                {
                    Debug.LogError($"[ScriptsReferences] Assembly definition not found for {assName}");
                    continue;
                }

                var assemblyDefinition = LoadAssemblyDefinition(asmdefPath);

                if (assemblyDefinition == null)
                {
                    Debug.LogError($"[ScriptsReferences] Failed to load assembly definition from {asmdefPath}");
                    continue;
                }

                List<string> list = refAss.ToList();
                list.Sort();
                assemblyDefinition.references = list.ToArray();

                SaveAssemblyDefinition(asmdefPath, assemblyDefinition);
                Debug.Log($"[ScriptsReferences] 更新 {assName} 引用: {list.Count} 个");
            }
        }
    }
}
