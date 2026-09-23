using System;
using System.Text;
using RD.Core;

namespace RD
{
    /// <summary>
    /// Protocol 导出器 - 主入口类
    /// 遵循单一职责原则，协调整个导出流程
    /// </summary>
    public static class InnerProtocolExporter
    {
        private static PathProvider _pathProvider;
        private static OpcodeRegistry _opcodeRegistry;

        static InnerProtocolExporter()
        {
            // 设置控制台输出编码为 UTF-8,避免 Unity Console 中中文乱码
            Console.OutputEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// 执行 Proto 导出
        /// </summary>
        public static void Proto2CS()
        {
            try
            {
                Initialize();
                ScanAndParseProtos();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[InnerProtocolExporter] 导出失败: {e}");
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
            Console.WriteLine($"[InnerProtocolExporter] ========== 开始 Proto 导出 ==========");

            // 强制调用 MongoDB，避免库被裁剪
            MongoHelper.ToJson(1);

            string solutionPath = GetSolutionPath();
            PackagesLock packagesLock = PackageHelper.LoadRDPackagesLock(solutionPath);
            PackageInfo protoPackage = packagesLock.dependencies["cn.rendu.protocol"];

            // 初始化组件
            _pathProvider = new PathProvider(solutionPath, protoPackage.dir);
            _opcodeRegistry = new OpcodeRegistry();
        }

        /// <summary>
        /// 扫描并解析所有 Proto 文件
        /// </summary>
        private static void ScanAndParseProtos()
        {
            PackagesLock packagesLock = PackageHelper.LoadRDPackagesLock(_pathProvider.SolutionPath);
            var scanner = new ProtoFileScanner(_pathProvider, packagesLock);
            var protoFiles = scanner.Scan();

            var parser = new ProtoParser(_opcodeRegistry);

            foreach (var protoFile in protoFiles)
            {
                ProcessProtoFile(protoFile, parser);
            }
        }

        /// <summary>
        /// 处理单个 Proto 文件
        /// </summary>
        private static void ProcessProtoFile(ProtoFileInfo protoFile, ProtoParser parser)
        {
            protoFile.ParseFileName(out string protoName, out string csFlag, out int startOpcode);

            Console.WriteLine($"[InnerProtocolExporter] 解析 Proto: {protoFile.FilePath} -> {protoName} (CS:{csFlag})");

            // 清空当前文件的 opcode 注册
            _opcodeRegistry.Clear();

            // 读取并解析文件内容
            string protoContent = System.IO.File.ReadAllText(protoFile.FilePath);
            var parseResult = parser.Parse(protoContent, protoName, startOpcode);

            // 格式化代码
            parseResult.CodeBuilder.FormatCode();

            // 根据文件名获取生成策略
            IOutputStrategy strategy = OutputStrategyFactory.GetStrategy(protoFile.FileNameWithoutExtension);

            // 生成 CS 文件
            strategy.Generate(parseResult.CodeBuilder.ToString(), _opcodeRegistry, protoFile.FileName, _pathProvider, protoName);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private static void Cleanup()
        {
            _opcodeRegistry?.Clear();
            Console.WriteLine($"[InnerProtocolExporter] ========== Proto 导出完成 ==========");
        }

        /// <summary>
        /// 获取解决方案路径
        /// </summary>
        private static string GetSolutionPath()
        {
            string directory = System.IO.Directory.GetCurrentDirectory();
            while (directory != null)
            {
                string[] slnFiles = System.IO.Directory.GetFiles(directory, "*.sln");
                if (slnFiles.Length > 0)
                    return directory;

                directory = System.IO.Directory.GetParent(directory)?.FullName;
            }
            throw new Exception("找不到解决方案目录");
        }
    }
}
