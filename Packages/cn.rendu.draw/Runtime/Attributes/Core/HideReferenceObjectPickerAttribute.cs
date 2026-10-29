using System;

namespace RD.Draw
{
    /// <summary>隐藏“引用类型”的选择器（复制/引用选择框）。（永久占位: 引擎本就没有引用选择器、用户类型一律按实际类型内联展开, 等价于始终隐藏; 特性名保留以兼容 Odin 迁移代码）</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HideReferenceObjectPickerAttribute : Attribute
    {
    }
}
