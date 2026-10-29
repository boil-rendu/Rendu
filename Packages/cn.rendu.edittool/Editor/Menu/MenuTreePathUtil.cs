using System;
using System.IO;
using UnityEditor;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 菜单树路径相关的纯静态工具(归一化 / 过滤 / 拼接), 无状态, 不依赖具体树实例。
    /// </summary>
    public static class MenuTreePathUtil
    {
        public static string NormalizeFolder(string assetFolderPath)
        {
            assetFolderPath = (assetFolderPath ?? "").Trim().TrimEnd('/') + "/";
            var lower = assetFolderPath.ToLower();
            if (lower.StartsWith("assets/") == false && lower.StartsWith("packages/") == false)
            {
                assetFolderPath = "Assets/" + assetFolderPath;
            }

            return assetFolderPath;
        }

        public static bool FilterAssetPath(string assetPath, string folder, bool includeSubDirectories)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return false;
            if (includeSubDirectories)
            {
                return assetPath.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase);
            }

            var dirName = GetDirectoryName(assetPath).Trim('/') + "/";
            return string.Compare(dirName, folder.Trim('/') + "/", StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static string BuildChildMenuPath(string menuPath, string assetPath, string folder, bool flattenSubDirectories)
        {
            menuPath = menuPath ?? "";
            menuPath = menuPath.Trim('/');

            if (flattenSubDirectories)
            {
                return $"{menuPath}/{Path.GetFileNameWithoutExtension(assetPath)}".Trim('/');
            }

            var relative = GetDirectoryName(assetPath).TrimEnd('/') + "/";
            if (relative.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase))
            {
                var sub = relative.Substring(folder.Length).Trim('/');
                if (string.IsNullOrEmpty(sub) == false)
                {
                    menuPath = $"{menuPath}/{sub}".Trim('/');
                }
            }

            return $"{menuPath}/{Path.GetFileNameWithoutExtension(assetPath)}".Trim('/');
        }

        public static string GetDirectoryName(string path)
        {
            var idx = path.LastIndexOf('/');
            return idx < 0 ? "" : path.Substring(0, idx);
        }
    }
}
