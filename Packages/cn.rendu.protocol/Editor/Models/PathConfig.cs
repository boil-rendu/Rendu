using System.IO;

namespace RD.Protocol.Editor
{
    /// <summary>
    /// 导出路径配置
    /// </summary>
    public class PathConfig
    {
        /// <summary>
        ///  包名称
        /// </summary>
        public const string PackageName = "cn.rendu.protocol";

        /// <summary>
        /// DotNet 目录名称
        /// </summary>
        public const string DotNetDirName = "DotNet~";

        /// <summary>
        /// 导出器程序集名称
        /// </summary>
        public const string ExporterAssemblyName = "RD.ProtocolExporter.dll";

        /// <summary>
        /// 导出器输出目录
        /// </summary>
        public const string ExporterOutputDir = "Bin";

        /// <summary>
        /// 获取项目根目录路径
        /// </summary>
        public string ProjectRootPath => Directory.GetCurrentDirectory();

        /// <summary>
        /// 获取 导出器 DLL 的完整路径
        /// </summary>
        public string GetExporterDllPath()
        {
            return Path.Combine(
                ProjectRootPath,
                ExporterOutputDir,
                ExporterAssemblyName
            );
        }

        /// <summary>
        /// 获取工作目录路径（项目根目录）
        /// </summary>
        public string GetWorkingDirectory() => ProjectRootPath;

        /// <summary>
        /// 获取 Packages 目录路径
        /// </summary>
        public string PackagesPath => Path.Combine(ProjectRootPath, "Packages");

        /// <summary>
        /// 获取  包路径
        /// </summary>
        public string PackagePath => Path.Combine(PackagesPath, PackageName);
    }
}
