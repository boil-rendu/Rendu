using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.EditTool.Editor
{
    /// <summary>
    /// 轻量菜单树编辑器窗口
    /// 左侧绘制 BuildMenuTree 返回的菜单树 右侧绘制当前选中项
    /// </summary>
    public class MenuEditorWindow : DrawWindow
    {
        /// <summary>本窗口的菜单树(首次访问时自动构建)</summary>
        protected MenuTree m_MenuTree;
        
        private bool m_MenuTreeBuilt;

        /// <summary>菜单栏最小宽: 同时也是 MenuWidth 的默认值与拖拽下限。</summary>
        private const float MinMenuWidth = 150f;
        /// <summary>
        /// 左树默认宽: 工具窗口的模块名普遍是 2~4 个字 用户仍可拖分割条改宽,
        /// 下限见 <see cref="MenuWidth"/>。
        /// </summary>
        private float m_MenuWidth = MinMenuWidth;

        /// <summary>
        /// 菜单行高: 18 是  单行控件的高度, 一排菜单挨得太紧, 点选容易串行;
        /// 24 给文字上下各留 3px, 与参考界面的疏密接近(参考约 30, 这里不取那么松)。
        /// </summary>
        private const float MenuRowHeight = 24f;

        /// <summary>折叠箭头(foldout)控件固定宽, 叶子项用等量 Space 对齐文字起点。</summary>
        private const float FoldoutWidth = 16f;

        /// <summary>子级菜单相对父级额外缩进(每层叠加一次)。</summary>
        private const float MenuIndentWidth = 12f;

        /// <summary>菜单/内容区上下留白, 以及菜单项文字左右内边距。</summary>
        private const int MenuPadding = 4;

        /// <summary>未选中菜单项顶部的空态提示距顶间距(菜单与内容区两处共用)。</summary>
        private const float EmptyHintTopMargin = 30f;

 

        /// <summary>分割条颜色(纯用 background 绘制)。</summary>
        private static readonly Color SplitterColor = new(0.15f, 0.15f, 0.15f, 1f);

        /// <summary>选中菜单项底纹色(低饱和蓝, 避免暗色面板过跳)。</summary>
        private static readonly Color MenuSelectColor = new(0.20f, 0.36f, 0.56f, 1f);

        /// <summary>
        /// 非选中菜单项文字色: 取编辑器默认标签色(跟随深浅皮肤)。
        /// 不能写 <c>StyleColor(StyleKeyword.Undefined)</c> —— 它在 color 上不会落回继承色, 而是解析成
        /// 初始的透明色, 结果只有被选中项(显式白字)可见、其余菜单项"消失"。
        /// </summary>
        private static Color NormalMenuTextColor => EditorStyles.label.normal.textColor;

        /// <summary>
        /// 左树与内容之间的分割条: 布局上只占 1px(参考界面就是一条线), 命中区左右各放宽 2px(不影响布局)。
        /// </summary>
        private const float SplitterWidth = 1f;

        /// <summary>分割条命中区在左右各多出来的宽度(只为好拖, 不占布局)。</summary>
        private const float SplitterHitPadding = 1f;
        
        /// <summary>承载左侧菜单树的面板(宽度由 MenuWidth 决定, 可拖分割条改)。</summary>
        private VisualElement m_MenuPanel;

        /// <summary>菜单项 -> 其行元素引用 的映射, 用于选中态高亮刷新(避免整树重建)。</summary>
        private Dictionary<MenuTreeItem, MenuRowRef> m_MenuRowMap;

        /// <summary>菜单搜索框(顶栏过滤, 只改 display 不重建树)。</summary>
        private TextField m_SearchFilter;

        /// <summary>承载菜单行的滚动视图(供键盘导航自动滚动)。</summary>
        private ScrollView m_MenuScroll;

        /// <summary>当前展开/过滤态下的可见有序项(供键盘导航跳过隐藏项)。</summary>
        private List<MenuTreeItem> m_VisibleOrder;

        /// <summary>当前搜索关键字(空串表示未过滤)。</summary>
        private string m_Filter = "";

        /// <summary>菜单项拖拽在 DragAndDrop 里用的键: 只认本窗口自己发起的项拖拽。</summary>
        private const string MenuItemDragKey = "RD.MenuTreeItem";

        /// <summary>位移超过它才算"拖", 免得手抖把一次点击变成拖拽。</summary>
        private const float DragThreshold = 4f;

        /// <summary>最近一次普通点击的项: Shift 范围选以它为锚点。</summary>
        private MenuTreeItem m_LastClicked;

        /// <summary>按下时暂存的拖拽候选(真正起拖要等 PointerMove 越过阈值)。</summary>
        private MenuTreeItem m_DragCandidate;

        /// <summary>按下时的指针位置: 指针事件的 position 是 Vector3(带 z), 与位移阈值比较要同类型。</summary>
        private Vector3      m_DragStartPos;
        private bool         m_DragPointerDown;
        private bool         m_DragStarted;

        /// <summary>
        /// 菜单栏宽度 
        /// </summary>
        public virtual float MenuWidth
        {
            get => m_MenuWidth;
            set => m_MenuWidth = Mathf.Max(MinMenuWidth, value);
        }

        /// <summary>构建菜单树 返回 null 表示无菜单</summary>
        protected virtual MenuTree BuildMenuTree()
        {
            return null;
        }

        /// <summary>当前菜单树(未构建时自动构建)</summary>
        protected MenuTree MenuTree
        {
            get
            {
                EnsureMenuTree();
                return m_MenuTree;
            }
        }

        private void EnsureMenuTree()
        {
            if (m_MenuTreeBuilt) return;
            m_MenuTreeBuilt = true;

            try
            {
                m_MenuTree = BuildMenuTree();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditTool] 菜单树构建失败 {GetType().Name}\n{e}");
                m_MenuTree = null;
            }

            // 无选中时默认选中第一项 触发派生类的 SelectionChanged 恢复逻辑
            if (m_MenuTree != null && m_MenuTree.Selection.Count == 0)
            {
                try
                {
                    m_MenuTree.SelectFirstMenuItem();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EditTool] 菜单默认选中失败 {GetType().Name}\n{e}");
                }
            }
        }

        /// <summary>强制重建菜单树(例如构建条件变化后调用): 重订阅选中事件并刷新左侧菜单 UI。</summary>
        protected void RebuildMenuTree()
        {
            m_MenuTreeBuilt = false;
            m_MenuTree = null;
            EnsureMenuTree();
            SubscribeSelectionEvents();   // 新建的树已在 BuildMenuTree 内订阅, 此处幂等补一次

            if (m_MenuPanel != null)
            {
                m_MenuPanel.Clear();
                BuildMenuUI(m_MenuPanel);

                // 重建后搜索框是新建的(空), 但过滤关键字 m_Filter 仍在:
                // 回填关键字并重套过滤, 否则"框里没字、树却被过滤着"两种状态会脱节。
                if (m_SearchFilter != null)
                {
                    m_SearchFilter.value = m_Filter;   // 回填(变更回调会顺带重套一次过滤)
                    ApplyFilter(m_Filter);             // 幂等, 显式保证树的显隐与关键字一致
                }

                RefreshMenuSelection();
            }
        }

        //-------------------------------------- 左侧菜单 --------------------------------------

        /// <summary>菜单行引用: 行容器 + 名称标签 + 折叠箭头 + 子级容器, 用于交互与高亮刷新。</summary>
        private sealed class MenuRowRef
        {
            public VisualElement Row;
            public Label         Label;
            public Label         Foldout;   // 叶子项为 null
            public VisualElement Children;  // 叶子项为 null
        }

        /// <summary>
        /// 统一订阅"菜单选中变化"事件的两个处理器(幂等, 可多处复用):
        /// - <see cref="OnSelectionChanged"/>      (业务/状态层, 由子类实现): 恢复上次选中、初始化模块、落盘最近选中;
        /// - <see cref="OnMenuSelectionChanged"/>   (UI 层): 刷新左侧高亮 + 重绘右侧内容。
        /// 先退订再订阅, 因此无论"首次构建 / 外壳(UI 布局)重建 / 强制重建"都只挂一份回调, 不会重复触发。
        /// </summary>
        protected void SubscribeSelectionEvents()
        {
            if (m_MenuTree == null) return;
            m_MenuTree.Selection.SelectionChanged -= OnSelectionChanged;
            m_MenuTree.Selection.SelectionChanged -= OnMenuSelectionChanged;
            m_MenuTree.Selection.SelectionChanged += OnSelectionChanged;
            m_MenuTree.Selection.SelectionChanged += OnMenuSelectionChanged;

            // 拖拽重排后要重建左侧 UI; 同样先退订再订阅, 重建多次也只挂一份
            m_MenuTree.ItemsReordered -= OnMenuItemsReordered;
            m_MenuTree.ItemsReordered += OnMenuItemsReordered;
        }

        /// <summary>基类钩子: 选中变化时的业务/状态处理(恢复选中、初始化模块、记忆最近选中)。子类重写。</summary>
        protected virtual void OnSelectionChanged(SelectionChangedType obj) { }

        private void BuildMenuUI(VisualElement menuPanel)
        {
            m_MenuRowMap = new Dictionary<MenuTreeItem, MenuRowRef>();

            // 外部对象(资源等)拖进菜单: 命中哪个节点由 userData 沿父链反查, 默认不接收 —— 子类重写 OnDropExternalObjects
            menuPanel.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.StopPropagation();
            });

            menuPanel.RegisterCallback<DragPerformEvent>(evt =>
            {
                var objs = DragAndDrop.objectReferences;
                if (objs == null || objs.Length == 0) return;

                DragAndDrop.AcceptDrag();
                evt.StopPropagation();

                var target = ResolveItemFromElement(evt.target as VisualElement);
                if (target != null)
                {
                    try { OnDropExternalObjects(target, objs); }
                    catch (Exception e) { Debug.LogError($"[EditTool] 外部对象拖入失败 {target.Name}\n{e}"); }
                }

                ResetDragState();
            });

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
                    paddingTop = MenuPadding,
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

                var row = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        height = item.RowHeight,      // 每项可调; 默认与 MenuRowHeight 一致
                        alignItems = Align.Center,
                    },
                };

                // 折叠箭头: 有子级才画, 点击只展开/收起(不选中); 没子级用等量留白对齐
                Label foldout = null;
                if (item.HasChildren)
                {
                    foldout = new Label(item.Toggled ? "▾" : "▸")
                    {
                        style =
                        {
                            width = FoldoutWidth,
                            height = FoldoutWidth,
                            unityTextAlign = TextAnchor.MiddleCenter,
                            flexShrink = 0f,
                        },
                    };
                    foldout.RegisterCallback<ClickEvent>(evt =>
                    {
                        SetItemExpanded(item, !item.Toggled);
                        evt.StopPropagation();
                    });
                    row.Add(foldout);
                }
                else
                {
                    row.Add(new VisualElement { style = { width = FoldoutWidth, flexShrink = 0f } });
                }

                // 图标 + 名称
                if (item.Icon is Texture2D icon)
                {
                    row.Add(new Image
                    {
                        image = icon,
                        style = { width = 16f, height = 16f, marginRight = MenuPadding, flexShrink = 0f },
                    });
                }

                var label = new Label(item.Name)
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

                // 右键菜单: 没有条目的节点不挂回调 —— UIElements 仍会弹一个空菜单, 不如干脆不注册
                if (item.ContextMenuEntries.Count > 0)
                {
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

                // 整行点击: Shift / Ctrl(⌘) 走多选, 普通左键仍"有子级顺手展开 + 选中"(与旧行为一致)
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

                // 拖放目标解析要靠它: 事件 target 可能是行内的图标 / 文字, 得沿父链把项找回来
                row.userData = item;
                SetupRowDrag(row, item);

                parent.Add(row);

                var rowRef = new MenuRowRef
                {
                    Row = row,
                    Label = label,
                    Foldout = foldout,
                };
                m_MenuRowMap[item] = rowRef;

                // 子级: 单独一个容器, 按 Toggled 显隐, 并整体缩进一级
                if (item.HasChildren)
                {
                    var children = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = MenuIndentWidth,
                            display = item.Toggled ? DisplayStyle.Flex : DisplayStyle.None,
                        },
                    };
                    parent.Add(children);
                    BuildMenuItems(item.ChildMenuItems, children);
                    rowRef.Children = children;
                }
            }
        }

        //-------------------------------------- 拖拽 --------------------------------------

        /// <summary>行的拖拽源与落点判定: 源要 Draggable, 落点按悬停行的上/下半段决定。</summary>
        private void SetupRowDrag(VisualElement row, MenuTreeItem item)
        {
            row.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                m_DragPointerDown = true;
                m_DragCandidate   = item;
                m_DragStartPos    = evt.position;
                m_DragStarted     = false;
            });

            row.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (m_DragStarted || m_DragPointerDown == false) return;
                if (ReferenceEquals(m_DragCandidate, item) == false) return;
                if (item.Draggable == false) return;
                if ((evt.position - m_DragStartPos).magnitude <= DragThreshold) return;

                m_DragStarted = true;
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(MenuItemDragKey, item);
                DragAndDrop.StartDrag(string.IsNullOrEmpty(item.Name) ? "MenuTreeItem" : item.Name);
            });

            row.RegisterCallback<PointerUpEvent>(_ => ResetDragState());

            // 作为落点: 只认本窗口发起的项拖拽(外部拖拽由面板那一层处理)
            row.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                var dragged = DragAndDrop.GetGenericData(MenuItemDragKey) as MenuTreeItem;
                if (dragged == null || ReferenceEquals(dragged, item)) return;
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                evt.StopPropagation();
            });

            row.RegisterCallback<DragPerformEvent>(evt =>
            {
                var dragged = DragAndDrop.GetGenericData(MenuItemDragKey) as MenuTreeItem;
                if (dragged == null) return;

                DragAndDrop.AcceptDrag();
                evt.StopPropagation();

                try
                {
                    var (parent, index) = ResolveDropTarget(item, evt.mousePosition, row);
                    m_MenuTree?.MoveItem(dragged, parent, index);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EditTool] 菜单项移动失败 {item.Name}\n{e}");
                }
                finally
                {
                    ResetDragState();
                }
            });
        }

        /// <summary>
        /// 落点: 悬停行**上半** → 插到它前面(同一父级); **下半** → 有子级就落为第一个子项, 否则插到它后面。
        /// 与常见树控件一致 —— 想放进某个文件夹, 就往它下半部分拖。
        /// </summary>
        private (MenuTreeItem parent, int index) ResolveDropTarget(MenuTreeItem hovered, Vector2 mousePos, VisualElement row)
        {
            var list = hovered.Parent != null ? hovered.Parent.ChildMenuItems : m_MenuTree.MenuItems;
            var idx  = list.IndexOf(hovered);

            // 落点按鼠标落在行的哪一半算: mousePosition 与 worldBound 同为面板坐标系, 无需换算
            var bound = row.worldBound;
            var upper = bound.height > 0f && mousePos.y < bound.yMin + bound.height * 0.5f;

            if (upper) return (hovered.Parent, idx < 0 ? 0 : idx);
            if (hovered.HasChildren) return (hovered, 0);
            return (hovered.Parent, idx < 0 ? list.Count : idx + 1);
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

        /// <summary>事件 target 可能是行内的图标 / 文字, 沿父链找回它所属的那一项。</summary>
        private static MenuTreeItem ResolveItemFromElement(VisualElement ve)
        {
            while (ve != null)
            {
                if (ve.userData is MenuTreeItem item) return item;
                ve = ve.parent;
            }

            return null;
        }

        private void ResetDragState()
        {
            m_DragPointerDown = false;
            m_DragStarted     = false;
            m_DragCandidate   = null;
        }

        /// <summary>
        /// 外部 Unity 对象拖到 target 节点上: 默认不接收, 子类重写决定语义
        /// (要在菜单里加项的话, 记得自己调 <see cref="RebuildMenuTree"/> 刷新)。
        /// </summary>
        protected virtual void OnDropExternalObjects(MenuTreeItem target, UnityEngine.Object[] objects) { }

        /// <summary>项被重排: 树的形状变了, 重建左侧 UI 并保住当前过滤关键字。</summary>
        private void OnMenuItemsReordered()
        {
            if (m_MenuPanel == null || m_MenuTree == null) return;

            m_MenuPanel.Clear();
            BuildMenuUI(m_MenuPanel);

            if (m_SearchFilter != null)
            {
                m_SearchFilter.value = m_Filter;   // 回填(变更回调会顺带重套一次过滤)
                ApplyFilter(m_Filter);             // 幂等, 保证树的显隐与关键字一致
            }

            RefreshMenuSelection();
        }

        private void SetItemExpanded(MenuTreeItem item, bool expanded)
        {
            item.Toggled = expanded;
            if (m_MenuRowMap == null || !m_MenuRowMap.TryGetValue(item, out var rowRef)) return;
            if (rowRef.Foldout != null) rowRef.Foldout.text = expanded ? "▾" : "▸";
            if (rowRef.Children != null) rowRef.Children.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
            RebuildVisibleOrder();
        }

        private void RefreshMenuSelection()
        {
            if (m_MenuRowMap == null) return;
            foreach (var kv in m_MenuRowMap)
            {
                var selected = kv.Key.IsSelected;
                kv.Value.Row.style.backgroundColor = selected ? MenuSelectColor : Color.clear;
                kv.Value.Label.style.color = selected ? Color.white : NormalMenuTextColor;
            }
        }

        //-------------------------------------- 分割条 --------------------------------------

        //-------------------------------------- 外壳（仅指本窗口 UIElements 布局：菜单+分割条，非引擎的宿主/后端/会话概念） --------------------------------------

        /// <summary>空态提示(没选中项时显示); 放在驱动容器**外面**, 免得被按位置复用换掉。</summary>
        private HelpBox m_ContentEmptyHint;

        private bool m_SplitterResizing;

        /// <summary>
        /// 排布: 左树(菜单) + 可拖分割条 + 右内容(引擎后端)。
        /// </summary>
        protected override void SetupUI(VisualElement root)
        {
            // 走到这里时 Initialize 已经跑过(基类 DrawWindow.CreateGUI 先 EnsureInitialize 再构建), 直接建树
            EnsureMenuTree();

            if (m_MenuTree == null)
            {
                // 没有菜单: 退化成普通窗口
                base.SetupUI(root);
                return;
            }

            root.style.flexDirection = FlexDirection.Row;

            m_MenuPanel = new VisualElement
            {
                style =
                {
                    width = MenuWidth,
                    flexGrow = 0f,
                    flexShrink = 0f,
                },
            };
            root.Add(m_MenuPanel);

            root.Add(BuildSplitter());

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1f;
            // ScrollView 的内容组默认按"内容高"收缩: 内容比外框短时下方就留一条空带, 内容框贴不住外框。
            // 显式撑满视口, 里面的引擎内容区(flexGrow=1)才可能跟着外框一起拉伸/收缩。
            scroll.contentContainer.style.flexGrow = 1f;
            root.Add(scroll);

            // 外框尺寸变了(窗口拉伸 / 隐藏后重开): 把内容区可用下沿报给引擎,
            // 表格数据区据此填满外框剩下的高度(见 HostState.SetContentBottom)。
            scroll.RegisterCallback<GeometryChangedEvent>(_ => ReportContentBottom(scroll));

            m_ContentEmptyHint = new HelpBox("请选择左侧菜单项", HelpBoxMessageType.Info);
            m_ContentEmptyHint.style.marginTop = EmptyHintTopMargin;
            scroll.contentContainer.Add(m_ContentEmptyHint);

            CreateContentArea(scroll.contentContainer, RedrawContent, false);

            BuildMenuUI(m_MenuPanel);
            RefreshMenuSelection();   // 把初始选中项的高亮画上

            // 键盘导航: 在根容器注册; 搜索框输入中(evt.target 为 TextField)不拦截
            root.RegisterCallback<KeyDownEvent>(OnMenuKeyDown);

            // 菜单选中项变了: 右侧内容要重画、菜单高亮要刷新。
            // 统一在 SubscribeSelectionEvents 里幂等订阅(首次构建 / 外壳(UI 布局)重建 / 强制重建共用), 避免重复回调。
            SubscribeSelectionEvents();
        }

        /// <summary>上一次报给引擎的内容区下沿(面板坐标), 用来判断外框是否真的变了。</summary>
        private float m_LastContentBottom;

        /// <summary>
        /// 把内容区的可用下沿交给引擎: 表格的数据区按"可用下沿 − 当前位置"填满外框剩下的高度,
        /// 底边贴着外框下沿, 窗口拉高时跟着拉长(见 <c>HostState.SetContentBottom</c> / <c>TableFillHeight</c>)。
        ///
        /// 只在值变化时补一次重绘: 重绘不会改变视口高度, 不会自己喂自己。
        /// 滚动位置要折算掉 —— 否则一滚滚轮"还剩多少高"就变, 表格会跟着长/缩。
        /// </summary>
        private void ReportContentBottom(ScrollView scroll)
        {
            var bottom = scroll.worldBound.yMax - scroll.scrollOffset.y;
            if (bottom <= 1f) return;
            if (Mathf.Abs(bottom - m_LastContentBottom) <= 0.5f) return;

            m_LastContentBottom = bottom;
            m_Session.SetContentBottom(bottom);

            // 延后重绘: 回调发生在布局阶段, 当场重画等于在事件处理里重入布局。
            scroll.schedule.Execute(RedrawContent);
        }

        private void OnMenuSelectionChanged(SelectionChangedType type)
        {
            RefreshMenuSelection();
            ScrollSelectedIntoView();
            RedrawContent();
        }

        //-------------------------------------- 搜索 / 键盘导航 --------------------------------------

        /// <summary>按关键字过滤菜单: 只改行 display, 不重建树; 命中项强制展开其祖先路径。</summary>
        private void ApplyFilter(string q)
        {
            if (m_MenuRowMap == null) return;
            m_Filter = (q ?? "").Trim();
            bool empty = m_Filter.Length == 0;

            if (empty)
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

        /// <summary>把当前选中项滚入菜单视口(选中变化后调用)。</summary>
        private void ScrollSelectedIntoView()
        {
            if (m_MenuScroll == null || m_MenuTree == null) return;
            var item = m_MenuTree.Selection.SelectedValue;
            if (item == null || m_MenuRowMap == null || !m_MenuRowMap.TryGetValue(item, out var rowRef)) return;

            var rowBound = rowRef.Row.worldBound;
            var viewBound = m_MenuScroll.worldBound;
            var off = m_MenuScroll.scrollOffset;
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
            var idx = current != null ? m_VisibleOrder.IndexOf(current) : -1;

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
                    char c = char.ToLowerInvariant(evt.character);
                    for (int i = 1; i <= m_VisibleOrder.Count; i++)
                    {
                        var cand = m_VisibleOrder[(idx + i) % m_VisibleOrder.Count];
                        if (!string.IsNullOrEmpty(cand.Name) && char.ToLowerInvariant(cand.Name[0]) == c)
                        {
                            idx = (idx + i) % m_VisibleOrder.Count;
                            break;
                        }
                    }
                    break;
            }

            evt.StopPropagation();
            if (idx >= 0 && idx < m_VisibleOrder.Count)
                m_VisibleOrder[idx].Select();
        }

        /// <summary>右侧内容: 选中项有值就画它, 否则只留空态提示。</summary>
        private void RedrawContent()
        {
            var target = m_MenuTree?.Selection?.SelectedValue?.Value;

            if (m_ContentEmptyHint != null)
            {
                m_ContentEmptyHint.style.display = target == null ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (target != null) DrawObject(target);
        }

        /// <summary>分割条: 与  版同宽同色(布局只占 1px 的一条竖线), 按住拖动改 MenuWidth。</summary>
        private VisualElement BuildSplitter()
        {
            var splitter = new VisualElement();
            splitter.style.width      = SplitterWidth;
            splitter.style.flexGrow   = 0f;
            splitter.style.flexShrink = 0f;

            var line = new VisualElement();
            line.style.width           = SplitterWidth;
            line.style.flexGrow        = 1f;
            line.style.backgroundColor = SplitterColor;
            splitter.Add(line);

            // 命中区: 绝对定位、横向盖住线两侧, 只负责拖动, 不参与布局(否则又把缝撑宽)
            var handle = new VisualElement();
            handle.style.position = Position.Absolute;
            handle.style.left     = -SplitterHitPadding;
            handle.style.width    = SplitterWidth + SplitterHitPadding * 2f;
            handle.style.top      = 0f;
            handle.style.bottom   = 0f;
            splitter.Add(handle);

            handle.RegisterCallback<PointerDownEvent>(evt =>
            {
                m_SplitterResizing = true;
                handle.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (m_SplitterResizing == false) return;

                // 指针坐标: evt.position 是屏幕/世界坐标, 不能直接用; 用"命中区在面板里的位置 + 指针相对命中区的偏移"
                // 换算成面板坐标, 再减掉根元素的位置 = 距窗口左边的距离(与  版的 mousePosition.x 同义)
                var pointerX = handle.worldBound.x + evt.localPosition.x - rootVisualElement.worldBound.x;

                MenuWidth = pointerX;
                if (m_MenuPanel != null) m_MenuPanel.style.width = MenuWidth;

                evt.StopPropagation();
            });

            handle.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (m_SplitterResizing == false) return;

                m_SplitterResizing = false;
                handle.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            });

            return splitter;
        }
    }
}
