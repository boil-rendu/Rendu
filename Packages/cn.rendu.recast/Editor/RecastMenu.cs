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
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Recast";
        const  int RootMenuPriority = 1010;
        
        const string ExportMenu = RootMenu +"/Export";
        
        #region Recast 工具 

        [MenuItem(ExportMenu, false, RootMenuPriority)]
        public static void BatchExport()
        {
            BatchNavMeshExportWindow.ShowWindow();
        }

        #endregion

    }
}
