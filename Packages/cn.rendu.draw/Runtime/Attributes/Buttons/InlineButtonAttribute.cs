using System;

namespace RD.Draw
{
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
