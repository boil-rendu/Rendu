using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// JSON 导出器
    /// </summary>
    public class JsonExporter : IDataExporter
    {
        private readonly PathProvider _pathProvider;

        public JsonExporter(PathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        /// <summary>
        /// 导出数据为 JSON 格式
        /// </summary>
        public void Export(ExcelPackage package, ExcelFileInfo excelFile, Table table, string excelRootDir)
        {
            // 根据表的配置类型决定导出到哪些目录
            // cs配置 → cs/、s/、c/ (所有平台)
            // s配置 → s/、cs/ (服务器+共享)
            // c配置 → c/、cs/ (客户端+共享)
            var exportTypes = GetExportTypes(table);
            foreach (var configType in exportTypes)
            {
                ExportForConfigType(package, excelFile, table, excelRootDir, configType);
            }
        }

        /// <summary>
        /// 根据表的配置标识获取需要导出的配置类型列表
        /// cs配置 → cs/、s/、c/ (所有平台)
        /// s配置 → s/、cs/ (服务器+共享)
        /// c配置 → c/、cs/ (客户端+共享)
        /// </summary>
        private List<ConfigType> GetExportTypes(Table table)
        {
            var types = new List<ConfigType>();

            if (table.C && table.S)
            {
                // cs配置：导出到所有平台
                types.Add(ConfigType.cs);
                types.Add(ConfigType.s);
                types.Add(ConfigType.c);
            }
            else if (table.S)
            {
                // s配置：导出到服务器和共享
                types.Add(ConfigType.s);
                types.Add(ConfigType.cs);
            }
            else if (table.C)
            {
                // c配置：导出到客户端和共享
                types.Add(ConfigType.c);
                types.Add(ConfigType.cs);
            }
            else
            {
                // 默认：共享
                types.Add(ConfigType.cs);
            }

            return types;
        }

        /// <summary>
        /// 为指定配置类型导出JSON
        /// </summary>
        private void ExportForConfigType(ExcelPackage package, ExcelFileInfo excelFile, Table table, string excelRootDir, ConfigType configType)
        {
            var sb = new StringBuilder();
            sb.Append("{\"dict\": [\n");

            bool hasData = false;
            foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                    continue;

                if (ExportSheetToJson(worksheet, excelFile.FileNameWithoutExtension, table, sb, configType))
                {
                    hasData = true;
                }
            }

            sb.Append("]}\n");

            // 如果该配置类型没有数据，跳过导出
            if (!hasData)
                return;

            string outputDir = _pathProvider.GetOutputDir(excelRootDir, configType, excelFile.RelativeDir, OutputType.Json);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string jsonPath = Path.Combine(outputDir, $"{excelFile.FileNameWithoutExtension}.txt");
            Console.WriteLine($"[ExcelExporter] 导出 JSON ({configType}): {jsonPath}");
            File.WriteAllText(jsonPath, sb.ToString());
        }

        /// <summary>
        /// 将工作表导出为 JSON
        /// </summary>
        /// <returns>是否导出了数据</returns>
        private bool ExportSheetToJson(ExcelWorksheet worksheet, string name, Table table, StringBuilder sb, ConfigType configType)
        {
            bool hasData = false;
            for (int row = 6; row <= worksheet.Dimension.End.Row; row++)
            {
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#"))
                    continue;

                if (string.IsNullOrEmpty(prefix))
                    prefix = "cs";

                // 检查该行是否包含当前配置类型
                if (!prefix.Contains(configType.ToString().ToLower()))
                    continue;

                if (string.IsNullOrEmpty(worksheet.Cells[row, 3].Text.Trim()))
                    continue;

                hasData = true;
                sb.Append($"[{worksheet.Cells[row, 3].Text.Trim()}, {{\"_t\":\"{name}\"");

                for (int col = 3; col <= worksheet.Dimension.End.Column; col++)
                {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!table.HeadInfos.ContainsKey(fieldName))
                        continue;

                    HeadInfo headInfo = table.HeadInfos[fieldName];
                    if (headInfo == null)
                        continue;

                    // 检查字段是否包含当前配置类型
                    if (!headInfo.FieldCS.Contains(configType.ToString().ToLower()))
                        continue;

                    string fieldN = headInfo.FieldName;
                    if (fieldN == "Id")
                        fieldN = "_id";

                    sb.Append($",\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }

                sb.Append("}],\n");
            }
            return hasData;
        }

        /// <summary>
        /// 将值转换为 JSON 格式
        /// </summary>
        private string Convert(string type, string value)
        {
            return ValueConverter.Convert(type, value);
        }
    }
}
