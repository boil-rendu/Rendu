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
    public class MenuEditorWindow : EditorWindowBase
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
            if (m_MenuTree != null)
            {
                m_MenuTree.Selection.SelectionChanged -= OnMenuSelectionChanged;
            }

            m_MenuTreeBuilt = false;
            m_MenuTree = null;
            EnsureMenuTree();

            if (m_MenuTree != null)
            {
                m_MenuTree.Selection.SelectionChanged += OnMenuSelectionChanged;
            }

            if (m_MenuPanel != null)
            {
                m_MenuPanel.Clear();
                BuildMenuUI(m_MenuPanel);
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

        private void BuildMenuUI(VisualElement menuPanel)
        {
            m_MenuRowMap = new Dictionary<MenuTreeItem, MenuRowRef>();

            var scroll = new ScrollView { style = { flexGrow = 1f } };
            menuPanel.Add(scroll);

            var inner = new VisualElement
            {
                style =
                {
                    paddingTop = MenuPadding,
                    paddingBottom = MenuPadding,
                },
            };
            scroll.contentContainer.Add(inner);

            BuildMenuItems(m_MenuTree.MenuItems, inner);
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
                        height = MenuRowHeight,
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

                // 整行点击: 有子级顺手展开/收起, 然后选中(与旧  行为一致)
                row.RegisterCallback<ClickEvent>(_ =>
                {
                    if (item.HasChildren) SetItemExpanded(item, !item.Toggled);
                    try { item.Select(); }
                    catch (Exception e) { Debug.LogError($"[EditTool] 菜单选中失败 {item.Name}\n{e}"); }
                });

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

        private void SetItemExpanded(MenuTreeItem item, bool expanded)
        {
            item.Toggled = expanded;
            if (m_MenuRowMap == null || !m_MenuRowMap.TryGetValue(item, out var rowRef)) return;
            if (rowRef.Foldout != null) rowRef.Foldout.text = expanded ? "▾" : "▸";
            if (rowRef.Children != null) rowRef.Children.style.display = expanded ? DisplayStyle.Flex : DisplayStyle.None;
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

        //-------------------------------------- 外壳 --------------------------------------

        /// <summary>空态提示(没选中项时显示); 放在驱动容器**外面**, 免得被按位置复用换掉。</summary>
        private HelpBox m_ContentEmptyHint;

        private bool m_SplitterResizing;

        /// <summary>
        /// 排布: 左树(菜单) + 可拖分割条 + 右内容(引擎后端)。
        /// </summary>
        protected override void BuildToolkitUI(VisualElement root)
        {
            // 走到这里时 Initialize 已经跑过(基类 DrawWindow.CreateGUI 先 EnsureInitialize 再构建), 直接建树
            EnsureMenuTree();

            if (m_MenuTree == null)
            {
                // 没有菜单: 退化成普通窗口
                base.BuildToolkitUI(root);
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

            CreateToolkitContent(scroll.contentContainer, RedrawContent, false);

            BuildMenuUI(m_MenuPanel);
            RefreshMenuSelection();   // 把初始选中项的高亮画上

            // 菜单选中项变了: 右侧内容要重画、菜单高亮要刷新。
            // 先退订再订: 外壳重建时会重跑这里, 不退订就会重复回调。
            m_MenuTree.Selection.SelectionChanged -= OnMenuSelectionChanged;
            m_MenuTree.Selection.SelectionChanged += OnMenuSelectionChanged;
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
            RedrawContent();
        }

        /// <summary>右侧内容: 选中项有值就画它, 否则只留空态提示。</summary>
        private void RedrawContent()
        {
            var target = m_MenuTree?.Selection?.SelectedValue?.Value;

            if (m_ContentEmptyHint != null)
            {
                m_ContentEmptyHint.style.display = target == null ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (target != null) DrawTargetObject(target);
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
