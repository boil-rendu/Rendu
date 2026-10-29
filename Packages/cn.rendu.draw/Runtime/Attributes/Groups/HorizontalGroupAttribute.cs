using System;

namespace RD.Draw
{
    /// <summary>水平排列分组。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class HorizontalGroupAttribute : PropertyGroupAttribute
    {
        public override GroupKind Kind => GroupKind.Horizontal;

        public HorizontalGroupAttribute(string path = "_DefaultGroup", int order = 0, float width = 0f)
            : base(path, order)
        {
            Width = width;
        }
    }
}
