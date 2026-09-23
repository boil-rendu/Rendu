using System.IO;
using UnityEngine;

namespace RD.Recast
{
    /// <summary>
    /// 文件管理器（负责清理导出过程中的各类文件）
    /// </summary>
    public static class NavMeshFileManager
    {
        private static readonly string[] IntermediateExtensions = { "_vertices.txt", "_triangles.txt", ".obj" };

        /// <summary>
        /// 清理中间文件（vertices/triangles/obj）
        /// </summary>
        public static void ClearIntermediateFiles(string exportPath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !Directory.Exists(exportPath))
                return;

            foreach (string ext in IntermediateExtensions)
            {
                string filePath = Path.Combine(exportPath, $"{fileName}{ext}");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    UnityEngine.Debug.Log($"已删除中间文件：{filePath}");
                }
            }
        }

        /// <summary>
        /// 清理导航网格文件
        /// </summary>
        public static void ClearNavMeshFile(string exportPath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !Directory.Exists(exportPath))
                return;

            string navMeshPath = Path.Combine(exportPath, $"{fileName}.recast");
            if (File.Exists(navMeshPath))
            {
                File.Delete(navMeshPath);
                UnityEngine.Debug.Log($"已删除导航网格文件：{navMeshPath}");
            }
        }

        /// <summary>
        /// 清理所有生成文件
        /// </summary>
        public static void ClearAllFiles(string exportPath, string fileName)
        {
            ClearIntermediateFiles(exportPath, fileName);
            ClearNavMeshFile(exportPath, fileName);
            UnityEngine.Debug.Log("已清理所有生成的文件（包括导航网格文件）");
        }

        /// <summary>
        /// 确保导出目录存在
        /// </summary>
        public static bool EnsureExportDirectory(string exportPath)
        {
            if (string.IsNullOrEmpty(exportPath))
            {
                Debug.LogError("导出路径未配置！");
                return false;
            }

            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
                Debug.Log($"创建导出目录：{exportPath}");
            }

            return true;
        }
    }
}
