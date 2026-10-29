using System;

namespace RD.Draw
{
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
