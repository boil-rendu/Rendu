using UnityEditor;
using UnityEngine;

namespace RD.Init.Editor
{
    /// <summary>
    /// Init 菜单管理器 
    /// 提供统一的菜单入口和快捷键管理
    /// </summary>
    internal static class InitMenu
    {
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Init";
        const  int RootMenuPriority = 10;
        
        
        #region 主菜单 

        [MenuItem(RootMenu + "/Home Page", false, RootMenuPriority)]
        public static void OpenHomePage()
        {
            Application.OpenURL("https://github.com/your-repo/rendu");
        }

        [MenuItem(RootMenu + "/Documentation", false, RootMenuPriority + 1)]
        public static void OpenRootDocumentation()
        {
            Application.OpenURL("https://github.com/your-repo/rendu/wiki");
        }

        #endregion
        
     
    }
}
