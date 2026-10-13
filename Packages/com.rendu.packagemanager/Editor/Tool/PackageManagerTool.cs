//------------------------------------------------------------
// 工具窗口: PackageManager Tool —— 与 LoaderTool / UITool / ExcelTool / RecastTool / ProtocolTool 同构的薄壳。
// 界面由模块自己用特性声明(PackageCreateModule), 由 cn.rendu.panel 引擎绘制;
// 菜单入口在 PackageManagerMenu 统一定义。
//------------------------------------------------------------

#if UNITY_EDITOR

using RD.EditorWindows.Editor;
using UnityEditor;

namespace RD.PackageManager.Editor
{
    /// <summary>
    /// PackageManager 工具窗口: 收纳"包管理域"的工具模块(目前是创建 / 更新 Rendu Package)。
    /// 模块类自己挂 [UIAutoMenu("菜单名", order, Scope = "packagemanager")], 本窗口按 Order 排序后自动挂树。
    /// </summary>
    public class PackageManagerTool : GenericToolWindow
    {
        public static void OpenWindow()
        {
            // 停靠到 WindowsDefine 登记的同组窗口旁: 与其它 Rendu 工具窗口合成同一窗口下的页签
            var window = GetWindow<PackageManagerTool>("Package", true,
                                                       RD.Init.Editor.WindowsDefine.DockedWindowTypes);
            if (window != null)
            {
                window.Show();
            }
        }

        /// <summary>本窗口只加载 Scope == "packagemanager" 的模块。</summary>
        protected override string AcceptScope => "packagemanager";
    }
}

#endif
