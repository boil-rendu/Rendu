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
        // 拍平成一级菜单项(与 Loader Tool / UI Tool 同风格), 不再套 Protocol 子菜单
        const string ToolMenu = MenuDefine.ProjectRootMenu + "/Protocol Tool";
        const  int ToolPriority = 1030;

        #region Protocol 工具

        [MenuItem(ToolMenu, false, ToolPriority)]
        public static void OpenProtoWindow()
        {
            ProtocolTool.OpenWindow();
        }

        #endregion

    }
}
