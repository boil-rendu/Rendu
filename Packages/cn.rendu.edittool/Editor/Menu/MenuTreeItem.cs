using System.Collections.Generic;
using RD.Draw.Editor;
using UnityEngine;

namespace RD.EditTool.Editor
{
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
}
