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
        // 一级菜单项(与 Loader Tool / UI Tool 同风格); 打开 Excel 工具窗口(声明式模块宿主)
        const string ExportMenu = MenuDefine.ProjectRootMenu + "/Excel Tool";
        const  int ExportPriority = 1020;

        #region Excel 工具

        [MenuItem(ExportMenu, false, ExportPriority)]
        public static void OpenExcelWindow()
        {
            ExcelTool.OpenWindow();
        }

        #endregion
    }
}