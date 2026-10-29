using System;

namespace RD.Draw
{
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
}
