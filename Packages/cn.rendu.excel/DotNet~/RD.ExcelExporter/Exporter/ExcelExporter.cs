using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using OfficeOpenXml;
using RD.Core;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace RD
{
    [EnableClass]
    public static class ExcelExporter
    {
        private static PathProvider _pathProvider;
        private static TableRegistry _tableRegistry;
        private static ExcelPackageCache _packageCache;
        private static AssemblyProvider _assemblyProvider;
        private static string _template;

        static ExcelExporter()
        {
            Console.OutputEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// 执行 Excel 导出
        /// </summary>
        public static void Export()
        {
            try
            {
                Initialize();
                ScanAndParseExcels();
                GenerateAndCompileConfigClasses();
                ExportData();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ExcelExporter] 导出失败: {e}");
                Console.WriteLine(e.ToString());
            }
            finally
            {
                Cleanup();
            }
        }

        /// <summary>
        /// 初始化导出所需的资源
        /// </summary>
        private static void Initialize()
        {
            Console.WriteLine($"[ExcelExporter] ========== 开始 Excel 导出 ==========");

            MongoHelper.ToJson(1);

            string solutionPath = GetSolutionPath();

            _pathProvider = new PathProvider(solutionPath);
            _tableRegistry = new TableRegistry();
            _packageCache = new ExcelPackageCache();
            _assemblyProvider = new AssemblyProvider();

            _template = File.ReadAllText(_pathProvider.GetTemplatePath());
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// 扫描并解析所有 Excel 文件
        /// </summary>
        private static void ScanAndParseExcels()
        {
            PackagesLock packagesLock = PackageHelper.LoadRDPackagesLock(GetSolutionPath());
            var scanner = new ExcelFileScanner(GetSolutionPath(), packagesLock);
            var excelFiles = scanner.Scan();

            foreach (var excelFile in excelFiles)
            {
                ExcelParser.ParseExcelFile(excelFile, _tableRegistry, _packageCache);
            }
        }

        /// <summary>
        /// 生成并编译配置类
        /// </summary>
        private static void GenerateAndCompileConfigClasses()
        {
            Console.WriteLine($"[ExcelExporter] 开始生成配置类代码，共 {_tableRegistry.GetAllTables().Count()} 个表");

            PackagesLock packagesLock = PackageHelper.LoadRDPackagesLock(GetSolutionPath());
            var scanner = new ExcelFileScanner(GetSolutionPath(), packagesLock);
            var excelFiles = scanner.Scan();

            var groupedByRoot = excelFiles.GroupBy(f => f.RootDir);

            foreach (var group in groupedByRoot)
            {
                string excelRootDir = group.Key;

                CleanConfigDirectories(excelRootDir);

                foreach (var table in _tableRegistry.GetAllTables())
                {
                    foreach (ConfigType configType in Enum.GetValues(typeof(ConfigType)))
                    {
                        var strategy = ConfigTypeStrategyFactory.GetStrategy(configType);
                        if (strategy.ShouldGenerate(table))
                        {
                            ConfigClassGenerator.GenerateConfigClass(table, configType, excelRootDir, _pathProvider, _template);
                        }
                    }
                }

                Console.WriteLine($"[ExcelExporter] 开始动态编译配置代码: {excelRootDir}");
                var compiler = new AssemblyCompiler(GetSolutionPath());

                string clientDir = _pathProvider.GetConfigFileDir(excelRootDir, ConfigType.c);
                string serverDir = _pathProvider.GetConfigFileDir(excelRootDir, ConfigType.s);
                string csDir = _pathProvider.GetConfigFileDir(excelRootDir, ConfigType.cs);

                if (Directory.Exists(clientDir))
                    _assemblyProvider.SetAssembly(excelRootDir, ConfigType.c, compiler.Compile(clientDir, ConfigType.c));
                if (Directory.Exists(serverDir))
                    _assemblyProvider.SetAssembly(excelRootDir, ConfigType.s, compiler.Compile(serverDir, ConfigType.s));
                if (Directory.Exists(csDir))
                    _assemblyProvider.SetAssembly(excelRootDir, ConfigType.cs, compiler.Compile(csDir, ConfigType.cs));

                Console.WriteLine($"[ExcelExporter] 动态编译完成: {excelRootDir}");
            }
        }

        /// <summary>
        /// 导出数据（JSON 和 Protobuf）
        /// </summary>
        private static void ExportData()
        {
            Console.WriteLine($"[ExcelExporter] 开始导出数据（JSON + Protobuf）");

            PackagesLock packagesLock = PackageHelper.LoadRDPackagesLock(GetSolutionPath());
            var scanner = new ExcelFileScanner(GetSolutionPath(), packagesLock);
            var excelFiles = scanner.Scan();

            var jsonExporter = ExporterFactory.CreateJsonExporter(_pathProvider);
            var protobufExporter = ExporterFactory.CreateProtobufExporter(_pathProvider, _assemblyProvider);

            foreach (var excelFile in excelFiles)
            {
                string protoName = FileNameHelper.ExtractProtoName(excelFile.FileName);
                Table table = _tableRegistry.GetOrCreate(protoName);

                var package = _packageCache.GetPackage(excelFile.FilePath);

                jsonExporter.Export(package, excelFile, table, excelFile.RootDir);
                protobufExporter.Export(package, excelFile, table, excelFile.RootDir);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private static void Cleanup()
        {
            _tableRegistry?.Clear();
            _packageCache?.Dispose();
            Console.WriteLine($"[ExcelExporter] ========== Excel 导出完成 ==========");
        }

        #region 辅助方法

        /// <summary>
        /// 获取解决方案路径
        /// </summary>
        private static string GetSolutionPath()
        {
            string directory = Directory.GetCurrentDirectory();
            while (directory != null)
            {
                string[] slnFiles = Directory.GetFiles(directory, "*.sln");
                if (slnFiles.Length > 0)
                    return directory;

                directory = Directory.GetParent(directory)?.FullName;
            }
            throw new Exception("找不到解决方案目录");
        }

        /// <summary>
        /// 清理配置目录
        /// </summary>
        private static void CleanConfigDirectories(string excelRootDir)
        {
            Console.WriteLine($"[ExcelExporter] 清理配置目录, excelRootDir: {excelRootDir}");

            // 只清理CodeMode目录下生成的配置实体类文件
            // 不清理Scripts目录,因为那里是工具类文件
            foreach (ConfigType configType in Enum.GetValues(typeof(ConfigType)))
            {
                string entityDir = _pathProvider.GetConfigFileDir(excelRootDir, configType);
                Console.WriteLine($"[ExcelExporter] 配置类型: {configType}, 清理实体目录: {entityDir}");

                if (Directory.Exists(entityDir))
                {
                    // 只删除直接目录下的.cs文件,不递归到子目录
                    var csFiles = Directory.GetFiles(entityDir, "*.cs", SearchOption.TopDirectoryOnly);
                    Console.WriteLine($"[ExcelExporter] 找到 {csFiles.Length} 个实体.cs文件待删除");
                    foreach (string csFile in csFiles)
                    {
                        Console.WriteLine($"[ExcelExporter] 删除文件: {csFile}");
                        File.Delete(csFile);
                    }
                }
                else
                {
                    Console.WriteLine($"[ExcelExporter] 实体目录不存在: {entityDir}");
                }
            }

            // 清理并重新创建Config目录结构
            string configBaseDir = Path.Combine(_pathProvider.SolutionPath, "Packages/cn.rendu.excel/Config");
            if (Directory.Exists(configBaseDir))
            {
                Console.WriteLine($"[ExcelExporter] 删除配置目录: {configBaseDir}");
                Directory.Delete(configBaseDir, true);
            }

            // 创建必要的目录结构(c、s、cs)
            CreateConfigDirectories();
        }

        /// <summary>
        /// 创建Config目录结构
        /// </summary>
        private static void CreateConfigDirectories()
        {
            string configBaseDir = Path.Combine(_pathProvider.SolutionPath, "Packages/cn.rendu.excel/Config");

            // 创建 Json 和 Bytes 子目录
            foreach (string outputType in new[] { "Json", "Bytes" })
            {
                string outputDir = Path.Combine(configBaseDir, outputType);

                // 为每种配置类型创建目录
                foreach (ConfigType configType in Enum.GetValues(typeof(ConfigType)))
                {
                    // 将ConfigType枚举转换为目录名称
                    string configTypeDir = configType switch
                    {
                        ConfigType.c => "Client",
                        ConfigType.s => "Server",
                        ConfigType.cs => "ClientServer",
                        _ => "ClientServer"
                    };

                    string typeDir = Path.Combine(outputDir, configTypeDir);
                    if (!Directory.Exists(typeDir))
                    {
                        Directory.CreateDirectory(typeDir);
                        Console.WriteLine($"[ExcelExporter] 创建目录: {typeDir}");
                    }
                }
            }
        }
        #endregion
    }
}
