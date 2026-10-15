using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// PackageManager 菜单管理器 
    /// 提供统一的菜单入口和快捷键管理
    /// </summary>
    internal static class PackageManagerMenu
    {
        // 拍平成一级菜单项(与 Loader Tool / UI Tool 同风格), 不再套 PackageManager 子菜单
        const string ToolMenu = MenuDefine.ProjectRootMenu + "/Package Manager Tool";
        const  int ToolPriority = 1000;

        #region PackageManager 工具

        [MenuItem(ToolMenu, false, ToolPriority)]
        public static void CreateNewPackage()
        {
            PackageManagerTool.OpenWindow();
        }

        #endregion

    }
}
