using System;

namespace RD.EditorKit
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

    /// <summary>选择文件夹路径。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FolderPathAttribute : Attribute
    {
        public string Parent { get; set; }
        public string FixedName { get; set; }
        public bool RequireExistingPath { get; set; }

        public FolderPathAttribute(string parent = null, string fixedName = null, bool requireExistingPath = false)
        {
            Parent = parent;
            FixedName = fixedName;
            RequireExistingPath = requireExistingPath;
        }
    }

    /// <summary>选择文件路径。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class FilePathAttribute : Attribute
    {
        public string Extensions { get; set; }
        public string ParentFolder { get; set; }
        public bool RequireExistingPath { get; set; }

        public FilePathAttribute()
        {
        }

        public FilePathAttribute(string extensions)
        {
            Extensions = extensions;
        }
    }

    /// <summary>隐藏表格内的绘制。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideInTablesAttribute : Attribute
    {
    }
}
