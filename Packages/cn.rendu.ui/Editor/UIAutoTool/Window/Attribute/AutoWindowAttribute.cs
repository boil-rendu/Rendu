using System;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>AutoTool 菜单标记：把模块类登记为左侧工具树的一个菜单项</summary>
    public class UIAutoMenuAttribute : Attribute
    {
        /// <summary>菜单显示名</summary>
        public string MenuName;

        /// <summary>排序值（越小越靠前）</summary>
        public int Order;

        /// <summary>构造：指定菜单名与排序值</summary>
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