using System;

namespace RD.Draw
{
    /// <summary>垂直排列分组。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class VerticalGroupAttribute : PropertyGroupAttribute
    {
        public float PaddingTop { get; set; }
        public float PaddingBottom { get; set; }

        public VerticalGroupAttribute(string path, int order = 0)
            : base(path, order)
        {
        }

        public VerticalGroupAttribute(string path, float paddingTop, float paddingBottom)
            : base(path)
        {
            PaddingTop    = paddingTop;
            PaddingBottom = paddingBottom;
        }
    }
}
