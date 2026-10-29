using System;
using System.Collections.Generic;
using RD.Draw;
using RD.Draw.Editor;
using RD.Draw.Editor.Selectors;
using UnityEditor;
using UnityEngine;

namespace RD.EditTool.Editor.Selectors
{
    /// <summary>
    /// 类型选择器浮窗（住在 cn.rendu.edittool：它是 EditorWindow 宿主，不该在纯引擎 RD.Draw 里）。
    ///
    /// 刻意做成**独立窗口 + 纯 **, 不经过驱动抽象（直接 ）:
    /// 它不属于某个面板的绘制序列, 在  /  两种后端下都是从引擎的按钮回调里
    /// 弹出来的一次性交互, 自己画自己的最省事, 也不会打乱宿主侧的布局。
    ///
    /// 形态是一条紧凑的选型弹窗: 顶部一条工具栏(搜索框 + 三枚图标键), 下面直接是
    /// "None + 候选类型"的紧凑列表 —— 没有标题栏、没有页脚、不显示命名空间列。
    /// 无标题栏靠 <see cref="EditorWindow.ShowAsDropDown(Rect, Vector2)"/> 实现, 点外面自动收起。
    ///
    /// 工具栏三枚键(都带悬停说明):
    ///   - ≡  按命名空间分组 / 平铺(对候选多的基类型有用);
    ///   - 眼睛 显示 / 隐藏 None 行(AllowNone 为 false 时不出现);
    ///   - 齿轮 打开 Unity 偏好设置 —— 本引擎还没有选择器专属设置页,
    ///     先落到通用偏好设置, 有设置项后再换。
    ///
    /// 交互: 输入即过滤(名称 / 全名), ↑↓ 高亮, Enter 确认, Esc 取消, 点窗口外面也算取消(不回调)。
    ///
    /// 自注册: 本类通过 <see cref="InitializeOnLoadAttribute"/> 把自身登记为 RD.Draw 的
    /// <see cref="TypeSelector.PickerOpener"/>, 这样纯引擎 RD.Draw 不持有任何 EditorWindow、
    /// 也不反向依赖宿主包（避免循环依赖）。
    /// </summary>
    [InitializeOnLoad]
    internal sealed class TypeSelectorWindow : EditorWindow
    {
        static TypeSelectorWindow()
        {
            // 把"打开浮窗"的具体实现交给本窗口；RD.Draw 只声明委托，不引用本类。
            TypeSelector.PickerOpener = Open;
        }

        private const string SearchControlName = "RD.TypeSelector.Search";

        private const float WindowWidth   = 260f;
        private const float ToolbarHeight = 20f;
        private const float RowHeight     = 22f;
        private const float GroupHeight   = 18f;
        private const float IconSize      = 16f;
        private const int   MaxVisibleRows = 12;

        /// <summary>选中行底纹: 与菜单选中行同一档蓝, 半透明让图标与文字仍然清楚。</summary>
        private static readonly Color SelectedRowColor = new(0.24f, 0.45f, 0.9f, 0.45f);

        /// <summary>悬停行底纹: 只提亮一点点, 用来提示"这行点得中"。</summary>
        private static readonly Color HoverRowColor = new(1f, 1f, 1f, 0.06f);

        private Action<Type> m_OnPicked;
        private bool m_AllowNone;
        private Type m_Current;

        private List<Type> m_Candidates = new();
        private readonly List<Type> m_Filtered = new();
        private string m_Search = string.Empty;
        private Vector2 m_Scroll;
        private int m_Index;
        private int m_Frames;
        private bool m_Closed;

        private bool m_GroupByNamespace;
        private bool m_ShowNone;

        private GUIStyle m_NameStyle;
        private GUIStyle m_GroupStyle;

        private static Texture s_ClassIcon;
        private static Texture s_EyeIcon;
        private static Texture s_EyeSlashIcon;

        //-------------------------------------- 打开 --------------------------------------

        public static void Open(Type baseType, Action<Type> onPicked, bool allowNone, Type current,
                                Vector2? screenAnchor)
        {
            var window = CreateInstance<TypeSelectorWindow>();
            window.m_OnPicked  = onPicked;
            window.m_AllowNone = allowNone;
            window.m_ShowNone  = allowNone;
            window.m_Current   = current;
            window.m_Candidates = new List<Type>(TypeCandidates.Get(baseType));

            window.titleContent = new GUIContent($"选择类型 · {baseType.Name}");

            var anchor = screenAnchor ?? ResolveAnchor();
            var rows   = Mathf.Clamp(window.m_Candidates.Count + (allowNone ? 1 : 0), 1, MaxVisibleRows);
            var size   = new Vector2(WindowWidth, ToolbarHeight + rows * RowHeight + 2f);

            window.ResetFilter();

            // 无标题栏的弹出式窗口: 紧凑、贴着"+"弹出, 点别处自动收起
            window.ShowAsDropDown(new Rect(anchor.x, anchor.y, 0f, 0f), size);
            window.Focus();
            window.Repaint();
        }

