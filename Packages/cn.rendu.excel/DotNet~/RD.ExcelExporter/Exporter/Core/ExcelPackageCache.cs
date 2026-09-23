using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// Excel 包缓存 - 管理 Excel 包的加载和缓存
    /// </summary>
    public class ExcelPackageCache : IDisposable
    {
        private readonly Dictionary<string, ExcelPackage> packages = new Dictionary<string, ExcelPackage>();

        public ExcelPackage GetPackage(string filePath)
        {
            if (packages.TryGetValue(filePath, out var package))
            {
                return package;
            }

            using Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            package = new ExcelPackage(stream);
            packages[filePath] = package;
            return package;
        }

        public void Dispose()
        {
            foreach (var package in packages.Values)
            {
                package.Dispose();
            }
            packages.Clear();
        }
    }
}
