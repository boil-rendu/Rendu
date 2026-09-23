using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using OfficeOpenXml;

namespace RD
{
    /// <summary>
    /// Protobuf 导出器
    /// </summary>
    public class ProtobufExporter : IDataExporter
    {
        private readonly PathProvider _pathProvider;
        private readonly AssemblyProvider _assemblyProvider;

        public ProtobufExporter(PathProvider pathProvider, AssemblyProvider assemblyProvider)
        {
            _pathProvider = pathProvider;
            _assemblyProvider = assemblyProvider;
        }

        /// <summary>
        /// 导出数据为 Protobuf 格式
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
                ExportToProtobuf(configType, table, excelFile.RelativeDir, excelRootDir, excelFile.FileNameWithoutExtension);
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
        /// 将数据导出为指定配置类型的 Protobuf 格式
        /// </summary>
        private void ExportToProtobuf(ConfigType configType, Table table, string relativeDir, string excelRootDir, string fileNameWithoutExtension)
        {
            string outputDir = _pathProvider.GetOutputDir(excelRootDir, configType, relativeDir, OutputType.Bytes);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            Assembly ass = _assemblyProvider.GetAssembly(excelRootDir, configType);

            // 调试:打印程序集中包含的类型
            if (ass == null)
            {
                Console.WriteLine($"[ExcelExporter] 错误: 程序集为空 (excelRootDir={excelRootDir}, configType={configType})");
                return;
            }
            // else
            // {
            //     Console.WriteLine($"[ExcelExporter] 程序集 {ass.FullName} 包含的类型:");
            //     foreach (Type t in ass.GetTypes())
            //     {
            //         Console.WriteLine($"  - {t.FullName}");
            //     }
            // }

            Type type = ass?.GetType($"RD.{table.Name}Category");

            if (type == null)
            {
                Console.WriteLine($"[ExcelExporter] 警告: 未找到类型 RD.{table.Name}Category，跳过导出");
                return;
            }

            IMerge final = Activator.CreateInstance(type) as IMerge;

            string jsonDir = _pathProvider.GetOutputDir(excelRootDir, configType, relativeDir, OutputType.Json);
            string jsonPath = Path.Combine(jsonDir, $"{fileNameWithoutExtension}.txt");

            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                try
                {
                    object deserialize = BsonSerializer.Deserialize(json, type);
                    final.Merge(deserialize);
                }
                catch (Exception e)
                {
                    throw new Exception($"json : {jsonPath} error", e);
                }
            }
            else
            {
                Console.WriteLine($"[ExcelExporter] 警告: JSON 文件不存在: {jsonPath}，跳过 Protobuf 导出");
                return;
            }

            string bytesPath = Path.Combine(outputDir, $"{table.Name}Category.bytes");
            Console.WriteLine($"[ExcelExporter] 导出 Protobuf: {bytesPath} ({configType})");

            using FileStream file = File.Create(bytesPath);
            file.Write(final.ToBson());
        }
    }
}
