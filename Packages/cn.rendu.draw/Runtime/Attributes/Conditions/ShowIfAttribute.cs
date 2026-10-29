using System;

namespace RD.Draw
{
    /// <summary>条件显示：仅当指定成员返回 true 时显示。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class ShowIfAttribute : Attribute
    {
        public string MemberName { get; set; }
        public object Value { get; set; }
        public bool Animate { get; set; }

        public ShowIfAttribute(string memberName)
        {
            MemberName = memberName;
        }

        public ShowIfAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }
    }
}
