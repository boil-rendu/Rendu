using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 菜单树资源加载扩展: 从磁盘批量把资源 / 导入器挂成节点。
    /// 作为扩展方法挂在 MenuTree 上, 与树结构的核心操作解耦。
    /// </summary>
    public static class MenuTreeAssetLoader
    {
        /// <summary>按资源类型加载并添加到菜单(资源本身作为节点值)</summary>
        public static IEnumerable<MenuTreeItem> AddAllAssetsAtPath(this MenuTree tree,
                                                                     string menuPath,
                                                                     string assetFolderPath,
                                                                     System.Type assetType,
                                                                     bool includeSubDirectories = false,
                                                                     bool flattenSubDirectories = false)
        {
            var result = new List<MenuTreeItem>();
            assetFolderPath = MenuTreePathUtil.NormalizeFolder(assetFolderPath);

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (MenuTreePathUtil.FilterAssetPath(assetPath, assetFolderPath, includeSubDirectories) == false) continue;

                var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                if (asset == null) continue;

                var childPath = MenuTreePathUtil.BuildChildMenuPath(menuPath, assetPath, assetFolderPath, flattenSubDirectories);
                var item = new MenuTreeItem(tree, Path.GetFileNameWithoutExtension(assetPath), asset);
                result.Add(tree.AddMenuItemAtPath(childPath, item));
            }

            return result;
        }

        /// <summary>按导入器类型加载并添加到菜单(导入器作为节点值)</summary>
        public static IEnumerable<MenuTreeItem> AddAllAssetImporterAtPath(this MenuTree tree,
                                                                             string menuPath,
                                                                             string assetFolderPath,
                                                                             System.Type importerType,
                                                                             bool includeSubDirectories = false,
                                                                             bool flattenSubDirectories = false)
        {
            var result = new List<MenuTreeItem>();
            assetFolderPath = MenuTreePathUtil.NormalizeFolder(assetFolderPath);

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (MenuTreePathUtil.FilterAssetPath(assetPath, assetFolderPath, includeSubDirectories) == false) continue;

                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null || importerType.IsInstanceOfType(importer) == false) continue;

                var childPath = MenuTreePathUtil.BuildChildMenuPath(menuPath, assetPath, assetFolderPath, flattenSubDirectories);
                var item = new MenuTreeItem(tree, Path.GetFileNameWithoutExtension(assetPath), importer);
                result.Add(tree.AddMenuItemAtPath(childPath, item));
            }

            return result;
        }
    }
}
