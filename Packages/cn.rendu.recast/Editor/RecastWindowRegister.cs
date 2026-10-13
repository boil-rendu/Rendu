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
                // 注册为 Rendu 工具窗口的一个页签(不再单独开窗口)
                ToolTabRegistry.Register(BatchNavMeshExportContent.TabName, 1010, BatchNavMeshExportContent.CreateContent);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[RecastWindowRegister] 注册工具页签失败: {e.Message}");
            }
        }
    }
}
