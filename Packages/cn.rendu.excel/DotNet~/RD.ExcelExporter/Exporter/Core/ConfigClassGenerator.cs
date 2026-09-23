using System;
using System.IO;
using System.Text;

namespace RD
{
    /// <summary>
    /// 配置类生成器 - 使用模板生成 Category 类和 Config 类
    /// Category 类生成到 CodeMode/Model 目录，Config 类也生成到 CodeMode/Model 目录
    /// </summary>
    public static class ConfigClassGenerator
    {
        /// <summary>
        /// 生成配置类（使用模板生成 Category 类和 Config 类）
        /// </summary>
        public static void GenerateConfigClass(Table table, ConfigType configType, string excelRootDir, PathProvider pathProvider, string template)
        {
            var strategy = ConfigTypeStrategyFactory.GetStrategy(configType);

            // 生成字段字符串
            string fields = GenerateFields(table, configType);

            // 使用模板生成 Category 类和 Config 类
            GenerateFromTemplate(table, configType, excelRootDir, pathProvider, template, fields);
        }

        /// <summary>
        /// 生成字段定义字符串
        /// </summary>
        private static string GenerateFields(Table table, ConfigType configType)
        {
            var sb = new StringBuilder();
            foreach ((string _, HeadInfo headInfo) in table.HeadInfos)
            {
                if (headInfo == null)
                    continue;

                var strategy = ConfigTypeStrategyFactory.GetStrategy(configType);
                if (configType != ConfigType.cs && !headInfo.FieldCS.Contains(strategy.Type.ToString()))
                    continue;

                sb.AppendLine($"\t\t/// <summary>{headInfo.FieldDesc}</summary>");
                sb.AppendLine($"\t\tpublic {headInfo.FieldType} {headInfo.FieldName} {{ get; set; }}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 使用模板生成 Category 类和 Config 类
        /// </summary>
        private static void GenerateFromTemplate(Table table, ConfigType configType, string excelRootDir, PathProvider pathProvider, string template, string fields)
        {
            string dir = pathProvider.GetConfigFileDir(excelRootDir, configType);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string exportPath = Path.Combine(dir, $"{table.Name}.cs");

            Console.WriteLine($"[ExcelExporter] 生成配置类: {exportPath} ({configType})");

            // 替换模板中的占位符
            string content = template.Replace("(ns)", "RD")
                                      .Replace("(ConfigName)", table.Name)
                                      .Replace("(Fields)", fields);

            File.WriteAllText(exportPath, content);
        }
    }
}
