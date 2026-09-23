using System.IO;
using RD.Core;
using UnityEditor;
using UnityEngine;

namespace RD.Loader.Editor
{
    /// <summary>
    /// 项目文件命令层 - 封装 Unity API 和文件操作
    /// </summary>
    public static class ProjectFilesCommand
    {
        /// <summary>
        /// 重新生成项目文件
        /// </summary>
        public static int ReGenerateProjectFiles()
        {
            // 重新生成项目文件
            UnityEditor.EditorUtility.DisplayProgressBar("ReGenerate Project Files", "Generating project files...", 0.5f);
            
            // 触发 Unity 项目文件重新生成
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // 删除忽略的 csproj 文件
            int deletedCount = DeleteIgnoredCsprojFiles();

            UnityEditor.EditorUtility.ClearProgressBar();

            Debug.Log($"[ProjectFilesCommand] Regenerate csproj completed. Deleted {deletedCount} ignored files.");

            return deletedCount;
        }

        /// <summary>
        /// 删除忽略的 csproj 文件
        /// </summary>
        private static int DeleteIgnoredCsprojFiles()
        {
            var files = FileHelper.GetAllFiles(".", "Ignore.RD*.csproj");
            int deletedCount = 0;

            foreach (string file in files)
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        deletedCount++;
                        Debug.Log($"[ProjectFilesCommand] Deleted ignored csproj: {file}");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ProjectFilesCommand] Failed to delete {file}: {ex.Message}");
                }
            }

            return deletedCount;
        }

        /// <summary>
        /// 获取忽略的 csproj 文件列表
        /// </summary>
        public static string[] GetIgnoredCsprojFiles()
        {
            return FileHelper.GetAllFiles(".", "Ignore.RD*.csproj").ToArray();
        }

        /// <summary>
        /// 删除指定的忽略的 csproj 文件
        /// </summary>
        public static void DeleteIgnoredCsprojFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[ProjectFilesCommand] Deleted ignored csproj: {filePath}");
            }
        }
    }
}
