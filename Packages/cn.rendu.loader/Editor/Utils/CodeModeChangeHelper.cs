using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 根据 CodeMode（Client / Server / ClientServer）切换项目中各代码目录的 AssemblyReference，
    /// 控制各目录编译时挂载到哪个 asmdef，实现按模式编译不同代码子集。
    /// </summary>
    public static class CodeModeChangeHelper
    {
        // Unity Package 可能位于 Packages/ 或缓存目录 Library/PackageCache/ 下
        private static readonly string[] moduleDirs = { "Packages", "Library/PackageCache" };

        // Scripts  = 常规运行时代码（不分模式）
        // CodeMode = 按模式隔离的专用实现（如 ClientEnetworkHelper / ServerEnetworkHelper）
        private static readonly string[] scriptDirs = { "Scripts", "CodeMode" };
        
        // Model / Core / Loader  = 通用程序集
        // ModelView / HotfixView   = 纯客户端程序集（Server 模式下不存在）
        private static readonly string[] modelDirs = {"Model", "Hotfix", "ModelView", "HotfixView", "Core", "Loader"};

        // Server / Client = 端特定代码    Share = 双端共享代码    ClientServer = 双端共用代码
        private static readonly string[] serverDirs = { "Server", "Client", "Share", "ClientServer" };

        // 各 CodeMode 下允许的 (ScriptDir, ModelDir) 组合
        // ModelView / HotfixView 为纯客户端模块，Server 模式不包含
        // CodeMode 目录仅包含 Model 模块
        private static readonly (string CodeMode, string ScriptDir, string ModelDir)[] referenceRules =
        {
            // ---------- Client 模式：全部模块 ----------
            ("Client",    "Scripts",  "Model"),
            ("Client",    "Scripts",  "ModelView"),
            ("Client",    "Scripts",  "Hotfix"),
            ("Client",    "Scripts",  "HotfixView"),
            ("Client",    "Scripts",  "Core"),
            ("Client",    "Scripts",  "Loader"),
            ("Client",    "CodeMode", "Model"),

            // ---------- Server 模式：不含 ModelView / HotfixView ----------
            ("Server",    "Scripts",  "Model"),
            ("Server",    "Scripts",  "Hotfix"),
            ("Server",    "Scripts",  "Core"),
            ("Server",    "Scripts",  "Loader"),
            ("Server",    "CodeMode", "Model"),

            // ---------- ClientServer 模式：全部模块 ----------
            ("ClientServer", "Scripts", "Model"),
            ("ClientServer", "Scripts", "ModelView"),
            ("ClientServer", "Scripts", "Hotfix"),
            ("ClientServer", "Scripts", "HotfixView"),
            ("ClientServer", "Scripts", "Core"),
            ("ClientServer", "Scripts", "Loader"),
            ("ClientServer", "CodeMode", "Model"),
        };
        
        
        // 操作计数器（纯日志用途）
        private static int createdCount;
        private static int deletedCount;

        /// <summary>
        /// 切换到指定的 CodeMode，创建或删除各代码目录下的 AssemblyReference.asmref 文件
        /// </summary>
        public static void ChangeToCodeMode(string codeMode)
        {
            createdCount = 0;
            deletedCount = 0;

            Debug.Log($"[CodeModeChangeHelper] ========== 开始生成 AssemblyReference ==========");
            Debug.Log($"[CodeModeChangeHelper] 当前 CodeMode: {codeMode}");

            // 遍历所有 cn.rendu.* 包下 {ScriptDir}/{ModelDir}/{ServerDir}/ 的叶子目录
            foreach (string moduleBase in moduleDirs)
            {
                foreach (string moduleDir in Directory.GetDirectories(moduleBase, "cn.rendu.*"))
                {
                    foreach (string scriptDir in scriptDirs)
                    {
                        string scriptFullPath = Path.Combine(moduleDir, scriptDir);
                        if (!Directory.Exists(scriptFullPath)) continue;

                        foreach (string modelDir in modelDirs)
                        {
                            string modelFullPath = Path.Combine(scriptFullPath, modelDir);
                            if (!Directory.Exists(modelFullPath)) continue;

                            foreach (string serverDir in serverDirs)
                            {
                                string serverFullPath = Path.Combine(modelFullPath, serverDir);
                                if (!Directory.Exists(serverFullPath)) continue;

                                // 对每个存在的叶子目录，判断是否需要创建/删除 .asmref
                                HandleAssemblyReferenceFile(codeMode, moduleDir, scriptDir, modelDir, serverDir);
                            }
                        }
                    }
                }
            }

            Debug.Log($"[CodeModeChangeHelper] 完成！创建: {createdCount}，删除: {deletedCount}");
            Debug.Log($"[CodeModeChangeHelper] ========== AssemblyReference 生成完成 ==========");
        }

        /// <summary>
        /// 对单个叶子目录，先删除旧 .asmref，再根据规则决定是否重建
        /// </summary>
        private static void HandleAssemblyReferenceFile(string codeMode, string moduleDir, string scriptDir, string modelDir, string serverDir)
        {
            string referenceKey = $"{codeMode}/{scriptDir}/{modelDir}/{serverDir}";
            string asmRefFilePath = Path.Combine(moduleDir, scriptDir, modelDir, serverDir, "AssemblyReference.asmref");

            bool existed = File.Exists(asmRefFilePath);
            DeleteAssemblyReference(asmRefFilePath);

            if (IsAllowedReference(codeMode, scriptDir, modelDir, serverDir))
            {
                // 允许引用：创建 .asmref，指向 RD.{modelDir} 程序集
                CreateAssemblyReference(asmRefFilePath, modelDir);
                if (!existed)
                {
                    createdCount++;
                    Debug.Log($"[CodeModeChangeHelper] 创建 AssemblyReference: {referenceKey} -> RD.{modelDir}");
                }
            }
            else if (existed)
            {
                // 不允许但之前存在：记录一次删除
                deletedCount++;
                Debug.Log($"[CodeModeChangeHelper] 删除 AssemblyReference: {referenceKey}");
            }
        }

        /// <summary>
        /// 判断当前目录在当前 CodeMode 下是否应持有 AssemblyReference
        ///
        /// 规则：
        ///   1. CodeMode/{X}/ 目录 → 仅 X == codeMode 时允许（精确匹配当前模式）
        ///   2. Scripts/Share/    → 始终允许（所有模式共享）
        ///   3. Scripts/{模块}/{角色}/ → 查 referenceRules 表，
        ///      Client 角色仅限 Client / ClientServer 模式，Server 角色仅限 Server / ClientServer 模式
        /// </summary>
        private static bool IsAllowedReference(string codeMode, string scriptDir, string modelDir, string serverDir)
        {
            // CodeMode 目录：与当前模式同名的子目录才可引用
            if (scriptDir == "CodeMode")
            {
                return serverDir == codeMode;
            }

            // Scripts/Share：所有模式都允许
            if (serverDir == "Share") return true;

            // 查表：当前 (CodeMode, ScriptDir, ModelDir) 组合是否在规则中
            foreach (var rule in referenceRules)
            {
                if (rule.CodeMode != codeMode || rule.ScriptDir != scriptDir || rule.ModelDir != modelDir)
                    continue;

                // 模块匹配后，Client 仅限 Client/ClientServer，Server 仅限 Server/ClientServer
                return serverDir switch
                {
                    "Client" => codeMode == "Client" || codeMode == "ClientServer",
                    "Server" => codeMode == "Server" || codeMode == "ClientServer",
                    _ => false
                };
            }
            return false;
        }

        private static void DeleteAssemblyReference(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            // 若流程走到这里但文件仍然存在：AssetDatabase 可能缓存了旧的 meta 状态，
            // 可补充 AssetDatabase.Refresh()，但不在批量循环内调用以避免性能问题
        }

        /// <summary>
        /// 创建一个 AssemblyReference.asmref，内容指向 RD.{modelDir} 程序集
        /// </summary>
        private static void CreateAssemblyReference(string path, string modelDir)
        {
            string content = $"{{ \"reference\": \"RD.{modelDir}\" }}";
            File.WriteAllText(path, content);
        }
    }
}