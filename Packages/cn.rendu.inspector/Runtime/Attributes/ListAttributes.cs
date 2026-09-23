using System;

namespace RD.Inspector
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

    /// <summary>列表绘制选项。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ListDrawerSettingsAttribute : Attribute
    {
        public bool ShowPaging { get; set; }
        public bool ShowItemCount { get; set; }
        public bool ShowFoldout { get; set; }
        public bool DefaultExpandedState { get; set; }
        public bool ShowIndexLabels { get; set; }
        public bool IsReadOnly { get; set; }
        public bool HideAddButton { get; set; }
        public bool HideRemoveButton { get; set; }
        public bool DraggableItems { get; set; }
        public int NumberOfItemsPerPage { get; set; }
        public bool PagingHasValue { get; set; }
        public string CustomAddFunction { get; set; }
        public string CustomRemoveIndexFunction { get; set; }
        public string CustomRemoveElementFunction { get; set; }
        public string OnBeginListElementGUI { get; set; }
        public string OnEndListElementGUI { get; set; }
        public string OnTitleBarGUI { get; set; }

        public ListDrawerSettingsAttribute()
        {
            NumberOfItemsPerPage = 15;
        }
    }

    public enum DictionaryDisplayOptions
    {
        CollapsedFoldout,
        ExpandedFoldout,
    }

    /// <summary>字典绘制选项。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DictionaryDrawerSettingsAttribute : Attribute
    {
        public string KeyLabel { get; set; }
        public string ValueLabel { get; set; }
        public bool IsReadOnly { get; set; }
        public DictionaryDisplayOptions DisplayMode { get; set; }

        public DictionaryDrawerSettingsAttribute(string keyLabel = null, string valueLabel = null)
        {
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
        }
    }

    /// <summary>枚举以按钮组方式展示。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class EnumToggleButtonsAttribute : Attribute
    {
    }
}
