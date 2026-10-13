using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Recast.Editor
{
    /// <summary>
    /// Recast 菜单管理器 
    /// 提供统一的菜单入口和快捷键管理
    /// </summary>
    internal static class RecastMenu
    {
        // 拍平成一级菜单项(与 Loader Tool / UI Tool 同风格), 不再套 Recast 子菜单
        const string ExportMenu = MenuDefine.ProjectRootMenu + "/Recast Export";
        const  int ExportPriority = 1010;

        #region Recast 工具

        [MenuItem(ExportMenu, false, ExportPriority)]
        public static void BatchExport()
        {
            ToolTabRegistry.OpenTab(BatchNavMeshExportContent.TabName);
        }

        #endregion

    }
}
