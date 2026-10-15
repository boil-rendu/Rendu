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
