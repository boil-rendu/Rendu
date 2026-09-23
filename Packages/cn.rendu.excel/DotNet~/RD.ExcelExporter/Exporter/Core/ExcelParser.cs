using System;
using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// Excel 解析器
    /// </summary>
    public static class ExcelParser
    {
        /// <summary>
        /// 解析单个 Excel 文件
        /// </summary>
        public static void ParseExcelFile(ExcelFileInfo excelFile, TableRegistry tableRegistry, ExcelPackageCache packageCache)
        {
            string protoName = FileNameHelper.ExtractProtoName(excelFile.FileName);
            var table = tableRegistry.GetOrCreate(protoName);
            ConfigType configType = FileNameHelper.ExtractConfigType(excelFile.FileName);

            UpdateTableFlags(table, configType);

            Console.WriteLine($"[ExcelExporter] 解析 Excel: {excelFile.FilePath} -> {protoName} (C:{table.C} S:{table.S})");

            var package = packageCache.GetPackage(excelFile.FilePath);
            ParseExcelSheets(package, table, excelFile.RootDir);
        }

        /// <summary>
        /// 解析 Excel 工作表
        /// </summary>
        private static void ParseExcelSheets(ExcelPackage package, Table table, string excelRootDir)
        {
            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                string sheetName = worksheet.Name.ToLower();

                if (sheetName.StartsWith("#const_"))
                {
                    EnumAndConstExporter.ExportConstClass(worksheet, excelRootDir);
                    continue;
                }

                if (sheetName.StartsWith("#enum_"))
                {
                    EnumAndConstExporter.ExportEnumClass(worksheet, excelRootDir);
                    continue;
                }

                ParseSheetFields(worksheet, table);
            }
        }

        /// <summary>
        /// 解析工作表字段
        /// </summary>
        private static void ParseSheetFields(ExcelWorksheet worksheet, Table table)
        {
            const int headerRow = 2;

            for (int col = 3; col <= worksheet.Dimension.End.Column; col++)
            {
                string fieldName = worksheet.Cells[headerRow + 2, col].Text.Trim();
                if (string.IsNullOrEmpty(fieldName))
                    continue;

                if (table.HeadInfos.ContainsKey(fieldName))
                    continue;

                string fieldCS = worksheet.Cells[headerRow, col].Text.Trim().ToLower();
                if (fieldCS.Contains("#"))
                {
                    table.HeadInfos[fieldName] = null;
                    continue;
                }

                if (string.IsNullOrEmpty(fieldCS))
                    fieldCS = "cs";

                string fieldDesc = worksheet.Cells[headerRow + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[headerRow + 3, col].Text.Trim();

                table.HeadInfos[fieldName] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType, ++table.Index);
            }
        }

        /// <summary>
        /// 更新表的 C/S 标志
        /// </summary>
        private static void UpdateTableFlags(Table table, ConfigType configType)
        {
            if (configType == ConfigType.c)
                table.C = true;
            else if (configType == ConfigType.s)
                table.S = true;
            else if (configType == ConfigType.cs)
            {
                // cs配置：同时设置C和S标志
                table.C = true;
                table.S = true;
            }
        }
    }
}
