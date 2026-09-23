using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RD.Excel.Editor
{
    /// <summary>
    /// Excel 视图模型 - 业务逻辑和数据管理
    /// </summary>
    public class ExcelViewModel
    {
        // 数据
        private List<ExcelItemModel> _excelItems = new List<ExcelItemModel>();
        private readonly PathConfig _pathConfig = new PathConfig();

        public IReadOnlyList<ExcelItemModel> ExcelItems => _excelItems;

        /// <summary>
        /// 刷新 Excel 文件列表
        /// </summary>
        public void RefreshExcelList()
        {
            _excelItems.Clear();

            // 扫描 Packages 目录下的 Excel 文件夹
            string packagesPath = _pathConfig.PackagesPath;

            if (Directory.Exists(packagesPath))
            {
                try
                {
                    var directories = Directory.GetDirectories(packagesPath);

                    foreach (var dir in directories)
                    {
                        string excelDir = Path.Combine(dir, "Excel");

                        if (Directory.Exists(excelDir))
                        {
                            ScanExcelDirectory(excelDir, dir);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"搜索Excel文件失败: {ex.Message}");
                }
            }

            // 排序
            _excelItems.Sort((a, b) => string.Compare(a.FileName, b.FileName, StringComparison.Ordinal));
        }

        /// <summary>
        /// 扫描指定的 Excel 目录
        /// </summary>
        private void ScanExcelDirectory(string excelDir, string packageDir)
        {
            try
            {
                var files = Directory.GetFiles(excelDir, "*.xlsx", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    string fileName = Path.GetFileName(filePath);

                    // 跳过临时文件
                    if (fileName.StartsWith("~$") || fileName.Contains("#"))
                        continue;

                    string relativeDir = GetRelativeDirectory(excelDir, filePath);
                    string packagePath = _pathConfig.ProjectRootPath;
                    string fullPath = filePath;

                    _excelItems.Add(new ExcelItemModel(fileName, relativeDir, fullPath, packageDir));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"扫描目录失败 {excelDir}: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取相对目录
        /// </summary>
        private string GetRelativeDirectory(string rootDir, string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            string relativePath = Path.GetRelativePath(rootDir, dir);

            // 如果是根目录，返回 "."
            return string.IsNullOrEmpty(relativePath) ? "." : relativePath;
        }

        /// <summary>
        /// 执行 Excel 导出
        /// </summary>
        public void ExecuteExcelExport()
        {
            ExecuteExcelExportInternal(_pathConfig);
        }

        /// <summary>
        /// 静态方法：执行 Excel 导出
        /// </summary>
        public static void ExecuteExcelExportStatic()
        {
            var pathConfig = new PathConfig();
            ExecuteExcelExportInternal(pathConfig);
        }

        /// <summary>
        /// 内部实现：执行 Excel 导出
        /// </summary>
        private static void ExecuteExcelExportInternal(PathConfig pathConfig)
        {
            var exporterDllPath = pathConfig.GetExporterDllPath();
            UnityEngine.Debug.Log($"[Excel] 导出器路径: {exporterDllPath}");
            // 检查 Exporter 是否存在
            if (!System.IO.File.Exists(exporterDllPath))
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "导出器未编译",
                    $"找不到导出器 DLL 文件：{exporterDllPath}\n\n请先编译 Exporter 项目：\n{System.IO.Path.Combine(pathConfig.PackagePath, PathConfig.DotNetDirName, "RD.ExcelExporter")}",
                    "确定"
                );
                return;
            }

            UnityEngine.Debug.Log("[Excel] ========== 开始导出 Excel ==========");
            var workingDir = pathConfig.GetWorkingDirectory();
            UnityEngine.Debug.Log($"[Excel] 工作目录: {workingDir}");

            System.Diagnostics.Process process = ProcessHelper.DotNet(
                exporterDllPath,
                workingDir, true);

            string output = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log(output);
            UnityEngine.Debug.Log("[Excel] ========== 导出 Excel 完成 ==========");
        }
    }
}