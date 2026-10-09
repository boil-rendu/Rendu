using System.IO;
using UnityEditor;

using RD.Client;
using RD.Core;
namespace RD.UI.Editor
{
    /// <summary>目录递归拷贝（目标已存在时需二次确认并全覆盖）</summary>
    public static class CopyFolder
    {
        /// <summary>
        /// 把 sourceFolder 完整拷贝到 targetFolder
        /// 目标已存在会先弹确认；拷贝完成后刷新程序集引用并关闭 AutoTool 窗口
        /// </summary>
        public static void Copy(string sourceFolder, string targetFolder)
        {
            DirectoryInfo sourceDir = new(sourceFolder);
            DirectoryInfo targetDir = new(targetFolder);

            if (Directory.Exists(targetFolder))
            {
                UnityTipsHelper.CallBack($"{targetFolder} 已存在!!!\n \n警告将会执行全覆盖操作!!!", () =>
                {
                    Directory.Delete(targetFolder, true);
                    Start();
                });
            }
            else
            {
                Start();
            }

            return;

            void Start()
            {
                Directory.CreateDirectory(targetFolder);
                CopyAll(sourceDir, targetDir);
                UnityTipsHelper.Show("创建完毕");
                EditorApplication.ExecuteMenuItem("RD/Loader/ReGenerateProjectAssemblyReference");
                AutoTool.CloseWindowRefresh();
            }
        }

        /// <summary>递归拷贝目录内容（先拷文件，再递归子目录）</summary>
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
            {
                target.Create();
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                string targetFilePath = Path.Combine(target.FullName, fi.Name);
                fi.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}