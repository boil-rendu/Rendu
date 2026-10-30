using System;
using System.Collections.Generic;
using RD.Draw.Editor;
using UnityEngine;
using UnityEngine.UIElements;   // CustomDraw 的 VisualElement

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 右键菜单里的一项: 文案 + 回调 + 可用判定。
    /// 挂在 <see cref="MenuTreeItem"/> 上, 由菜单窗口在行上弹出(见 MenuEditorWindow 的 ContextualMenuPopulateEvent)。
    /// </summary>
    public sealed class ContextMenuEntry
    {
        public string Text;

        /// <summary>点击时执行; 参数是**被右键的那一项**。</summary>
        public Action<MenuTreeItem> Action;

        /// <summary>null 视为可用; 返回 false 时条目置灰(与 05 档的项置灰相互独立)。</summary>
        public Func<bool> Enabled;
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

        /// <summary>
        /// 在默认行(折叠箭头 + 图标 + 名称)之后追加的自绘内容: 徽标 / 内联按钮 / 进度条等。
        /// null 表示走默认行, 不追加。由菜单窗口在行构建末尾调用。
        /// </summary>
        public Action<VisualElement> CustomDraw;

        /// <summary>
        /// 行高: 默认 24(与菜单默认行高一致)。一行放不下的内容(徽标 + 双行)调大它, 别去改全局常量。
        /// </summary>
        public float RowHeight = 24f;

        /// <summary>
        /// 是否允许被拖动重排(opt-in, 默认 false): 只有 true 的项才能作为拖拽源。
        /// 拖放的目标不限于 Draggable 项 —— 落点由被悬停的那一行决定。
        /// </summary>
        public bool Draggable;

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

        /// <summary>右键菜单项(按注册顺序弹出); 没有条目的节点右键不会有菜单。</summary>
        public readonly List<ContextMenuEntry> ContextMenuEntries = new();

        /// <summary>给本节点加一条右键菜单; <paramref name="enabled"/> 为 null 表示始终可用。</summary>
        public MenuTreeItem AddContextMenu(string text, Action<MenuTreeItem> act, Func<bool> enabled = null)
        {
            if (string.IsNullOrEmpty(text) || act == null) return this;
            ContextMenuEntries.Add(new ContextMenuEntry { Text = text, Action = act, Enabled = enabled });
            return this;
        }

        public void Select()
        {
            Tree?.Selection.Select(this);
        }
    }
}
