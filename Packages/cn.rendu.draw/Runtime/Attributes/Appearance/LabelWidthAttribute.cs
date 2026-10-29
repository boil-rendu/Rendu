using System;

namespace RD.Draw
{
    /// <summary>设置属性标签宽度。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class LabelWidthAttribute : Attribute
    {
        public float Width { get; set; }

        public LabelWidthAttribute(float width)
        {
            Width = width;
        }
    }
}
