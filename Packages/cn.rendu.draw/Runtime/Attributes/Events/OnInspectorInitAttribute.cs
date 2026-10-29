using System;

namespace RD.Draw
{
    /// <summary>Inspector 初始化回调。（永久占位: 引擎有意不实现 —— 初始化时机归宿主管, 用构造函数或 [OnInspectorGUI] 代替; 特性名保留以兼容 Odin 迁移代码）</summary>
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
}
