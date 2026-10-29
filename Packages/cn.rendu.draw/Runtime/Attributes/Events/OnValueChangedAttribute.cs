using System;

namespace RD.Draw
{
    /// <summary>成员值变化时回调方法。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class OnValueChangedAttribute : Attribute
    {
        public string MethodName { get; set; }
        public bool IncludeChildren { get; set; }

        public OnValueChangedAttribute(string methodName, bool includeChildren = false)
        {
            MethodName = methodName;
            IncludeChildren = includeChildren;
        }
    }
}