        /// <summary>没有鼠标坐标可选时的落点: 当前窗口正中(通常就是正在编辑的那个检视器)。</summary>
        private static Vector2 ResolveAnchor()
        {
            var focused = focusedWindow;
            return focused != null ? focused.position.center : new Vector2(400f, 240f);
        }

        //-------------------------------------- 绘制 --------------------------------------

        private void OnGUI()
        {
            if (m_NameStyle == null) CreateStyles();
            m_Frames++;

            // 键盘要在输入框之前拦: 否则 ↑↓ 被 TextField 当成光标移动, 永远选不到下一行
            HandleKeyboard();

            DrawToolbar();
            DrawList();

            if (m_Frames == 1) GUI.FocusControl(SearchControlName);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.SetNextControlName(SearchControlName);
            var search = GUILayout.TextField(m_Search ?? string.Empty, EditorStyles.toolbarSearchField,
                                             GUILayout.ExpandWidth(true));
            if (string.Equals(search, m_Search, StringComparison.Ordinal) == false)
            {
                m_Search = search;
                ResetFilter();
            }

            var groupContent = new GUIContent("≡", "按命名空间分组 / 平铺");
            if (GUILayout.Button(groupContent, EditorStyles.toolbarButton, GUILayout.Width(26f)))
            {
                m_GroupByNamespace = m_GroupByNamespace == false;
                Repaint();
            }

            if (m_AllowNone)
            {
                var eyeContent = new GUIContent(m_ShowNone ? EyeIcon : EyeSlashIcon, "显示 / 隐藏 None");
                if (GUILayout.Button(eyeContent, EditorStyles.toolbarButton, GUILayout.Width(26f)))
                {
                    m_ShowNone = m_ShowNone == false;
                    ResetFilter();
                }
            }

            var settings = new GUIContent(SettingsIcon, "打开偏好设置");
            if (GUILayout.Button(settings, EditorStyles.toolbarButton, GUILayout.Width(26f)))
            {
                OpenPreferences();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll, false, false);

            if (m_ShowNone) DrawRow(0, null);

            var offset         = m_ShowNone ? 1 : 0;
            var lastNamespace  = (string)null;

            for (var i = 0; i < m_Filtered.Count; i++)
            {
                var type = m_Filtered[i];
                if (m_GroupByNamespace)
                {
                    var ns = string.IsNullOrEmpty(type.Namespace) ? "(全局)" : type.Namespace;
                    if (string.Equals(ns, lastNamespace, StringComparison.Ordinal) == false)
                    {
                        DrawGroupHeader(ns);
                        lastNamespace = ns;
                    }
                }

                DrawRow(i + offset, type);
            }

            if (m_ShowNone == false && m_Filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("没有可用的类型", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawGroupHeader(string name)
        {
            var rect = GUILayoutUtility.GetRect(0f, GroupHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.12f));
                GUI.Label(new Rect(rect.x + 6f, rect.y, rect.width - 12f, rect.height), name, m_GroupStyle);
            }
        }

        private void DrawRow(int row, Type type)
        {
            var rect  = GUILayoutUtility.GetRect(0f, RowHeight, GUILayout.ExpandWidth(true));
            var hover = rect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                if (row == m_Index) EditorGUI.DrawRect(rect, SelectedRowColor);
                else if (hover) EditorGUI.DrawRect(rect, HoverRowColor);

                var icon = IconFor(type);
                if (icon != null)
                {
                    GUI.DrawTexture(new Rect(rect.x + 6f, rect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize),
                                    icon, ScaleMode.ScaleToFit);
                }
            }
            else if (Event.current.type == EventType.MouseMove && hover)
            {
                // 悬停高亮只在重绘时画, 鼠标进来必须主动排一次, 否则要等到下一次输入才亮
                Repaint();
            }

            var textX     = rect.x + 6f + IconSize + 6f;
            var textWidth = Mathf.Max(rect.xMax - textX - 6f, 0f);
            var name      = type == null ? "None" : type.Name;

            // 悬停提示用全名: 列表里只显示短名, 同名不同命名空间的类型靠它区分
            GUI.Label(new Rect(textX, rect.y, textWidth, rect.height),
                      new GUIContent(name, type?.FullName ?? "置空 (null)"), m_NameStyle);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && hover)
            {
                m_Index = row;
                Pick(type);
                Event.current.Use();
            }
        }

        //-------------------------------------- 交互 --------------------------------------

