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
        const string RootMenu = MenuDefine.ProjectRootMenu + "/PackageManager";
        const  int RootMenuPriority = 1000;
        
        const string CreatePackageMenu = RootMenu +"/Create Package";


        
        #region PackageManager 工具

        [MenuItem(CreatePackageMenu, false, RootMenuPriority + 1)]
        public static void CreateNewPackage()
        {
            PackageCreateView.OpenWindow();
        }

        #endregion

    }
}
