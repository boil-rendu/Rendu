//------------------------------------------------------------
// 工具窗口: Loader Tool —— 把原来 Loader 菜单里那些平铺的窗口与动作收进一个工具窗。
// 自身只是 GenericToolWindow 的薄壳 + 接受 "loader" 作用域, 各功能由挂在 [UIAutoMenu(Scope="loader")]
// 的模块类提供:
//   100  宏定义                (MacroModule)
//   200  全局配置              (GlobalConfigModule)
//   300  项目初始化            (ProjectInitModule)
//   400  项目文件              (ProjectFilesModule)
//   500  构建                  (BuildModule)
//   600  服务器                (ServerModule)
//   700  脚本引用              (ScriptsReferencesModule)
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditTool.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// Loader 工具窗口: 统一收纳编译/构建/服务/项目文件/脚本引用/宏定义等"loader 域"工具。
    /// 各模块类自己挂 [UIAutoMenu("菜单名", order, Scope = "loader")], 本窗口按 Order 排序后
    /// 自动挂到左侧菜单树, 不必在此处登记。
    /// </summary>
    public class LoaderTool : GenericToolWindow
    {
        [MenuItem(RD.Init.Editor.MenuDefine.ProjectRootMenu + "/Loader Tool")]
        public static void OpenWindow()
        {
            // 停靠到 WindowsDefine 登记的同组窗口旁: Loader 工具与 Excel View / 批量导出导航网格 /
            // Protocol / PackageManager 等菜单窗口合为同一个窗口下的不同页签, 不再各自飘独立窗口
            var window = GetWindow<LoaderTool>("Loader", true, RD.EditTool.Editor.WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Show();
            }
        }

        // 菜单路径: "Rendu/Loader Tool" (由 MenuDefine.ProjectRootMenu 拼出),
        // 与 Rendu/快捷键/ 子菜单 (F6/F7 入口) 并列, 都在 Rendu 一级菜单下, 互不挤占。

        /// <summary>本窗口只加载 Scope == "loader" 的模块, 与 ui 包的 UITool 互不串台。</summary>
        protected override string AcceptScope => "loader";

        // 不重写 BuildToolMenuTree: 基类默认就是"反射扫描 + 按 Order 排序",
        // 所有 loader 模块会按 Order 自然落到菜单树的合适位置,
        // 不需要在头部/尾部追加额外根节点(与 UITool 的"头部发布 + 尾部全局设置"不同)。
    }
}

#endif
