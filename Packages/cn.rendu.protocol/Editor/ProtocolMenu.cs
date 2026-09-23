using RD.Init.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Protocol 菜单管理器 
    /// 提供统一的菜单入口和快捷键管理
    /// </summary>
    internal static class ProtocolMenu
    {
        const string RootMenu = MenuDefine.ProjectRootMenu + "/Protocol";
        const  int RootMenuPriority = 1030;
        
        const string ExportMenu = RootMenu +"/Export";
        
        #region Protocol 工具 

        [MenuItem(ExportMenu, false, RootMenuPriority)]
        public static void OpenProtoWindow()
        {
            ProtoViewWindow.Init();
        }

        #endregion

    }
}
