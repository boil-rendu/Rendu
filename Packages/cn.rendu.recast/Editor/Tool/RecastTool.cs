//------------------------------------------------------------
// 工具窗口: Recast Tool —— 与 LoaderTool / UITool / ExcelTool 同构的薄壳, 只负责:
//   1) 接受 Scope == "recast" 的模块; 2) 停靠到工具窗口组;  菜单入口在 RecastMenu 统一定义。
// 界面由模块自己用特性声明(见 NavMeshExportModule), 由 cn.rendu.draw 引擎绘制。
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditTool.Editor;
using UnityEditor;

namespace RD.Recast.Editor
{
    /// <summary>
    /// Recast 工具窗口: 收纳"Recast 域"的工具模块(目前是批量导出导航网格)。
    /// 模块类自己挂 [UIAutoMenu("菜单名", order, Scope = "recast")], 本窗口按 Order 排序后自动挂树。
    /// </summary>
    public class RecastTool : GenericToolWindow
    {
        public static void OpenWindow()
        {
            // 停靠到 WindowsDefine 登记的同组窗口旁: 与 Loader Tool / UI Tool / Excel Tool 合成同一窗口下的页签
            var window = GetWindow<RecastTool>("Recast", true, RD.EditTool.Editor.WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Show();
            }
        }

        /// <summary>本窗口只加载 Scope == "recast" 的模块。</summary>
        protected override string AcceptScope => "recast";
    }
}

#endif
