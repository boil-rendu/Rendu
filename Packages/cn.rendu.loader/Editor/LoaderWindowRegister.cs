using System;
using RD.Init.Editor;
using UnityEditor;

namespace RD.Loader.Editor
{
    /// <summary>
    /// Loader 包窗口类型注册器
    /// </summary>
    [InitializeOnLoad]
    public static class LoaderWindowRegister
    {
        static LoaderWindowRegister()
        {
            RegisterLoaderWindows();
        }

        private static void RegisterLoaderWindows()
        {
            try
            {
                // 注册 Loader 包中的窗口类型
                WindowsDefine.RegisterWindowTypes(
                    typeof(ProjectInitWindow),
                    typeof(GlobalConfigWindow),
                    typeof(ProjectFilesWindow),
                    typeof(ScriptsReferencesWindow),
                    typeof(DefineWindow),
                    typeof(ServerWindow),
                    typeof(BuildWindow)
                );
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[LoaderWindowRegister] 注册窗口类型失败: {e.Message}");
            }
        }
    }
}
