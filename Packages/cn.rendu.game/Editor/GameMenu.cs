using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Game.Editor
{
    /// <summary>
    /// Game 菜单管理器
    /// 提供统一的菜单入口和快捷键管理
    /// </summary>
    internal static class GameMenu
    {
        
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Game";
        const  int RootMenuPriority = 1050;
        
        const string InitMenu = RootMenu +"/Init";

        #region 运行初始化
        [MenuItem(InitMenu, false, RootMenuPriority)]
        public static void GameSpecificTool()
        {
            InitHelper.Init();
        }
        #endregion
    }
}
