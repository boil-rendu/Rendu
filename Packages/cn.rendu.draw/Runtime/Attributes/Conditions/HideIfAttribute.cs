using System;

namespace RD.Draw
{
    /// <summary>条件隐藏。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class HideIfAttribute : Attribute
    {
        public string MemberName { get; set; }
        public object Value { get; set; }

        public HideIfAttribute(string memberName)
        {
            MemberName = memberName;
        }

        public HideIfAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }
    }
}
