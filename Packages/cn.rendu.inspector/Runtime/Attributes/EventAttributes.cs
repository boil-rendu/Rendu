using System;

namespace RD.Inspector
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

    /// <summary>Inspector 初始化回调。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class OnInspectorInitAttribute : Attribute
    {
        public string MethodName { get; set; }

        public OnInspectorInitAttribute()
        {
        }

        public OnInspectorInitAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }

    /// <summary>Inspector GUI 自定义回调。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class OnInspectorGUIAttribute : Attribute
    {
        public string MethodName { get; set; }

        public OnInspectorGUIAttribute()
        {
        }

        public OnInspectorGUIAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }

    /// <summary>
    /// 状态更新回调。Action 有两种写法：
    /// 以 <c>@</c> 开头的表达式（如 "@$property.State.Expanded = false"）会被求值器执行，
    /// 否则按本对象上的方法名直接调用。
    /// </summary>

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OnStateUpdateAttribute : Attribute
    {
        public string Action { get; set; }

        public OnStateUpdateAttribute()
        {
        }

        public OnStateUpdateAttribute(string action)
        {
            Action = action;
        }
    }
}
