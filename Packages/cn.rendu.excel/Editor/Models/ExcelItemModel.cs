using System;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 文件数据模型
    /// </summary>
    public class ExcelItemModel
    {
        public string FileName { get; }
        public string RelativeDir { get; }
        public string FullPath { get; }
        public string PackageDir { get; }

        public ExcelItemModel(string fileName, string relativeDir, string fullPath, string packageDir)
        {
            FileName = fileName;
            RelativeDir = relativeDir;
            FullPath = fullPath;
            PackageDir = packageDir;
        }
    }
}
