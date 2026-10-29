using System;
using System.Collections.Generic;
using UnityEngine;

namespace RD.EditTool.Editor
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
}
