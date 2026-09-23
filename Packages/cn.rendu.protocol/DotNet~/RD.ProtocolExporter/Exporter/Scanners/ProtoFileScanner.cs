using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RD.Core;

namespace RD
{
    /// <summary>
    /// Proto 文件扫描器 - 负责扫描和查找 Proto 文件
    /// </summary>
    public class ProtoFileScanner
    {
        private readonly PathProvider pathProvider;
        private readonly PackagesLock packagesLock;

        public ProtoFileScanner(PathProvider pathProvider, PackagesLock packagesLock)
        {
            this.pathProvider = pathProvider;
            this.packagesLock = packagesLock;
        }

        public List<ProtoFileInfo> Scan()
        {
            var files = new List<ProtoFileInfo>();

            foreach ((string key, PackageInfo packageInfo) in packagesLock.dependencies)
            {
                string protoDir = pathProvider.GetProtoDir(packageInfo.dir);
                if (!Directory.Exists(protoDir))
                    continue;

                Console.WriteLine($"[ProtoFileScanner] 发现 Proto 目录: {protoDir}");

                foreach (string filePath in FileHelper.GetAllFiles(protoDir, "*.proto"))
                {
                    files.Add(new ProtoFileInfo
                    {
                        PackageDir = packageInfo.dir,
                        FilePath = filePath,
                        Module = packageInfo.module
                    });
                }
            }

            Console.WriteLine($"[ProtoFileScanner] 找到 {files.Count} 个 Proto 文件");
            return files;
        }
    }

    /// <summary>
    /// Proto 文件信息
    /// </summary>
    public class ProtoFileInfo
    {
        public string PackageDir { get; set; }
        public string FilePath { get; set; }
        public string Module { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(FilePath);

        /// <summary>
        /// 解析文件名: Name_CS_StartOpcode
        /// </summary>
        public void ParseFileName(out string protoName, out string csFlag, out int startOpcode)
        {
            string[] parts = FileNameWithoutExtension.Split('_');
            protoName = parts[0];
            csFlag = parts[1];
            startOpcode = int.Parse(parts[2]);
        }
    }
}
