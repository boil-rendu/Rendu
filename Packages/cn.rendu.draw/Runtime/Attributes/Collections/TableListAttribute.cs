using System;

namespace RD.Draw
{
    /// <summary>将列表/数组以表格形式展示。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TableListAttribute : Attribute
    {
        public bool DrawScrollView { get; set; }
        public bool IsReadOnly { get; set; }
        public bool AlwaysExpanded { get; set; }
        public bool HideToolbar { get; set; }
        public bool ShowIndexLabels { get; set; }
        public bool ShowPaging { get; set; }
        public int NumberOfItemsPerPage { get; set; }
        public bool ShowItemCount { get; set; }

        public TableListAttribute(bool drawScrollView = true)
        {
            DrawScrollView = drawScrollView;
            NumberOfItemsPerPage = 15;
        }
    }
}
