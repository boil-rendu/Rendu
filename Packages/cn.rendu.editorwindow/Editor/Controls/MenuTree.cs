using System;
using System.Collections.Generic;
using System.IO;
using RD.Panel.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.EditorWindows.Editor
{
    /// <summary>
    /// 树节点选中变化类型
    /// </summary>
    public enum SelectionChangedType
    {
        SelectionChanged = 0,
        ItemAdded,
        ItemRemoved,
    }

    /// <summary>
    /// 轻量菜单树节点
    /// </summary>
    public class MenuTreeItem
    {
        public MenuTree Tree { get; internal set; }

        public string Name { get; set; }

        public object Value { get; set; }

        public Texture Icon { get; set; }

        public MenuTreeItem Parent { get; internal set; }

        public readonly List<MenuTreeItem> ChildMenuItems = new();

        public bool IsSelected { get; internal set; }

        /// <summary>是否展开</summary>
        public bool Toggled { get; set; }

        public bool HasChildren => ChildMenuItems.Count > 0;

        public MenuTreeItem()
        {
        }

        public MenuTreeItem(MenuTree tree, string name, object value = null)
        {
            Tree  = tree;
            Name  = name;
            Value = value;
        }

        /// <summary>排序后的全部子节点(含子树的叶子)</summary>
        public IEnumerable<MenuTreeItem> GetChildMenuItems()
        {
            return ChildMenuItems;
        }

        public MenuTreeItem AddChild(MenuTreeItem child)
        {
            child.Tree   = Tree;
            child.Parent = this;
            ChildMenuItems.Add(child);
            return child;
        }

        public void RemoveChild(MenuTreeItem child)
        {
            ChildMenuItems.Remove(child);
        }

        public MenuTreeItem AddIcon(Texture icon)
        {
            Icon = icon;
            return this;
        }

        public MenuTreeItem AddIcon(EditorIcon icon)
        {
            Icon = icon.Icon;
            return this;
        }

        public void Select()
        {
            Tree?.Selection.Select(this);
        }
    }

    /// <summary>
    /// 选中集合 兼容原 SelectionChanged 事件
    /// </summary>
    public class MenuTreeSelection
    {
        private readonly List<MenuTreeItem> m_Selected = new();

        public event Action<SelectionChangedType> SelectionChanged;

        public int Count => m_Selected.Count;

        public MenuTreeItem this[int index] => m_Selected[index];

        public MenuTreeItem SelectedValue => m_Selected.Count > 0 ? m_Selected[0] : null;

        public IReadOnlyList<MenuTreeItem> Selected => m_Selected;

        public void Select(MenuTreeItem item)
        {
            if (item == null)
            {
                Clear();
                return;
            }

            if (m_Selected.Count == 1 && ReferenceEquals(m_Selected[0], item))
            {
                return;
            }

            foreach (var old in m_Selected)
            {
                old.IsSelected = false;
            }

            m_Selected.Clear();
            item.IsSelected = true;
            m_Selected.Add(item);

            OnSelectionChanged(SelectionChangedType.ItemAdded);
        }

        public void Clear()
        {
            if (m_Selected.Count == 0) return;

            foreach (var old in m_Selected)
            {
                old.IsSelected = false;
            }

            m_Selected.Clear();
            OnSelectionChanged(SelectionChangedType.ItemRemoved);
        }

        public void Select(MenuTreeItem[] items)
        {
            Clear();
            foreach (var item in items)
            {
                if (item == null || m_Selected.Contains(item)) continue;
                item.IsSelected = true;
                m_Selected.Add(item);
            }

            OnSelectionChanged(SelectionChangedType.ItemAdded);
        }

        private void OnSelectionChanged(SelectionChangedType type)
        {
            try
            {
                SelectionChanged?.Invoke(type);
            }
            catch (Exception e)
            {
                // 订阅者回调抛异常不该把 Select/Clear 的调用栈顶穿, 但也不能用 Debug.LogException:
                // 它同样是 Unity 内部调用, 在非 Unity 进程里会再抛 SecurityException, 等于用一个异常换掉另一个。
                // 走 Log 后消息里仍带完整堆栈(字符串插值会调 Exception.ToString())。
                Debug.LogError($"[EditorWindow] 菜单选中回调失败\n{e}");
            }
        }
    }

    /// <summary>
    /// 轻量菜单树
    /// </summary>
    public class MenuTree
    {
        public readonly List<MenuTreeItem> MenuItems = new();

        public MenuTreeSelection Selection { get; } = new();

        public string ConfigID { get; set; }

        public void SelectFirstMenuItem()
        {
            if (Selection.Count > 0) return;
            foreach (var root in MenuItems)
            {
                Selection.Select(root);
                return;
            }
        }

        public void SelectMenuItem(string path)
        {
            var item = FindMenuItem(path);
            item?.Select();
        }

        public MenuTreeItem FindMenuItem(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0) return null;

            List<MenuTreeItem> currentLevel = MenuItems;
            MenuTreeItem current = null;

            foreach (var segment in segments)
            {
                current = null;
                foreach (var item in currentLevel)
                {
                    if (item.Name == segment)
                    {
                        current = item;
                        break;
                    }
                }

                if (current == null) return null;
                currentLevel = current.ChildMenuItems;
            }

            return current;
        }

        /// <summary>根据路径添加节点(可带斜杠) 缺省的中间层级会自动补为文件夹节点</summary>
        public MenuTreeItem Add(string menuPath, object value, Texture icon = null)
        {
            if (string.IsNullOrEmpty(menuPath)) return null;
            return AddMenuItemAtPath(menuPath, value, icon);
        }

        public MenuTreeItem Add(string menuPath, object value, EditorIcon icon)
        {
            return Add(menuPath, value, icon.Icon);
        }

        private MenuTreeItem AddMenuItemAtPath(string menuPath, object value, Texture icon)
        {
            var name = menuPath;
            var parentPath = "";
            var lastSlash = menuPath.LastIndexOf('/');
            if (lastSlash >= 0)
            {
                name = menuPath.Substring(lastSlash + 1);
                parentPath = menuPath.Substring(0, lastSlash);
            }

            var item = new MenuTreeItem(this, name, value);
            if (icon != null) item.Icon = icon;
            return AddMenuItemAtPath(parentPath, item);
        }

        /// <summary>在指定父路径下挂载一个已创建的节点</summary>
        public MenuTreeItem AddMenuItemAtPath(string parentMenuPath, MenuTreeItem item)
        {
            var parent = EnsureFolder(parentMenuPath);
            if (parent != null)
            {
                var exist = parent.ChildMenuItems.Find(x => x.Name == item.Name);
                if (exist != null && ReferenceEquals(exist, item) == false)
                {
                    //同名节点直接替换内容 避免重复
                    exist.Value = item.Value;
                    if (item.Icon != null) exist.Icon = item.Icon;
                    return exist;
                }

                parent.AddChild(item);
            }
            else
            {
                var exist = MenuItems.Find(x => x.Name == item.Name);
                if (exist != null && ReferenceEquals(exist, item) == false)
                {
                    exist.Value = item.Value;
                    if (item.Icon != null) exist.Icon = item.Icon;
                    return exist;
                }

                item.Tree   = this;
                item.Parent = null;
                MenuItems.Add(item);
            }

            return item;
        }

        /// <summary>
        /// 确保某路径的父级文件夹节点存在
        /// </summary>
        private MenuTreeItem EnsureFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return null;
            }

            var segments = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            List<MenuTreeItem> currentLevel = MenuItems;
            MenuTreeItem current = null;

            foreach (var segment in segments)
            {
                current = null;
                foreach (var item in currentLevel)
                {
                    if (item.Name == segment)
                    {
                        current = item;
                        break;
                    }
                }

                if (current == null)
                {
                    current = new MenuTreeItem(this, segment, null);
                    AddAtLevel(currentLevel, current);
                }

                currentLevel = current.ChildMenuItems;
            }

            return current;
        }

        private void AddAtLevel(List<MenuTreeItem> level, MenuTreeItem item)
        {
            item.Tree = this;
            level.Add(item);
        }

        /// <summary>遍历所有节点(含子节点)</summary>
        public void ForEach(Action<MenuTreeItem> action)
        {
            if (action == null) return;
            var stack = new Stack<MenuTreeItem>();
            for (var i = MenuItems.Count - 1; i >= 0; i--)
            {
                stack.Push(MenuItems[i]);
            }

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                action(item);
                for (var i = item.ChildMenuItems.Count - 1; i >= 0; i--)
                {
                    stack.Push(item.ChildMenuItems[i]);
                }
            }
        }

        public void Clear()
        {
            MenuItems.Clear();
            Selection.Clear();
        }

        /// <summary>按资源类型加载并添加到菜单(资源本身作为节点值)</summary>
        public IEnumerable<MenuTreeItem> AddAllAssetsAtPath(string menuPath,
                                                                string assetFolderPath,
                                                                System.Type assetType,
                                                                bool includeSubDirectories = false,
                                                                bool flattenSubDirectories = false)
        {
            var result = new List<MenuTreeItem>();
            assetFolderPath = NormalizeFolder(assetFolderPath);

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (FilterAssetPath(assetPath, assetFolderPath, includeSubDirectories) == false) continue;

                var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                if (asset == null) continue;

                var childPath = BuildChildMenuPath(menuPath, assetPath, assetFolderPath, flattenSubDirectories);
                var item = new MenuTreeItem(this, Path.GetFileNameWithoutExtension(assetPath), asset);
                result.Add(AddMenuItemAtPath(childPath, item));
            }

            return result;
        }

        /// <summary>按导入器类型加载并添加到菜单(导入器作为节点值)</summary>
        public IEnumerable<MenuTreeItem> AddAllAssetImporterAtPath(string menuPath,
                                                                       string assetFolderPath,
                                                                       System.Type importerType,
                                                                       bool includeSubDirectories = false,
                                                                       bool flattenSubDirectories = false)
        {
            var result = new List<MenuTreeItem>();
            assetFolderPath = NormalizeFolder(assetFolderPath);

            foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
            {
                if (FilterAssetPath(assetPath, assetFolderPath, includeSubDirectories) == false) continue;

                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer == null || importerType.IsInstanceOfType(importer) == false) continue;

                var childPath = BuildChildMenuPath(menuPath, assetPath, assetFolderPath, flattenSubDirectories);
                var item = new MenuTreeItem(this, Path.GetFileNameWithoutExtension(assetPath), importer);
                result.Add(AddMenuItemAtPath(childPath, item));
            }

            return result;
        }

        private static string NormalizeFolder(string assetFolderPath)
        {
            assetFolderPath = (assetFolderPath ?? "").Trim().TrimEnd('/') + "/";
            var lower = assetFolderPath.ToLower();
            if (lower.StartsWith("assets/") == false && lower.StartsWith("packages/") == false)
            {
                assetFolderPath = "Assets/" + assetFolderPath;
            }

            return assetFolderPath;
        }

        private static bool FilterAssetPath(string assetPath, string folder, bool includeSubDirectories)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return false;
            if (includeSubDirectories)
            {
                return assetPath.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase);
            }

            var dirName = GetDirectoryName(assetPath).Trim('/') + "/";
            return string.Compare(dirName, folder.Trim('/') + "/", StringComparison.OrdinalIgnoreCase) == 0;
        }

        private string BuildChildMenuPath(string menuPath, string assetPath, string folder, bool flattenSubDirectories)
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

        private static string GetDirectoryName(string path)
        {
            var idx = path.LastIndexOf('/');
            return idx < 0 ? "" : path.Substring(0, idx);
        }
    }
}
