using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// 枚举和常量导出器
    /// </summary>
    public static class EnumAndConstExporter
    {
        /// <summary>
        /// 导出枚举类
        /// </summary>
        public static void ExportEnumClass(ExcelWorksheet worksheet, string excelRootDir)
        {
            const int row = 2;
            List<string> listEnums = new List<string>();

            for (int col = 3; col <= worksheet.Dimension.End.Column; col++)
            {
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (string.IsNullOrEmpty(fieldName))
                    continue;

                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains('#'))
                    continue;

                if (string.IsNullOrEmpty(fieldCS))
                    fieldCS = "cs";

                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                if (fieldType.ToLower() == "enum")
                {
                    for (int i = 0; i < 999999; i++)
                    {
                        string name = worksheet.Cells[row + 4 + i, col].Text.Trim();
                        if (string.IsNullOrEmpty(name)) break;

                        string desc = worksheet.Cells[row + 4 + i, col - 1].Text.Trim();
                        string val = worksheet.Cells[row + 4 + i, col + 1].Text.Trim();

                        if (string.IsNullOrEmpty(val))
                            val = ",";
                        else
                        {
                            val = $" = {val},";
                        }

                        listEnums.Add($"        /// <summary>{desc}</summary>\n        {name}{val}\n");
                    }
                }
            }

            string cs = worksheet.Cells[1, 1].Text.Trim();
            List<ConfigType> listTypes = new List<ConfigType>() { ConfigType.cs, ConfigType.c, ConfigType.s };
            if (cs == "c") listTypes.Remove(ConfigType.s);
            else if (cs == "s") listTypes.Remove(ConfigType.c);

            foreach (var configType in listTypes)
            {
                string dir = PathProvider.GetConfigFileDirStatic(excelRootDir, configType);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string ename = worksheet.Name.Substring(6);
                string exportPath = System.IO.Path.Combine(dir, $"{ename}.cs");

                using FileStream txt = new FileStream(exportPath, FileMode.Create);
                using StreamWriter sw = new StreamWriter(txt);

                sw.WriteLine("namespace RD");
                sw.WriteLine("{");
                sw.WriteLine($"    public enum {ename}");
                sw.WriteLine("    {");
                for (int i = 0; i < listEnums.Count; i++)
                {
                    sw.WriteLine(listEnums[i]);
                }
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }
        }

        /// <summary>
        /// 导出常量类
        /// </summary>
        public static void ExportConstClass(ExcelWorksheet worksheet, string excelRootDir)
        {
            const int row = 2;
            List<string> listConst = new List<string>();

            for (int col = 3; col <= worksheet.Dimension.End.Column; col++)
            {
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (string.IsNullOrEmpty(fieldName))
                    continue;

                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains('#'))
                    continue;

                if (string.IsNullOrEmpty(fieldCS))
                    fieldCS = "cs";

                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                if (fieldType.ToLower() == "const")
                {
                    string constType = worksheet.Cells[row + 3, col + 1].Text.Trim();

                    bool isStatic = constType.Contains("[]");

                    for (int i = 0; i < 999999; i++)
                    {
                        string name = worksheet.Cells[row + 4 + i, col].Text.Trim();
                        if (string.IsNullOrEmpty(name)) break;

                        string desc = worksheet.Cells[row + 4 + i, col - 1].Text.Trim();
                        string val = worksheet.Cells[row + 4 + i, col + 1].Text.Trim();

                        if (isStatic)
                        {
                            val = "{" + val + "}";
                            listConst.Add($"        /// <summary>{desc}</summary>\n        [StaticField]\n        public static readonly {constType} {name} = {val}; \n");
                        }
                        else
                        {
                            listConst.Add($"        /// <summary>{desc}</summary>\n        public const {constType} {name} = {ConvertForConst(constType, val)}; \n");
                        }
                    }
                }
            }

            string cs = worksheet.Cells[1, 1].Text.Trim();

            List<ConfigType> listTypes = new List<ConfigType>() { ConfigType.cs, ConfigType.c, ConfigType.s };
            if (cs == "c")
            {
                listTypes.Remove(ConfigType.s);
            }
            else if (cs == "s")
            {
                listTypes.Remove(ConfigType.c);
            }

            foreach (var configType in listTypes)
            {
                string dir = PathProvider.GetConfigFileDirStatic(excelRootDir, configType);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string ename = worksheet.Name.Substring(7);
                string exportPath = System.IO.Path.Combine(dir, $"{ename}.cs");

                using FileStream txt = new FileStream(exportPath, FileMode.Create);
                using StreamWriter sw = new StreamWriter(txt);

                sw.WriteLine("namespace RD");
                sw.WriteLine("{");
                sw.WriteLine($"    public static partial class {ename}");
                sw.WriteLine("    {");
                for (int i = 0; i < listConst.Count; i++)
                {
                    sw.WriteLine(listConst[i]);
                }
                sw.WriteLine("    }");
                sw.WriteLine("}");
            }
        }

        private static string ConvertForConst(string type, string value)
        {
            return ValueConverter.Convert(type, value);
        }
    }
}
