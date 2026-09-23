using System;
using RD.Init.Editor;
using UnityEditor;

namespace RD.Recast.Editor
{
    /// <summary>
    /// Recast 包窗口类型注册器
    /// </summary>
    [InitializeOnLoad]
    public static class RecastWindowRegister
    {
        static RecastWindowRegister()
        {
            RegisterRecastWindows();
        }

        private static void RegisterRecastWindows()
        {
            try
            {
                // 注册 Recast 包中的窗口类型
                WindowsDefine.RegisterWindowType(typeof(BatchNavMeshExportWindow));
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[RecastWindowRegister] 注册窗口类型失败: {e.Message}");
            }
        }
    }
}
