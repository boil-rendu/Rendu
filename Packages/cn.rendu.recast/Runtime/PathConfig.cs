using System.IO;

namespace RD.Recast
{
    /// <summary>
    /// 导出路径配置
    /// </summary>
    public class PathConfig
    {
        /// <summary>
        ///  包名称
        /// </summary>
        public const string PackageName = "cn.rendu.recast";

        /// <summary>
        /// DotNet 目录名称
        /// </summary>
        public const string DotNetDirName = "DotNet~";
        
        /// <summary>
        /// 获取项目根目录路径
        /// </summary>
        public string ProjectRootPath => Directory.GetCurrentDirectory();
        
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

        /// <summary>
        /// 获取 Recast 导出路径（包内 Config/Recast 目录）
        /// </summary>
        public string RecastExportPath => Path.Combine(PackagePath, "Config", "Recast");

    }
}
