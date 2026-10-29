using System;

namespace RD.Draw
{
    /// <summary>条件禁用。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class DisableIfAttribute : Attribute
    {
        public string MemberName { get; set; }
        public object Value { get; set; }

        public DisableIfAttribute(string memberName)
        {
            MemberName = memberName;
        }

        public DisableIfAttribute(string memberName, object value)
        {
            MemberName = memberName;
            Value = value;
        }
    }
}
