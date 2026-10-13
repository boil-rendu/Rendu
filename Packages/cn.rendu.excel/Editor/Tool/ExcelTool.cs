//------------------------------------------------------------
// 工具窗口: Excel Tool —— 与 LoaderTool / UITool 完全同构的薄壳, 只负责:
//   1) 接受 Scope == "excel" 的模块; 2) 提供菜单入口; 3) 停靠到工具窗口组。
// 界面由模块自己用特性声明(见 ExcelToolModule), 由 cn.rendu.panel 引擎绘制(IMGUI / UI Toolkit 双后端),
// 不再写 UXML + 手写控件绑定。
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditorWindows.Editor;
using UnityEditor;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 工具窗口: 收纳"Excel 域"的工具模块。
    /// 模块类自己挂 [UIAutoMenu("菜单名", order, Scope = "excel")], 本窗口按 Order 排序后自动挂树。
    /// 菜单入口在 ExcelMenu 里统一定义(与 RecastMenu / ProtocolMenu 一样), 本类不挂 [MenuItem]。
    /// </summary>
    public class ExcelTool : GenericToolWindow
    {
        public static void OpenWindow()
        {
            // 停靠到 WindowsDefine 登记的同组窗口旁: 与 Loader Tool / UI Tool 等合为同一个窗口下的页签
            var window = GetWindow<ExcelTool>("Excel", true, RD.Init.Editor.WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Show();
            }
        }

        /// <summary>本窗口只加载 Scope == "excel" 的模块。</summary>
        protected override string AcceptScope => "excel";
    }
}

#endif
