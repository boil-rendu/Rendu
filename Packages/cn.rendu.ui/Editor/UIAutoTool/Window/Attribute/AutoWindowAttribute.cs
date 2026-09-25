using System;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    public class UIAutoMenuAttribute : Attribute
    {
        public string MenuName;

        public int Order;

        public UIAutoMenuAttribute(string menuName, int order = 0)
        {
            MenuName = menuName;
            Order    = order;
        }
    }

    public class UIAutoMenuData
    {
        public Type Type;

        public string MenuName;

        public int Order;
    }
}