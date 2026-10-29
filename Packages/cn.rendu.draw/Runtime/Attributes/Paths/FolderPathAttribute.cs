using System;

namespace RD.Draw
{
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
}
