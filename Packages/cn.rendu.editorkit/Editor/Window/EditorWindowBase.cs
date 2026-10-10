using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RD.EditorKit.Editor
{
    /// <summary>
    /// 轻量编辑器窗口基类
    /// 默认在 OnGUI 中依据 Inspector 属性自动绘制窗口自身成员
    /// </summary>
    public class EditorWindowBase : UnityEditor.EditorWindow
    {
        private bool m_Initialized;
        private Vector2 m_ScrollPosition;

        /// <summary>本窗口的状态会话: 生命周期与窗口一致, 窗口关闭即释放。</summary>
        private readonly HostSession m_Session = new();

        /// <summary>
        /// 首次显示前调用一次 子类可重写做初始化
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// 窗口关闭销毁时调用 子类可重写做清理
        /// </summary>
        protected virtual void OnWindowDestroy()
        {
        }

        private void EnsureInitialize()
        {
            if (m_Initialized) return;
            m_Initialized = true;

            try
            {
                Initialize();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口初始化失败 {GetType().Name}\n{e}");
            }
        }

        /// <summary>
        /// 子类 OnGUI 覆盖时也需先完成惰性初始化
        /// </summary>
        protected void EnsureWindowInitialized()
        {
            EnsureInitialize();
        }

        protected virtual void OnGUI()
        {
            EnsureInitialize();
            DrawObjectPanel();
        }

        /// <summary>
        /// 以滚动视图绘制宿主窗口对象
        /// </summary>
        protected void DrawObjectPanel()
        {
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
            Engine.DrawObject(this, this, null, null, m_Session);
            GUILayout.EndScrollView();
        }

        /// <summary>用本窗口的会话绘制任意对象, 供子类复用。</summary>
        protected void DrawTargetObject(object target)
        {
            Engine.DrawObject(target, this, null, null, m_Session);
        }

        private void OnEnable()
        {
            // 消息占位:统一走 EnsureInitialize 惰性初始化
        }

        private void OnDestroy()
        {
            try
            {
                OnWindowDestroy();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Inspector] 窗口销毁失败 {GetType().Name}\n{e}");
            }
        }
    }

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
            EnsureWindowInitialized();
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

        private static GUIStyle GetItemStyle()
        {
            if (s_ItemStyle != null) return s_ItemStyle;

            s_ItemStyle = new GUIStyle(GUI.skin.label)
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
            return s_ItemStyle;
        }

        private static GUIStyle GetSelectStyle()
        {
            if (s_SelectStyle != null) return s_SelectStyle;

            s_SelectStyle = new GUIStyle(GUI.skin.label)
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
            s_SelectStyle.normal.background = SelectTex;
            s_SelectStyle.normal.textColor = Color.white;
            return s_SelectStyle;
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
    }
}
