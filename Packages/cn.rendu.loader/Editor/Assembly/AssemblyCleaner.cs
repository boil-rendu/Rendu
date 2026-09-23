using System.IO;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 程序集清理器 - 退出编辑模式时清理 Library 中的 DLL
    /// </summary>
    public static class AssemblyCleaner
    {
        private static readonly string[] DllNames = { "RD.Hotfix", "RD.HotfixView", "RD.Model", "RD.ModelView" };
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    {
                        OnExitingEditMode();
                        break;
                    }
                }
            };
        }

        /// <summary>
        /// 退出编辑模式时处理(即将进入运行模式)
        /// EnableDll模式时, 屏蔽掉Library的dll(通过改文件后缀方式屏蔽), 仅使用Define.CodeDir下的dll
        /// </summary>
        static void OnExitingEditMode()
        {
            foreach (string dll in DllNames)
            {
                string dllFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll";
                if (File.Exists(dllFile))
                {
                    File.Delete(dllFile);
                }

                string pdbFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb";
                if (File.Exists(pdbFile))
                {
                    File.Delete(pdbFile);
                }
            }
        }
    }
}