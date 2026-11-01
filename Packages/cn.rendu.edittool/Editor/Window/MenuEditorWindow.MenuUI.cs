using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 左侧菜单树的 UI: 行的构建、点击 / 右键、搜索过滤与键盘导航。
    /// 交互产生的状态交回主文件(选中 / 高亮 / 重绘), 这里只碰 VisualElement。
    /// </summary>
    public partial class MenuEditorWindow
    {
        /// <summary>菜单行引用: 行容器 + 名称标签 + 折叠箭头 + 子级容器, 用于交互与高亮刷新。</summary>
        private sealed class MenuRowRef
        {
            public VisualElement Row;
            public Label         Label;
            public Label         Foldout;   // 叶子项为 null
            public VisualElement Children;  // 叶子项为 null
        }

        /// <summary>最近一次普通点击的项: Shift 范围选以它为锚点。</summary>
        private MenuTreeItem m_LastClicked;

        //-------------------------------------- 构建 --------------------------------------

        private void BuildMenuUI(VisualElement menuPanel)
        {
            m_MenuRowMap = new Dictionary<MenuTreeItem, MenuRowRef>();

            SetupExternalDrop(menuPanel);

            // 注: 2022.3 的 UIElements TextField 无 placeholderText(该能力 Unity 6 才有), 故不设占位提示
            m_SearchFilter = new TextField();
            m_SearchFilter.style.marginBottom = MenuPadding;
            m_SearchFilter.RegisterValueChangedCallback(evt => ApplyFilter(evt.newValue));
            menuPanel.Add(m_SearchFilter);

            m_MenuScroll = new ScrollView { style = { flexGrow = 1f } };
            menuPanel.Add(m_MenuScroll);

            var inner = new VisualElement
            {
                style =
                {
                    paddingTop    = MenuPadding,
                    paddingBottom = MenuPadding,
                },
            };
            m_MenuScroll.contentContainer.Add(inner);

            BuildMenuItems(m_MenuTree.MenuItems, inner);
            RebuildVisibleOrder();
        }

        private void BuildMenuItems(IReadOnlyList<MenuTreeItem> items, VisualElement parent)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                var row    = BuildRow(item, out var label, out var foldout);
                var rowRef = new MenuRowRef { Row = row, Label = label, Foldout = foldout };
                m_MenuRowMap[item] = rowRef;

                // 拖放目标解析要靠它: 事件 target 可能是行内的图标 / 文字, 得沿父链把项找回来
                row.userData = item;
                SetupRowDrag(row, item);

                parent.Add(row);

                // 子级: 单独一个容器, 按 Toggled 显隐, 并整体缩进一级
                if (item.HasChildren)
                {
                    var children = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = MenuIndentWidth,
                            display     = item.Toggled ? DisplayStyle.Flex : DisplayStyle.None,
                        },
                    };
                    parent.Add(children);
                    BuildMenuItems(item.ChildMenuItems, children);
                    rowRef.Children = children;
                }
            }
        }

        /// <summary>一行菜单: 折叠箭头 → 图标 → 名称 → 项自绘, 并挂上点击 / 右键回调。</summary>
        private VisualElement BuildRow(MenuTreeItem item, out Label label, out Label foldout)
        {
            var row = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height        = item.RowHeight,   // 每项可调; 默认见 MenuTreeItem.RowHeight
                    alignItems    = Align.Center,
                },
            };

            row.Add(BuildFoldout(item, out foldout));

            // 图标 + 名称
            if (item.Icon is Texture2D icon)
            {
                row.Add(new Image
                {
                    image = icon,
                    style = { width = 16f, height = 16f, marginRight = MenuPadding, flexShrink = 0f },
                });
            }

            label = new Label(item.Name)
            {
                style = { flexGrow = 1f, unityTextAlign = TextAnchor.MiddleLeft, color = NormalMenuTextColor },
            };
            row.Add(label);

            // 每项自绘: 在默认行之后追加(徽标 / 内联按钮 / 进度条); 未设置的项与现状像素级一致
            if (item.CustomDraw != null)
            {
                try { item.CustomDraw.Invoke(row); }
                catch (Exception e) { Debug.LogError($"[EditTool] 菜单项自绘失败 {item.Name}\n{e}"); }
            }

            SetupRowContextMenu(row, item);
            SetupRowClick(row, item);
            return row;
        }

        /// <summary>折叠箭头: 有子级才画, 点击只展开/收起(不选中); 没子级用等量留白对齐文字起点。</summary>
        private VisualElement BuildFoldout(MenuTreeItem item, out Label foldout)
        {
            foldout = null;

            if (item.HasChildren == false)
                return new VisualElement { style = { width = FoldoutWidth, flexShrink = 0f } };

            foldout = new Label(item.Toggled ? "▾" : "▸")
            {
                style =
                {
                    width           = FoldoutWidth,
                    height          = FoldoutWidth,
                    unityTextAlign  = TextAnchor.MiddleCenter,
                    flexShrink      = 0f,
                },
            };
            foldout.RegisterCallback<ClickEvent>(evt =>
            {
                SetItemExpanded(item, !item.Toggled);
                evt.StopPropagation();
            });
            return foldout;
        }

        //-------------------------------------- 行交互 --------------------------------------

        /// <summary>右键菜单: 没有条目的节点不挂回调 —— UIElements 仍会弹一个空菜单, 不如干脆不注册。</summary>
        private void SetupRowContextMenu(VisualElement row, MenuTreeItem item)
        {
            if (item.ContextMenuEntries.Count == 0) return;

            row.RegisterCallback<ContextualMenuPopulateEvent>(evt =>
            {
                foreach (var entry in item.ContextMenuEntries)
                {
                    var e       = entry;   // 逐项捕获, 避免闭包绑到最后一个
                    var enabled = e.Enabled?.Invoke() ?? true;
                    evt.menu.AppendAction(e.Text,
                                          _ =>
                                          {
                                              try { e.Action?.Invoke(item); }
                                              catch (Exception ex)
                                              {
                                                  Debug.LogError($"[EditTool] 右键菜单执行失败 {item.Name} / {e.Text}\n{ex}");
                                              }
                                          },
                                          enabled ? DropdownMenuAction.Status.Normal
                                                  : DropdownMenuAction.Status.Disabled);
                }
            });
        }

        /// <summary>整行点击: Shift / Ctrl(⌘) 走多选, 普通左键仍"有子级顺手展开 + 选中"(与旧行为一致)。</summary>
        private void SetupRowClick(VisualElement row, MenuTreeItem item)
        {
            row.RegisterCallback<ClickEvent>(evt =>
            {
                try
                {
                    // 多选只改选中, 不动展开态 —— 否则 Ctrl 点一下就把子级收起来了
                    if (evt.shiftKey && m_LastClicked != null)
                    {
                        var range = RangeInVisibleOrder(m_LastClicked, item);
                        if (range != null)
                        {
                            m_MenuTree.Selection.Select(range);
                            return;
                        }
                    }
                    else if (evt.ctrlKey || evt.commandKey)
                    {
                        m_MenuTree.Selection.ToggleSelect(item);
                        m_LastClicked = item;
                        return;
                    }

                    if (item.HasChildren) SetItemExpanded(item, !item.Toggled);
                    item.Select();
                    m_LastClicked = item;
                }
                catch (Exception e) { Debug.LogError($"[EditTool] 菜单选中失败 {item.Name}\n{e}"); }
            });
        }

        private void SetItemExpanded(MenuTreeItem item, bool expanded)
        {
            item.Toggled = expanded;
            if (m_MenuRowMap == null || !m_MenuRowMap.TryGetValue(item, out var rowRef)) return;
            if (rowRef.Foldout != null) rowRef.Foldout.text = expanded ? "▾" : "▸";
            if (rowRef.Children != null) rowRef.Children.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            RebuildVisibleOrder();
        }

        /// <summary>按当前选中态重刷行的高亮(避免整树重建)。</summary>
        private void RefreshMenuSelection()
        {
            if (m_MenuRowMap == null) return;
            foreach (var kv in m_MenuRowMap)
            {
                var selected = kv.Key.IsSelected;
                kv.Value.Row.style.backgroundColor = selected ? MenuSelectColor : Color.clear;
                kv.Value.Label.style.color         = selected ? Color.white : NormalMenuTextColor;
            }
        }

        /// <summary>项被重排: 树的形状变了, 重建左侧 UI 并保住当前过滤关键字。</summary>
        private void OnMenuItemsReordered()
        {
            if (m_MenuPanel == null || m_MenuTree == null) return;

            m_MenuPanel.Clear();
            BuildMenuUI(m_MenuPanel);

            // 重建后搜索框是新建的(空), 但过滤关键字 m_Filter 仍在:
            // 回填关键字并重套过滤, 否则"框里没字、树却被过滤着"两种状态会脱节。
            if (m_SearchFilter != null)
            {
                m_SearchFilter.value = m_Filter;   // 回填(变更回调会顺带重套一次过滤)
                ApplyFilter(m_Filter);             // 幂等, 保证树的显隐与关键字一致
            }

            RefreshMenuSelection();
        }

        //-------------------------------------- 搜索 / 可见序 --------------------------------------

        /// <summary>按关键字过滤菜单: 只改行 display, 不重建树; 命中项强制展开其祖先路径。</summary>
        private void ApplyFilter(string q)
        {
            if (m_MenuRowMap == null) return;

            m_Filter = (q ?? "").Trim();
            if (m_Filter.Length == 0)
            {
                foreach (var kv in m_MenuRowMap)
                    kv.Value.Row.style.display = RowShownCanonical(kv.Key) ? DisplayStyle.Flex : DisplayStyle.None;
                RebuildVisibleOrder();
                return;
            }

            foreach (var kv in m_MenuRowMap)
                kv.Value.Row.style.display = DisplayStyle.None;

            foreach (var kv in m_MenuRowMap)
            {
                if (!NameMatches(kv.Key, m_Filter)) continue;
                var n = kv.Key;
                while (n != null && m_MenuRowMap.TryGetValue(n, out var nr))
                {
                    nr.Row.style.display = DisplayStyle.Flex;
                    n = n.Parent;
                }
            }
            RebuildVisibleOrder();
        }

        private static bool NameMatches(MenuTreeItem item, string q)
            => item.Name != null && item.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>无过滤时, 项是否按 Toggled 链可见(其所有祖先都已展开)。</summary>
        private static bool RowShownCanonical(MenuTreeItem item)
        {
            var p = item.Parent;
            while (p != null)
            {
                if (!p.Toggled) return false;
                p = p.Parent;
            }
            return true;
        }

        /// <summary>按当前 display 重建可见有序项(先序遍历, 供键盘导航)。</summary>
        private void RebuildVisibleOrder()
        {
            if (m_VisibleOrder == null) m_VisibleOrder = new List<MenuTreeItem>();
            m_VisibleOrder.Clear();
            if (m_MenuTree == null) return;
            WalkVisible(m_MenuTree.MenuItems);
        }

        private void WalkVisible(IReadOnlyList<MenuTreeItem> items)
        {
            if (items == null) return;
            foreach (var item in items)
            {
                if (item == null) continue;
                if (m_MenuRowMap.TryGetValue(item, out var rr) && rr.Row.style.display != DisplayStyle.None)
                    m_VisibleOrder.Add(item);
                if (item.HasChildren) WalkVisible(item.ChildMenuItems);
            }
        }

        /// <summary>
        /// 取两项之间的连续区间(按当前**可见**序, 过滤/折叠下才符合题意);
        /// 任一端已不在可见序里(例如被过滤掉了)就返回 null, 调用方退回单选。
        /// </summary>
        private MenuTreeItem[] RangeInVisibleOrder(MenuTreeItem a, MenuTreeItem b)
        {
            if (m_VisibleOrder == null) return null;

            var ia = m_VisibleOrder.IndexOf(a);
            var ib = m_VisibleOrder.IndexOf(b);
            if (ia < 0 || ib < 0) return null;

            var s = Mathf.Min(ia, ib);
            var e = Mathf.Max(ia, ib);
            var result = new MenuTreeItem[e - s + 1];
            for (var i = s; i <= e; i++) result[i - s] = m_VisibleOrder[i];
            return result;
        }

        /// <summary>把当前选中项滚入菜单视口(选中变化后调用)。</summary>
        private void ScrollSelectedIntoView()
        {
            if (m_MenuScroll == null || m_MenuTree == null || m_MenuRowMap == null) return;

            var item = m_MenuTree.Selection.SelectedValue;
            if (item == null || !m_MenuRowMap.TryGetValue(item, out var rowRef)) return;

            var rowBound  = rowRef.Row.worldBound;
            var viewBound = m_MenuScroll.worldBound;
            var off       = m_MenuScroll.scrollOffset;
            if (rowBound.yMin < viewBound.yMin)
                m_MenuScroll.scrollOffset = new Vector2(off.x, off.y + (rowBound.yMin - viewBound.yMin));
            else if (rowBound.yMax > viewBound.yMax)
                m_MenuScroll.scrollOffset = new Vector2(off.x, off.y + (rowBound.yMax - viewBound.yMax));
        }

        private void OnMenuKeyDown(KeyDownEvent evt)
        {
            if (evt.target is TextField) return;
            if (m_MenuTree == null || m_VisibleOrder == null || m_VisibleOrder.Count == 0) return;

            var current = m_MenuTree.Selection.SelectedValue;
            var idx     = current != null ? m_VisibleOrder.IndexOf(current) : -1;

            switch (evt.keyCode)
            {
                case KeyCode.UpArrow:   idx = idx <= 0 ? m_VisibleOrder.Count - 1 : idx - 1; break;
                case KeyCode.DownArrow: idx = idx >= m_VisibleOrder.Count - 1 ? 0 : idx + 1; break;
                case KeyCode.Home:      idx = 0; break;
                case KeyCode.End:       idx = m_VisibleOrder.Count - 1; break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (current != null) m_MenuTree.Activate(current);
                    evt.StopPropagation();
                    return;
                default:
                    if (evt.character == '\0' || char.IsControl(evt.character)) return;
                    idx = IndexOfTypedChar(evt.character, idx);
                    break;
            }

            evt.StopPropagation();
            if (idx >= 0 && idx < m_VisibleOrder.Count)
                m_VisibleOrder[idx].Select();
        }

        /// <summary>从当前项的下一项起找首字母匹配项(循环一圈), 没有就停在原地。</summary>
        private int IndexOfTypedChar(char typed, int from)
        {
            var c = char.ToLowerInvariant(typed);
            for (int i = 1; i <= m_VisibleOrder.Count; i++)
            {
                var cand = m_VisibleOrder[(from + i) % m_VisibleOrder.Count];
                if (string.IsNullOrEmpty(cand.Name) || char.ToLowerInvariant(cand.Name[0]) != c) continue;
                return (from + i) % m_VisibleOrder.Count;
            }
            return from;
        }
    }
}
