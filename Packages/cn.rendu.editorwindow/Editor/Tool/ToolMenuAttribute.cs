using System;

namespace RD.EditorWindows.Editor
{
    /// <summary>工具窗口菜单标记: 把模块类登记为左侧工具树的一个菜单项。
    /// 名字沿用 UIAutoMenu 是历史原因(最早用在 UI 自动化工具上), 框架本身已经是通用的, 所有继承
    /// <see cref="GenericToolWindow"/> 的窗口都按这个特性反射扫描模块。</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class UIAutoMenuAttribute : Attribute
    {
        /// <summary>菜单显示名</summary>
        public string MenuName;

        /// <summary>排序值(越小越靠前)</summary>
        public int Order;

        /// <summary>
        /// 可选作用域: 限定本模块只被某个 <see cref="GenericToolWindow"/> 实例加载。
        /// 为空/null 表示接受所有作用域(默认, 用于跨包通用的工具模块);
        /// 非空时, 模块只会出现在 <see cref="GenericToolWindow.AcceptScope"/> 匹配它的窗口里
        /// (用于"按包位置拆分模块": ui 的模块归 AutoTool, loader 的模块归 LoaderTool)。
        /// </summary>
        public string Scope;

        /// <summary>构造: 指定菜单名与排序值</summary>
        public UIAutoMenuAttribute(string menuName, int order = 0)
        {
            MenuName = menuName;
            Order    = order;
        }
    }

    /// <summary>扫描 <see cref="UIAutoMenuAttribute"/> 后得到的菜单项数据</summary>
    public class UIAutoMenuData
    {
        /// <summary>模块类型</summary>
        public Type Type;

        /// <summary>菜单显示名</summary>
        public string MenuName;

        /// <summary>排序值</summary>
        public int Order;
    }
}
