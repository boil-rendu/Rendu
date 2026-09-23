using System;
using System.IO;

namespace RD
{
    /// <summary>
    /// 路径提供器 - 统一管理所有路径
    /// </summary>
    public class PathProvider
    {
        private readonly string solutionPath;

        public PathProvider(string solutionPath)
        {
            this.solutionPath = solutionPath;
        }

        public string SolutionPath => solutionPath;

        /// <summary>
        /// 获取生成的配置类文件目录
        /// 生成的文件包含 Category 类和 Config 类,统一放在 CodeMode/Model 目录
        /// </summary>
        public string GetConfigFileDir(string excelRootDir, ConfigType configType)
        {
            // 生成的配置类文件导出到 CodeMode/Model 目录
            string baseDir = Path.Combine(solutionPath, "Packages/cn.rendu.excel/CodeMode/Model");
            string subDir = configType switch
            {
                ConfigType.c => "Client",
                ConfigType.s => "Server",
                ConfigType.cs => "ClientServer",
                _ => "ClientServer"
            };
            return Path.GetFullPath(Path.Combine(baseDir, subDir));
        }

        public string GetOutputDir(string excelRootDir, ConfigType configType, string relativeDir, OutputType outputType)
        {
            // 统一导出到 RD.Excel 包的 Config 目录下
            string baseDir = Path.Combine(solutionPath, "Packages/cn.rendu.excel/Config");
            string subDir = outputType switch
            {
                OutputType.Json => "Json",
                OutputType.Bytes => "Bytes",
                _ => throw new ArgumentOutOfRangeException(nameof(outputType), outputType, null)
            };
            // 将ConfigType枚举转换为目录名称
            string configTypeDir = configType switch
            {
                ConfigType.c => "Client",
                ConfigType.s => "Server",
                ConfigType.cs => "ClientServer",
                _ => "ClientServer"
            };
            string configDir = Path.GetFullPath(Path.Combine(baseDir, subDir, configTypeDir));
            return Path.Combine(configDir, relativeDir);
        }

        public string GetTemplatePath()
        {
            // 模板由 csproj 的 CopyToOutputDirectory 复制到输出目录，避免依赖 SolutionDir 与工程相对路径
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Template.txt");
            if (File.Exists(templatePath))
                return templatePath;

            // 兼容旧布局：模板位于 DotNet~ 根目录
            string legacyPath = Path.Combine(solutionPath, "Packages/cn.rendu.excel/DotNet~/Template.txt");
            if (File.Exists(legacyPath))
                return legacyPath;

            throw new FileNotFoundException($"找不到模板文件 Template.txt，查找路径: {templatePath}、{legacyPath}");
        }

        /// <summary>
        /// 静态方法：获取生成的配置类文件目录（用于枚举和常量导出）
        /// </summary>
        public static string GetConfigFileDirStatic(string excelRootDir, ConfigType configType)
        {
            string solutionPath = GetSolutionPathStatic();
            string baseDir = Path.Combine(solutionPath, "Packages/cn.rendu.excel/CodeMode/Model");
            string subDir = configType switch
            {
                ConfigType.c => "Client",
                ConfigType.s => "Server",
                ConfigType.cs => "ClientServer",
                _ => "ClientServer"
            };
            return Path.GetFullPath(Path.Combine(baseDir, subDir));
        }

        private static string GetSolutionPathStatic()
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
    }

    public enum OutputType
    {
        Json,
        Bytes
    }
}
