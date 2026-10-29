using System;

namespace RD.Draw
{
    /// <summary>不显示属性名标签。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideLabelAttribute : Attribute
    {
    }
}
