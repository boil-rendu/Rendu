using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace RD.EditorWindows.Editor
{
    /// <summary>
    /// 轻量菜单树编辑器窗口
    /// 左侧绘制 BuildMenuTree 返回的菜单树 右侧绘制当前选中项
    /// </summary>
    public class MenuEditorWindow : EditorWindowBase
    {
        private MenuTree m_MenuTree;
        private bool m_MenuTreeBuilt;

        protected float m_MenuWidth = 300f;
        private Vector2 m_MenuScrollPosition;
        private Vector2 m_ContentScrollPosition;
        private bool m_ResizingSplitter;

        private static GUIStyle s_SelectStyle;
        private static GUIStyle s_ItemStyle;
        private static GUIStyle s_IndentStyle;
        private static Texture2D s_SelectTex;

        /// <summary>
        /// 选中高亮纹理 惰性创建
        /// 不能放在静态构造/字段初始化里:EditorWindow 是 ScriptableObject,
        /// 构造阶段调用 new Texture2D 会触发 SupportsTextureFormatNative 限制
        /// </summary>
        private static Texture2D SelectTex
        {
            get
            {
                if (s_SelectTex == null)
                {
                    s_SelectTex = new Texture2D(1, 1);
                    s_SelectTex.SetPixel(0, 0, new Color(0.24f, 0.45f, 0.9f, 0.9f));
                    s_SelectTex.Apply();
                }

                return s_SelectTex;
            }
        }

        /// <summary>
        /// 菜单栏宽度 对应原 MenuWidth
        /// </summary>
        public virtual float MenuWidth
        {
            get => m_MenuWidth;
            set => m_MenuWidth = Mathf.Max(150f, value);
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
                Debug.LogError($"[Inspector] 菜单树构建失败 {GetType().Name}\n{e}");
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
                    Debug.LogError($"[Inspector] 菜单默认选中失败 {GetType().Name}\n{e}");
                }
            }
        }

        /// <summary>强制重建菜单树(例如构建条件变化后调用)</summary>
        protected void RebuildMenuTree()
        {
            m_MenuTreeBuilt = false;
            m_MenuTree = null;
            EnsureMenuTree();
        }

        protected override void OnGUI()
        {
            // 先走基类的惰性初始化 + 后端对齐(重写 OnGUI 的窗口只能在这里带上这两步)
            EnsureWindowInitialized();

            // UI Toolkit 模式: 左树 / 分割条 / 右内容都改由 UIElements 排布(菜单树本身仍是 IMGUI,
            // 放进 IMGUIContainer), 这里不再画 IMGUI 那一遍 —— 见 BuildToolkitUI
            if (UseToolkitBackend) return;

            EnsureMenuTree();

            if (m_MenuTree == null)
            {
                DrawObjectPanel();
                return;
            }

            GUILayout.BeginHorizontal();

            DrawMenuPanel(m_MenuTree);
            DrawSplitter();
            DrawContentPanel(m_MenuTree);

            GUILayout.EndHorizontal();
        }

        //-------------------------------------- 左侧菜单 --------------------------------------

        private void DrawMenuPanel(MenuTree tree)
        {
            GUILayout.BeginVertical(GUILayout.Width(MenuWidth));

            m_MenuScrollPosition = GUILayout.BeginScrollView(m_MenuScrollPosition);

            GUILayout.Space(4);
            DrawMenuItems(tree.MenuItems);
            GUILayout.Space(4);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawMenuItems(IReadOnlyList<MenuTreeItem> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                GUILayout.BeginHorizontal();

                var hasChildren = item.HasChildren;
                if (hasChildren)
                {
                    var toggled = GUILayout.Toggle(item.Toggled, GUIContent.none, EditorStyles.foldout,
                                                   GUILayout.Width(16), GUILayout.Height(18));
                    if (toggled != item.Toggled)
                    {
                        item.Toggled = toggled;
                    }
                }
                else
                {
                    GUILayout.Space(16);
                }

                var content = new GUIContent(item.Name, item.Icon);
                var style = item.IsSelected ? GetSelectStyle() : GetItemStyle();

                if (GUILayout.Button(content, style, GUILayout.Height(18)))
                {
                    if (hasChildren)
                    {
                        item.Toggled = !item.Toggled;
                    }

                    try
                    {
                        item.Select();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Inspector] 菜单选中失败 {item.Name}\n{e}");
                    }
                }

                GUILayout.EndHorizontal();

                if (item.Toggled && hasChildren)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    GUILayout.BeginVertical();
                    DrawMenuItems(item.ChildMenuItems);
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
        }

        /// <summary>菜单项样式工厂: 选中态 = 同一基底 + 选中底纹与白字。</summary>
        private static GUIStyle CreateItemStyle(bool selected)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding =
                {
                    left = 4,
                    right = 4,
                    top = 1,
                    bottom = 1,
                },
            };

            if (selected)
            {
                style.normal.background = SelectTex;
                style.normal.textColor  = Color.white;
            }

            return style;
        }

        private static GUIStyle GetItemStyle()
        {
            return s_ItemStyle ??= CreateItemStyle(false);
        }

        private static GUIStyle GetSelectStyle()
        {
            return s_SelectStyle ??= CreateItemStyle(true);
        }

        //-------------------------------------- 分割条 --------------------------------------

        private void DrawSplitter()
        {
            var rect = GUILayoutUtility.GetRect(5f, 5f, GUILayout.ExpandHeight(true));
            var splitterRect = new Rect(rect.x, rect.y, 5f, Mathf.Max(rect.height, 1f));

            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(splitterRect.x + 2, splitterRect.y, 1f, Mathf.Max(splitterRect.height, 1f)),
                                   new Color(0.15f, 0.15f, 0.15f, 1f));
            }

            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

            var e = Event.current;
            if (e.type == EventType.MouseDown && splitterRect.Contains(e.mousePosition))
            {
                m_ResizingSplitter = true;
                e.Use();
            }
            else if (m_ResizingSplitter && e.type == EventType.MouseUp)
            {
                m_ResizingSplitter = false;
                e.Use();
            }
            else if (m_ResizingSplitter && e.type == EventType.MouseDrag)
            {
                MenuWidth = e.mousePosition.x;
                e.Use();
                Repaint();
            }
        }

        //-------------------------------------- 右侧内容 --------------------------------------

        private void DrawContentPanel(MenuTree tree)
        {
            GUILayout.BeginVertical();

            var selected = tree.Selection.Count > 0 ? tree.Selection[0] : null;
            var target = selected?.Value;

            if (target == null)
            {
                GUILayout.Space(30);
                EditorGUILayout.HelpBox("请选择左侧菜单项", MessageType.Info);
            }
            else
            {
                m_ContentScrollPosition = GUILayout.BeginScrollView(m_ContentScrollPosition);
                GUILayout.Space(4);
                DrawTargetObject(target);
                GUILayout.Space(4);
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }

        //-------------------------------------- UI Toolkit 模式的外壳 --------------------------------------

#if UNITY_2021_2_OR_NEWER
        /// <summary>UITK 模式下承载左侧菜单树的容器(菜单仍是原 IMGUI 绘制, 位置与宽度由 UIElements 决定)。</summary>
        private IMGUIContainer m_ToolkitMenu;

        /// <summary>UITK 模式下的空态提示(没选中项时显示); 放在驱动容器**外面**, 免得被按位置复用换掉。</summary>
        private HelpBox m_ToolkitEmptyHint;

        private bool m_ToolkitResizing;

        /// <summary>
        /// 与 IMGUI 版同样的排布(左树 + 可拖分割条 + 右内容), 只是换成 UIElements 布局:
        /// 左树留在 IMGUIContainer 里(观感 / 交互与 IMGUI 模式完全一致), 右侧内容交给引擎的 UI Toolkit 后端。
        /// </summary>
        protected override void BuildToolkitUI(VisualElement root)
        {
            // 走到这里时 Initialize 已经跑过(基类先 EnsureWindowInitialized 再切后端), 直接建树
            EnsureMenuTree();

            if (m_MenuTree == null)
            {
                // 没有菜单: 退化成普通窗口
                base.BuildToolkitUI(root);
                return;
            }

            root.style.flexDirection = FlexDirection.Row;

            m_ToolkitMenu = new IMGUIContainer(DrawToolkitMenu);
            m_ToolkitMenu.style.width      = MenuWidth;
            m_ToolkitMenu.style.flexGrow   = 0f;
            m_ToolkitMenu.style.flexShrink = 0f;
            root.Add(m_ToolkitMenu);

            root.Add(BuildToolkitSplitter());

            var scroll = new ScrollView();
            scroll.style.flexGrow = 1f;
            root.Add(scroll);

            m_ToolkitEmptyHint = new HelpBox("请选择左侧菜单项", HelpBoxMessageType.Info);
            m_ToolkitEmptyHint.style.marginTop = 30f;
            scroll.contentContainer.Add(m_ToolkitEmptyHint);

            CreateToolkitContent(scroll.contentContainer, RedrawToolkitContent, false);

            // 菜单点击不经过绘制后端(左树是 IMGUIContainer 画的), 选中项变了得自己重画右侧。
            // 先退订再订: 开关来回切会重建外壳, 不退订就会重复回调。
            m_MenuTree.Selection.SelectionChanged -= OnToolkitSelectionChanged;
            m_MenuTree.Selection.SelectionChanged += OnToolkitSelectionChanged;
        }

        private void OnToolkitSelectionChanged(SelectionChangedType type)
        {
            RedrawToolkitContent();
        }

        /// <summary>UITK 模式下左侧菜单的绘制体: 与 IMGUI 版共用 DrawMenuPanel。</summary>
        private void DrawToolkitMenu()
        {
            if (m_MenuTree == null) return;
            DrawMenuPanel(m_MenuTree);
        }

        /// <summary>UITK 模式下的右侧内容: 选中项有值就画它, 否则只留空态提示。</summary>
        private void RedrawToolkitContent()
        {
            // 切回 IMGUI 后驱动/选中事件还可能再回调到这里: 那时 tree 已经拆了,
            // 再往下走就是"在 OnGUI 之外调 IMGUI", 必须挡住
            if (UseToolkitBackend == false) return;

            var target = m_MenuTree?.Selection?.SelectedValue?.Value;

            if (m_ToolkitEmptyHint != null)
            {
                m_ToolkitEmptyHint.style.display = target == null ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (target != null) DrawTargetObject(target);
        }

        /// <summary>分割条: 与 IMGUI 版同宽同色(5px 区域里一条 1px 竖线), 按住拖动改 MenuWidth。</summary>
        private VisualElement BuildToolkitSplitter()
        {
            var splitter = new VisualElement();
            splitter.style.width      = 5f;
            splitter.style.flexGrow   = 0f;
            splitter.style.flexShrink = 0f;

            var line = new VisualElement();
            line.style.width           = 1f;
            line.style.flexGrow        = 1f;
            line.style.marginLeft      = 2f;
            line.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            splitter.Add(line);

            splitter.RegisterCallback<PointerDownEvent>(evt =>
            {
                m_ToolkitResizing = true;
                splitter.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            });

            splitter.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (m_ToolkitResizing == false) return;

                // 指针坐标: evt.position 是屏幕/世界坐标, 不能直接用; 用"分割条在面板里的位置 + 指针相对分割条的偏移"
                // 换算成面板坐标, 再减掉根元素的位置 = 距窗口左边的距离(与 IMGUI 版的 mousePosition.x 同义)
                var pointerX = splitter.worldBound.x + evt.localPosition.x - rootVisualElement.worldBound.x;

                MenuWidth = pointerX;
                if (m_ToolkitMenu != null) m_ToolkitMenu.style.width = MenuWidth;

                evt.StopPropagation();
            });

            splitter.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (m_ToolkitResizing == false) return;

                m_ToolkitResizing = false;
                splitter.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            });

            return splitter;
        }
#endif
    }
}