        private void HandleKeyboard()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown) return;

            switch (e.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveSelection(1);
                    e.Use();
                    break;
                case KeyCode.UpArrow:
                    MoveSelection(-1);
                    e.Use();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (m_Index >= 0 && m_Index < RowCount) Pick(TypeAtRow(m_Index));
                    e.Use();
                    break;
                case KeyCode.Escape:
                    CloseWindow();
                    e.Use();
                    break;
            }
        }

        private void MoveSelection(int delta)
        {
            var count = RowCount;
            if (count == 0) return;

            m_Index = Mathf.Clamp(m_Index + delta, 0, count - 1);
            ScrollToSelection();
            Repaint();
        }

        /// <summary>键盘移动时把高亮行滚进视口(估算值即可: 只求"不会选到看不见的行")。</summary>
        private void ScrollToSelection()
        {
            var top        = m_Index * RowHeight;
            var bottom     = top + RowHeight;
            var viewHeight = Mathf.Max(position.height - ToolbarHeight, RowHeight);

            if (top < m_Scroll.y)
            {
                m_Scroll.y = Mathf.Max(top - RowHeight, 0f);
            }
            else if (bottom > m_Scroll.y + viewHeight)
            {
                m_Scroll.y = bottom - viewHeight;
            }
        }

        private void Pick(Type type)
        {
            if (m_Closed) return;

            CloseWindow();

            var callback = m_OnPicked;
            m_OnPicked = null;
            callback?.Invoke(type);
        }

        private void CloseWindow()
        {
            if (m_Closed) return;
            m_Closed = true;
            Close();
        }

        /// <summary>点窗口外面 = 取消。首帧的焦点抖动不算(弹出窗口建立时会先失焦一次)。</summary>
        private void OnLostFocus()
        {
            if (m_Frames < 2) return;
            CloseWindow();
        }

        /// <summary>
        /// 打开 Unity 偏好设置。macOS 的 Preferences 在 Unity 菜单下、Windows/Linux 在 Edit 菜单下,
        /// 所以两条路径都试一次 —— 失败的那条 ExecuteMenuItem 只是返回 false, 不会抛。
        ///
        /// 说明: 本引擎还没有选择器专属设置页, 先落到通用偏好设置,
        /// 将来有设置项再换成自己的入口。
        /// </summary>
        private static void OpenPreferences()
        {
            if (EditorApplication.ExecuteMenuItem("Edit/Preferences...") == false)
            {
                EditorApplication.ExecuteMenuItem("Unity/Preferences...");
            }
        }

        //-------------------------------------- 过滤与索引 --------------------------------------

        private int RowCount => m_Filtered.Count + (m_ShowNone ? 1 : 0);

        /// <summary>行号 → 类型; "None" 行返回 null。</summary>
        private Type TypeAtRow(int row)
        {
            if (m_ShowNone)
            {
                if (row <= 0) return null;
                row--;
            }

            return row >= 0 && row < m_Filtered.Count ? m_Filtered[row] : null;
        }

        private void ResetFilter()
        {
            m_Filtered.Clear();

            var search = (m_Search ?? string.Empty).Trim();
            foreach (var type in m_Candidates)
            {
                if (search.Length == 0 || Matches(type, search)) m_Filtered.Add(type);
            }

            // 默认高亮"当前值 > 第一个候选": 不主动停在 None 上, 免得一个回车就往集合里加 null
            m_Index = 0;
            if (m_Filtered.Count > 0)
            {
                var wanted = m_Current != null ? m_Filtered.IndexOf(m_Current) : -1;
                m_Index = (wanted >= 0 ? wanted : 0) + (m_ShowNone ? 1 : 0);
            }

            Repaint();
        }

        private static bool Matches(Type type, string search)
        {
            if (type.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return type.FullName != null &&
                   type.FullName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        //-------------------------------------- 样式与图标 --------------------------------------

        private void CreateStyles()
        {
            m_NameStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping  = TextClipping.Clip,
                padding   = new RectOffset(0, 0, 0, 0),
                margin    = new RectOffset(0, 0, 0, 0),
            };

            m_GroupStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping  = TextClipping.Clip,
                padding   = new RectOffset(0, 0, 0, 0),
                margin    = new RectOffset(0, 0, 0, 0),
            };
        }

        /// <summary>候选里已经没有 Unity 对象(见 <see cref="TypeCandidates"/>), 统一用脚本图标即可。</summary>
        private static Texture IconFor(Type type)
        {
            return type == null ? null : ClassIcon;
        }

        private static Texture ClassIcon
        {
            get
            {
                if (s_ClassIcon == null) s_ClassIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
                return s_ClassIcon;
            }
        }

        private static Texture EyeIcon
        {
            get
            {
                if (s_EyeIcon == null) s_EyeIcon = EditorIcons.FromSdf(SdfIconType.Eye);
                return s_EyeIcon;
            }
        }

        private static Texture EyeSlashIcon
        {
            get
            {
                if (s_EyeSlashIcon == null) s_EyeSlashIcon = EditorIcons.FromSdf(SdfIconType.EyeSlash);
                return s_EyeSlashIcon;
            }
        }

        private static Texture SettingsIcon => EditorIcons.SettingsCog;
    }
}
