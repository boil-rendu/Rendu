using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RD.Panel.Editor
{
    /// <summary>
    /// IMGUI 后端: 把 <see cref="IPanelDriver"/> 的调用翻译成本地 EditorGUILayout 调用。
    /// 这是改造前绘制代码的一比一平移 —— 行为应当**逐项**与改造前一致。
    /// </summary>
    public sealed class ImGuiDriver : IPanelDriver
    {
        private readonly EditorWindow m_Host;

        private readonly Stack<Color> m_Colors      = new();
        private readonly Stack<bool>  m_EnabledTags = new();
        private readonly Stack<float> m_LabelWidths = new();

        /// <summary>Unity 的 BeginChangeCheck 不支持可靠嵌套, 这里只在最外层真正打开一次。</summary>
        private int  m_CheckDepth;
        private bool m_CheckOpened;

        // 样式缓存必须是静态的: 本后端每次绘制都可能重新构造, 而样式活到域重载(每帧 new GUIStyle 太贵)。
        private static readonly Dictionary<(bool bold, bool mini, PanelAlign align, bool wrap, bool rich), GUIStyle>
            s_TextStyles = new();

        /// <summary>上一次出现的 Scroll / options, 详见 <see cref="ToolbarContents"/>。 </summary>
        private static string[]     s_LastToolbarTexts;
        private static GUIContent[] s_LastToolbarContents;

        /// <summary>复用的 GUIContent: 编辑器 UI 单线程, 一次调用内覆写 text/image/tooltip 即为零分配。</summary>
        private readonly GUIContent m_ReusableContent = new();

        public ImGuiDriver(EditorWindow host)
        {
            m_Host = host;
        }

        //-------------------------------------- 度量 --------------------------------------

        public float LineHeight      => EditorGUIUtility.singleLineHeight;
        public float ScrollbarHeight => GUI.skin.horizontalScrollbar.fixedHeight;
        public float ViewWidth       => EditorGUIUtility.currentViewWidth;

        //-------------------------------------- 生命周期 --------------------------------------

        public void BeginPanel()
        {
            // 不动 EditorGUIUtility.labelWidth: IMGUI 就用 Unity 自己的值, UI Toolkit 那边也来读同一个值
            // (见 PanelMetrics.LabelWidth), 两边不再是两套算法。
        }

        public void EndPanel() { }

        public void RequestRepaint()
        {
            // IMGUI 是立即模式: 数据改了不重绘就看不见; 保留模式这一步是多余的。
            m_Host?.Repaint();
        }

        //-------------------------------------- 容器 --------------------------------------

        public void BeginVertical(bool boxed, float width = 0f)
        {
            // 不缓存 box 样式: GUI.skin 在域重载后会整体换掉, 缓存下来的那份就成了死对象
            var style = boxed ? GUI.skin.GetStyle("box") : GUIStyle.none;
            GUILayout.BeginVertical(style, WidthOnly(width));
        }

        public void EndVertical()
        {
            GUILayout.EndVertical();
        }

        public void BeginHorizontal(bool boxed = false)
        {
            // IMGUI 的水平容器没有"加框"的用法: 行范围内的 box 由外层垂直容器承担
            EditorGUILayout.BeginHorizontal();
        }

        public void EndHorizontal()
        {
            EditorGUILayout.EndHorizontal();
        }

        public void BeginIndent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(IndentWidth);
            GUILayout.BeginVertical();
        }

        public void EndIndent()
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        public void BeginScroll(ref Vector2 scroll, float maxHeight)
        {
            // 高度有界, 避免与外层滚动视图互相抢空间
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(maxHeight), GUILayout.ExpandHeight(false));
        }

        public void EndScroll()
        {
            GUILayout.EndScrollView();
        }

        public void Space(float pixels)
        {
            GUILayout.Space(pixels);
        }

        public void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        public void Separator(float thickness, Color color)
        {
            var rect = GUILayoutUtility.GetRect(ViewWidth, thickness);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, color);
            }
        }

        private const float IndentWidth = PanelMetrics.IndentWidth;

        private static readonly GUILayoutOption[] s_EmptyOptions = new GUILayoutOption[0];
        private static readonly GUILayoutOption[] s_WidthSlot    = new GUILayoutOption[1];

        /// <summary>params 重载不接受 null, 也没必要为定宽分配数组: 单槽复用即可(调用方不会嵌套使用它)。</summary>
        private static GUILayoutOption[] WidthOnly(float width)
        {
            if (width <= 0f) return s_EmptyOptions;

            s_WidthSlot[0] = GUILayout.Width(width);
            return s_WidthSlot;
        }

        private readonly GUILayoutOption[] m_ButtonOptions = new GUILayoutOption[3];

        private GUILayoutOption[] ButtonOptions(in PanelButtonSpec spec)
        {
            var count = 0;
            if (spec.Width > 0f) m_ButtonOptions[count++] = GUILayout.Width(spec.Width);
            if (spec.Height > 0f)
            {
                m_ButtonOptions[count++] = GUILayout.Height(spec.Height);
                m_ButtonOptions[count++] = GUILayout.MinHeight(spec.Height);
            }
            // 没指定高度就不锁高: 用 GUI.skin.button 自己的高度(这个值就是 PanelMetrics.ButtonHeight
            // 的来源, UITK 那边按它对齐), 反过来给 IMGUI 塞一个外来高度才是本末倒置
            else if (spec.ExpandWidth == false && count < m_ButtonOptions.Length)
            {
                m_ButtonOptions[count++] = GUILayout.ExpandWidth(false);
            }

            return Slice(count);
        }

        private GUILayoutOption[] Slice(int count)
        {
            if (count == 0) return s_EmptyOptions;

            // GUILayoutOption[] 只是个 params 数组, 多余的位置必须裁掉 —— 带着脏尾巴会被 Unity 当成 option
            var slice = new GUILayoutOption[count];
            for (var i = 0; i < count; i++)
            {
                slice[i] = m_ButtonOptions[i];
            }

            return slice;
        }

        //-------------------------------------- 状态栈 --------------------------------------

        public void PushColor(Color color)
        {
            m_Colors.Push(GUI.color);
            GUI.color = color;
        }

        public void PopColor()
        {
            if (m_Colors.Count == 0) return;
            GUI.color = m_Colors.Pop();
        }

        public void PushEnabled(bool enabled)
        {
            m_EnabledTags.Push(enabled);
            EditorGUI.BeginDisabledGroup(enabled == false);
        }

        public void PopEnabled()
        {
            if (m_EnabledTags.Count == 0) return;
            m_EnabledTags.Pop();
            EditorGUI.EndDisabledGroup();
        }

        public void PushLabelWidth(float? width)
        {
            // NaN 作哨兵: 这一层没改过宽度, 退出时也不用写回
            m_LabelWidths.Push(width.HasValue ? EditorGUIUtility.labelWidth : float.NaN);
            if (width.HasValue) EditorGUIUtility.labelWidth = width.Value;
        }

        public void PopLabelWidth()
        {
            if (m_LabelWidths.Count == 0) return;

            var previous = m_LabelWidths.Pop();
            if (float.IsNaN(previous) == false) EditorGUIUtility.labelWidth = previous;
        }

        public void BeginCheck()
        {
            if (m_CheckDepth++ > 0) return;
            m_CheckOpened = true;
            EditorGUI.BeginChangeCheck();
        }

        public bool EndCheck()
        {
            if (m_CheckDepth == 0) return false;
            if (--m_CheckDepth > 0) return false;

            // 只有最外层真正开了 Unity 的观察, 由它负责读出结果
            if (m_CheckOpened == false) return false;
            m_CheckOpened = false;
            return EditorGUI.EndChangeCheck();
        }

        //-------------------------------------- 文本 --------------------------------------

        public void Label(string text, in PanelTextSpec spec)
        {
            // emoji 没有字形, 走内联贴图: 一行里文本段用 Label, emoji 段用贴图
            if (PanelEmoji.Contains(text))
            {
                LabelWithEmoji(text ?? "", spec);
                return;
            }

            var style = TextStyle(spec.Bold, spec.Mini, spec.Align, spec.Wrap, spec.RichText);
            var text0 = text ?? "";

            if (spec.Width > 0f && spec.Height > 0f)
            {
                GUILayout.Label(text0, style, GUILayout.Width(spec.Width), GUILayout.Height(spec.Height));
            }
            else if (spec.Width > 0f)
            {
                GUILayout.Label(text0, style, GUILayout.Width(spec.Width));
            }
            else if (spec.Height > 0f)
            {
                GUILayout.Label(text0, style, GUILayout.Height(spec.Height));
            }
            else
            {
                GUILayout.Label(text0, style);
            }
        }

        public void KeyValueLabel(string label, string value)
        {
            if (PanelEmoji.Contains(value))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth));
                LabelWithEmoji(value ?? "", PanelTextSpec.Of());
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.LabelField(label, value);
        }

        /// <summary>把"文字 + emoji"切成一段段画在同一行里。</summary>
        private void LabelWithEmoji(string text, in PanelTextSpec spec)
        {
            var style = TextStyle(spec.Bold, spec.Mini, spec.Align, spec.Wrap, spec.RichText);

            EditorGUILayout.BeginHorizontal();
            var runs = PanelEmoji.Split(text);
            for (int i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                if (run.IsEmoji)
                {
                    GUILayout.Label(new GUIContent(run.Texture), EmojiStyle(),
                                    GUILayout.Width(PanelEmoji.Size), GUILayout.Height(PanelEmoji.Size));
                }
                else if (string.IsNullOrEmpty(run.Text) == false)
                {
                    GUILayout.Label(run.Text, style);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static GUIStyle s_EmojiStyle;

        private static GUIStyle EmojiStyle()
        {
            if (s_EmojiStyle != null) return s_EmojiStyle;

            s_EmojiStyle = new GUIStyle(GUI.skin.label)
            {
                alignment   = TextAnchor.MiddleCenter,
                padding     = new RectOffset(0, 0, 0, 0),
                margin      = new RectOffset(0, 2, 0, 0),
                fixedWidth  = PanelEmoji.Size,
                fixedHeight = PanelEmoji.Size,
                stretchWidth = false,
            };

            return s_EmojiStyle;
        }

        public void HelpBox(string message, PanelMessageKind kind)
        {
            EditorGUILayout.HelpBox(message, ToMessageType(kind));
        }

        private static MessageType ToMessageType(PanelMessageKind kind)
        {
            switch (kind)
            {
                case PanelMessageKind.Info:    return MessageType.Info;
                case PanelMessageKind.Warning: return MessageType.Warning;
                case PanelMessageKind.Error:   return MessageType.Error;
                default:                       return MessageType.None;
            }
        }

        /// <summary>文本样式按 (粗体 × 迷你 × 对齐 × 换行 × 富文本) 缓存: 标题/表头在热路径上, 每帧 new 太贵。</summary>
        private static GUIStyle TextStyle(bool bold, bool mini, PanelAlign align, bool wrap, bool rich)
        {
            if (bold == false && mini == false && wrap == false && rich == false && align == PanelAlign.Left)
            {
                return EditorStyles.label;
            }

            var key = (bold, mini, align, wrap, rich);
            if (s_TextStyles.TryGetValue(key, out var cached)) return cached;

            var baseStyle = bold
                ? (mini ? EditorStyles.miniBoldLabel : EditorStyles.boldLabel)
                : (mini ? EditorStyles.miniLabel : EditorStyles.label);

            // 换 GUIStyle 的实例, 否则会把 EditorStyles 里那份共享样式改掉
            var style = new GUIStyle(baseStyle)
            {
                alignment = ToAnchor(align),
                wordWrap  = wrap || baseStyle.wordWrap,
                richText  = rich || baseStyle.richText,
            };

            s_TextStyles[key] = style;
            return style;
        }

        private static TextAnchor ToAnchor(PanelAlign align)
        {
            switch (align)
            {
                case PanelAlign.Center: return TextAnchor.MiddleCenter;
                case PanelAlign.Right:  return TextAnchor.MiddleRight;
                default:                return TextAnchor.MiddleLeft;
            }
        }

        //-------------------------------------- 控件 --------------------------------------

        public bool Button(string text, in PanelButtonSpec spec)
        {
            // 按钮里的 emoji 转成图标: 保留图案, 不把豆腐块留在文字里
            var buttonText = text ?? "";
            var buttonIcon = spec.Icon;
            if (buttonIcon == null)
            {
                var emoji = PanelEmoji.First(buttonText);
                if (emoji != null)
                {
                    buttonIcon = emoji;
                    buttonText = PanelEmoji.Strip(buttonText);
                }
            }

            var content = m_ReusableContent;
            content.text    = buttonText;
            content.image   = buttonIcon;
            content.tooltip = string.IsNullOrEmpty(spec.Tooltip) ? null : spec.Tooltip;

            // 只覆盖本次按钮的背景色并在结束后还原: 直接写回 Color.white 会把外层作用域设的颜色一起吃掉,
            // 而按钮回调里若抛异常, 没有 finally 就会把错误颜色留给整帧后续的绘制。
            var previousBackground = GUI.backgroundColor;
            if (spec.HasBackground) GUI.backgroundColor = spec.Background;
            try
            {
                // Unity ImagePosition 没有 ImageRight, 图标统一居左
                return GUILayout.Button(content, GUI.skin.button, ButtonOptions(spec));
            }
            finally
            {
                GUI.backgroundColor = previousBackground;
            }
        }

        public bool Foldout(bool expanded, string label)
        {
            return GUILayout.Toggle(expanded, label, EditorStyles.foldout);
        }

        public string TextField(string label, string value, float width = 0f)
        {
            return EditorGUILayout.TextField(label, value ?? "", WidthOnly(width));
        }

        public long IntegerField(string label, long value, float width = 0f)
        {
            return EditorGUILayout.LongField(label, value, WidthOnly(width));
        }

        public double FloatField(string label, double value, float width = 0f)
        {
            return EditorGUILayout.FloatField(label, (float)value, WidthOnly(width));
        }

        public float Slider(string label, float value, float min, float max, float width = 0f)
        {
            return EditorGUILayout.Slider(label, value, min, max, WidthOnly(width));
        }

        public bool ToggleField(string label, bool value, float width = 0f)
        {
            return EditorGUILayout.Toggle(label, value, WidthOnly(width));
        }

        public int Popup(string label, int selected, string[] options, float width = 0f)
        {
            if (options == null || options.Length == 0) return selected;

            GUIContent content;
            if (string.IsNullOrEmpty(label))
            {
                content = GUIContent.none;
            }
            else
            {
                content = m_ReusableContent;
                content.text    = label;
                content.image   = null;
                content.tooltip = null;
            }

            return EditorGUILayout.Popup(content, selected, options, WidthOnly(width));
        }

        public int Toolbar(int selected, string[] options, float width = 0f)
        {
            if (options == null || options.Length == 0) return selected;
            return GUILayout.Toolbar(selected, ToolbarContents(options));
        }

        /// <summary>枚举按钮组的 GUIContent[] 缓存: 同一份 options 会被每帧反复用, 不该每帧重建。</summary>
        private static GUIContent[] ToolbarContents(string[] options)
        {
            if (SameText(s_LastToolbarTexts, options)) return s_LastToolbarContents;

            var contents = new GUIContent[options.Length];
            for (var i = 0; i < options.Length; i++)
            {
                contents[i] = new GUIContent(options[i]);
            }

            s_LastToolbarTexts    = options;
            s_LastToolbarContents = contents;
            return contents;
        }

        private static bool SameText(string[] a, string[] b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null || a.Length != b.Length) return false;

            for (var i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }

            return true;
        }

        public UnityEngine.Object ObjectField(string label, UnityEngine.Object value, System.Type type, float width = 0f)
        {
            return EditorGUILayout.ObjectField(label, value, type, true, WidthOnly(width));
        }

        public object StructField(string label, object value, System.Type type, float width = 0f)
        {
            var options = WidthOnly(width);

            if (type == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)(value ?? Vector2.zero), options);
            }

            if (type == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)(value ?? Vector3.zero), options);
            }

            if (type == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(label, (Vector4)(value ?? Vector4.zero), options);
            }

            if (type == typeof(Vector2Int))
            {
                return EditorGUILayout.Vector2IntField(label, (Vector2Int)(value ?? Vector2Int.zero), options);
            }

            if (type == typeof(Vector3Int))
            {
                return EditorGUILayout.Vector3IntField(label, (Vector3Int)(value ?? Vector3Int.zero), options);
            }

            // Quaternion 没有专用控件: 按欧拉角编辑, 与 Unity 默认检视器的表现一致
            if (type == typeof(Quaternion))
            {
                var rotation = value is Quaternion quaternion ? quaternion : Quaternion.identity;
                return Quaternion.Euler(EditorGUILayout.Vector3Field(label, rotation.eulerAngles, options));
            }

            if (type == typeof(Rect))
            {
                return EditorGUILayout.RectField(label, value is Rect rect ? rect : default(Rect), options);
            }

            if (type == typeof(RectInt))
            {
                return EditorGUILayout.RectIntField(label, value is RectInt r ? r : default(RectInt), options);
            }

            if (type == typeof(Bounds))
            {
                return EditorGUILayout.BoundsField(label, value is Bounds b ? b : default(Bounds), options);
            }

            if (type == typeof(BoundsInt))
            {
                return EditorGUILayout.BoundsIntField(label, value is BoundsInt b ? b : default(BoundsInt), options);
            }

            if (type == typeof(LayerMask))
            {
                var mask = value is LayerMask layerMask ? layerMask.value : 0;
                return (LayerMask)EditorGUILayout.LayerField(label, mask, options);
            }

            if (type == typeof(AnimationCurve))
            {
                return EditorGUILayout.CurveField(label, value as AnimationCurve, options);
            }

            if (type == typeof(Gradient))
            {
                return EditorGUILayout.GradientField(label, value as Gradient, options);
            }

            // 不支持的类型原样返回: 交给上层走文本框兜底
            return value;
        }
    }
}
