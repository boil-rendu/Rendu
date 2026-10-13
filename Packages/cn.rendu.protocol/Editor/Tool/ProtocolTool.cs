//------------------------------------------------------------
// 工具窗口: Protocol Tool —— 与 LoaderTool / UITool / ExcelTool / RecastTool 同构的薄壳。
// 界面由模块自己用特性声明(ProtoListModule / ProtoEditModule), 由 cn.rendu.panel 引擎绘制;
// 菜单入口在 ProtocolMenu 统一定义。
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditorWindows.Editor;
using UnityEditor;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// Protocol 工具窗口: 收纳"协议域"的工具模块。
    /// 模块类自己挂 [UIAutoMenu("菜单名", order, Scope = "protocol")], 本窗口按 Order 排序后自动挂树。
    /// </summary>
    public class ProtocolTool : GenericToolWindow
    {
        public static void OpenWindow()
        {
            // 停靠到 WindowsDefine 登记的同组窗口旁: 与其它 Rendu 工具窗口合成同一窗口下的页签
            var window = GetWindow<ProtocolTool>("Protocol", true, RD.Init.Editor.WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Show();
            }
        }

        /// <summary>本窗口只加载 Scope == "protocol" 的模块。</summary>
        protected override string AcceptScope => "protocol";
    }
}

#endif
