using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 菜单管理器 
    /// 提供统一的菜单入口和快捷键管理
    /// </summary>
    internal static class ExcelMenu
    {
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Excel";
        const  int RootMenuPriority = 1020;
        
        const string ExportMenu = RootMenu +"/Export";
        
        #region Excel 工具

        [MenuItem(ExportMenu, false, RootMenuPriority)]
        public static void OpenExcelWindow()
        {
            ExcelViewWindow.Init();
        }

        #endregion
    }
}