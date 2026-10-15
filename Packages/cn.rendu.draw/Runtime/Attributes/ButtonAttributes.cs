using System;

namespace RD.Draw
{
    /// <summary>将方法显示为可点击的按钮。</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>显示排序号(与 [PropertyOrder] 同一排序通道), 未指定时按声明顺序。</summary>
        public int Order { get; }
        public string ButtonName { get; set; }
        public int ButtonHeight { get; set; }
        public ButtonStyle ButtonStyle { get; set; }
        public SdfIconType Icon { get; set; }
        public IconAlignment IconAlignment { get; set; }
        public string Tooltip { get; set; }

        public ButtonAttribute()
        {
        }

        public ButtonAttribute(string buttonName = null, int order = 0)
        {
            ButtonName = buttonName;
            Order      = order;
        }

        /// <summary>只指定排序号的写法: [Button(10)]。</summary>
        public ButtonAttribute(int order)
        {
            Order = order;
        }
    }

    /// <summary>把按钮绘制在其他成员的行内。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class InlineButtonAttribute : Attribute
    {
        public string MemberMethod { get; set; }
        public string ButtonLabel { get; set; }

        public InlineButtonAttribute(string memberMethod, string buttonLabel = null)
        {
            MemberMethod = memberMethod;
            ButtonLabel = buttonLabel;
        }
    }
}
