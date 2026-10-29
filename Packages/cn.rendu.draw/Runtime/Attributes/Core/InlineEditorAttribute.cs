using System;

namespace RD.Draw
{
    /// <summary>占位兼容：表示内联编辑（Inspector 中当前为空操作）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class InlineEditorAttribute : Attribute
    {
        public bool Expanded { get; set; }

        public InlineEditorAttribute()
        {
        }

        public InlineEditorAttribute(bool expanded)
        {
            Expanded = expanded;
        }
    }
}
