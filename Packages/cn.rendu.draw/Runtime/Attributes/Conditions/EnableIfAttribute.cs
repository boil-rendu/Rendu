using System;

namespace RD.Draw
{
    /// <summary>条件启用。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class EnableIfAttribute : Attribute
    {
        public string MemberName { get; set; }
        public object Value { get; set; }

        public EnableIfAttribute(string memberName)
        {
            MemberName = memberName;
        }

        public EnableIfAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }
    }
}
