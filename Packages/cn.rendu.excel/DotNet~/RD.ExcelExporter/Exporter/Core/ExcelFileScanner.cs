using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.Core;

namespace RD
{
    /// <summary>
    /// Excel 文件扫描器 - 负责扫描和查找 Excel 文件
    /// </summary>
    public class ExcelFileScanner
    {
        private readonly string solutionPath;
        private readonly PackagesLock packagesLock;

        public ExcelFileScanner(string solutionPath, PackagesLock packagesLock)
        {
            this.solutionPath = solutionPath;
            this.packagesLock = packagesLock;
        }

        public List<ExcelFileInfo> Scan()
        {
            var files = new List<ExcelFileInfo>();

            foreach ((string key, PackageInfo packageInfo) in packagesLock.dependencies)
            {
                string excelDir = Path.Combine(packageInfo.dir, "Excel");
                if (!Directory.Exists(excelDir))
                    continue;

                Console.WriteLine($"[ExcelFileScanner] 发现 Excel 目录: {excelDir}");

                foreach (string filePath in FileHelper.GetAllFiles(excelDir))
                {
                    if (!IsExcelFile(filePath))
                        continue;

                    string relativeDir = GetRelativeDirectory(excelDir, filePath);
                    files.Add(new ExcelFileInfo
                    {
                        RootDir = excelDir,
                        FilePath = filePath,
                        RelativeDir = relativeDir
                    });
                }
            }

            Console.WriteLine($"[ExcelFileScanner] 找到 {files.Count} 个 Excel 文件");
            return files;
        }

        private bool IsExcelFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            return fileName.EndsWith(".xlsx") && !fileName.StartsWith("~$") && !fileName.Contains("#");
        }

        private string GetRelativeDirectory(string rootDir, string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            return Path.GetRelativePath(rootDir, dir);
        }
    }

    /// <summary>
    /// Excel 文件信息
    /// </summary>
    public class ExcelFileInfo
    {
        public string RootDir { get; set; }
        public string FilePath { get; set; }
        public string RelativeDir { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FilePath);
    }
}
