using System;
using System.IO;

namespace RD
{
    /// <summary>
    /// 路径提供器 - 统一管理 Protocol 导出的所有路径
    /// </summary>
    public class PathProvider
    {
        private readonly string solutionPath;
        private readonly string protocolPackageDir;

        public PathProvider(string solutionPath, string protocolPackageDir)
        {
            this.solutionPath = solutionPath;
            this.protocolPackageDir = protocolPackageDir;
        }

        public string SolutionPath => solutionPath;

        public string GetClientMessagePath()
        {
            // 统一到 RD.Protocol 包的 CodeMode/Model/Client
            return Path.Combine(solutionPath, "Packages/cn.rendu.protocol/CodeMode/Model/Client");
        }

        public string GetServerMessagePath()
        {
            // 统一到 RD.Protocol 包的 CodeMode/Model/Server
            return Path.Combine(solutionPath, "Packages/cn.rendu.protocol/CodeMode/Model/Server");
        }

        public string GetClientServerMessagePath()
        {
            // 统一到 RD.Protocol 包的 CodeMode/Model/ClientServer（统一命名，与 Excel 导出保持一致）
            return Path.Combine(solutionPath, "Packages/cn.rendu.protocol/CodeMode/Model/ClientServer");
        }

        public string GetProtoDir(string packageDir)
        {
            // Proto 文件仍然在各包的 Proto 目录
            return Path.Combine(packageDir, "Proto");
        }
    }
}
