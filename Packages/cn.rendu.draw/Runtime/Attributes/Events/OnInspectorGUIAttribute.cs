using System;

namespace RD.Draw
{
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
}
