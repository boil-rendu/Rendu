using System;
using System.Collections.Generic;
using System.IO;
using RD.Draw.Editor;
using UnityEditor;
using UnityEngine;

namespace RD.EditTool.Editor
{
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
    }
}
