using System;

namespace RD.EditorKit
{
    /// <summary>将方法显示为可点击的按钮。</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ButtonAttribute : Attribute
    {
        public string ButtonName { get; set; }
        public int ButtonHeight { get; set; }
        public ButtonStyle ButtonStyle { get; set; }
        public SdfIconType Icon { get; set; }
        public IconAlignment IconAlignment { get; set; }
        public string Tooltip { get; set; }

        public ButtonAttribute()
        {
        }

        public ButtonAttribute(string buttonName = null, int buttonHeight = 0)
        {
            ButtonName = buttonName;
            ButtonHeight = buttonHeight;
        }

        public ButtonAttribute(int buttonHeight)
        {
            ButtonHeight = buttonHeight;
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
