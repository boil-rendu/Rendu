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
                // Recast 已改用声明式模块(NavMeshExportModule) + RecastTool 宿主绘制; 这里只登记宿主窗口
                WindowsDefine.RegisterWindowType(typeof(RecastTool));
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[RecastWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
