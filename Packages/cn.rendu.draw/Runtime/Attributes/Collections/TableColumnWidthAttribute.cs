using System;

namespace RD.Draw
{
    /// <summary>设置表格中的列宽。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class TableColumnWidthAttribute : Attribute
    {
        public int Width { get; set; }
        public bool Resizable { get; set; }

        public TableColumnWidthAttribute(int width)
        {
            Width = width;
            Resizable = true;
        }
    }
}
