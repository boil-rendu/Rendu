#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Panel.Editor
{
    /// <summary>
    /// UI Toolkit 后端: 把 <see cref="IPanelDriver"/> 的调用序列落到一棵 VisualElement 树上。
    ///
    /// 与 IMGUI 侧的关键差别 —— 立即模式的调用序列要落进保留模式的树, 因此这里是**按位置复用**:
    /// 每次 Take 一个元素时, 先看当前容器在游标处的子元素是不是想要的类型, 是就复用(只更新内容与状态),
    /// 不是才换掉; 容器结束时把游标之后多余的子元素删掉。好处是折叠/条件显隐变化时只动受影响的那一段,
    /// 正在输入的输入框不会因为整树重建而丢焦点。
    ///
    /// 这也意味着**宿主必须每次面板数据变化后重新调用一次 DrawObject**(与 IMGUI 的每帧 OnGUI 同理):
    /// 按钮点击最后由引擎换面板状态时也会走 <see cref="RequestRepaint"/> → 宿主的自绘入口。
    /// </summary>
    public sealed class UitkDriver : IPanelDriver
    {
        private readonly VisualElement m_Root;

        private sealed class Scope
        {
            public VisualElement Container;
            public int           Cursor;
        }

        private readonly List<Scope> m_Scopes = new();
        private readonly StringBuilder m_KeyBuilder = new();

        /// <summary>本次 "绘制" 里被点中的按钮的 key(路径); 取到一次即消费掉 —— 对齐 IMGUI 的"返回 true 一次"。</summary>
        private string m_ClickedKey;

        /// <summary>
        /// 自上一次绘制以来被用户改动过的控件(记元素本身)。
        /// 保留模式的关键差别: IMGUI 的改动发生在 OnGUI 执行期间, BeginChangeCheck/EndChangeCheck 能直接
        /// 比出差值; 这里改动发生在**两次绘制之间**, 只记次数的话下一次 BeginCheck 时计数已经是新值,
        /// 永远比不出变化(值就写不进去)。所以记下被改的元素, EndCheck 按"是否属于这一段"来消费。
        /// </summary>
        private readonly HashSet<VisualElement> m_ChangedElements = new();

        private float? m_LabelWidth;

        private readonly Stack<Color>  m_Colors = new();
        private readonly Stack<bool>   m_Enabled = new();
        private readonly Stack<float?> m_Labels = new();

        /// <summary>BeginCheck 压入的是"这一段"所在的容器(不是计数)。</summary>
        private readonly Stack<VisualElement> m_Checks = new();

        /// <param name="root">挂树的位置, 一般是 <c>EditorWindow.rootVisualElement</c>; 不允许为空。</param>
        /// <param name="requestRedraw">
        /// 重新绘制整个面板的动作(一般是"再调一次 DrawObject"那个闭包)。IMGUI 靠每帧 OnGUI 天然重绘,
        /// UI Toolkit 是保留模式, 没有它就做不到"折叠之后子树消失 / 增删元素后重排"; 不传时退化成
        /// "只在宿主自己重绘时才更新"。
        /// </param>
        public UitkDriver(VisualElement root, Action requestRedraw = null)
        {
            m_Root = root ?? throw new ArgumentNullException(nameof(root),
                "UI Toolkit 后端必须挂在一棵真实的 VisualElement 树上; 没有宿主窗口的场合请继续用 ImGuiDriver。");
            m_RedrawAction = requestRedraw;
        }

        /// <summary>重绘动作。</summary>
        private readonly Action m_RedrawAction;

        /// <summary>是否已经在这一轮排过重绘: 一帧里的多次请求合并成一次。</summary>
        private bool m_RedrawScheduled;

        //-------------------------------------- 度量 --------------------------------------

        // UIElements 有自己的布局规则, 这三个值只用于表格高度公式与占位: 取 Unity 单行控件高度。
        public float LineHeight      => 18f;
        public float ScrollbarHeight => 15f;

        /// <summary>最近一次解析到的有效宽度: 布局完成前读不到宽度, 用它兜住那几帧。</summary>
        private float m_LastKnownWidth;

        /// <summary>这一轮绘制里是否读到过能用的宽度(没有就说明布局还没完成, 见 <see cref="EndPanel"/>)。</summary>
        private bool m_WidthWasUsable;

        /// <summary>连续多少轮没读到宽度 —— 只补排一次重绘, 免得一直量不到尺寸的根元素把重绘排成死循环。</summary>
        private int m_WidthRetry;

        /// <summary>布局尚未完成时宽度会读到 0/NaN; 大于它就说明这层已经量过尺寸了。
        /// 注意别在这里卡"够不够宽"——窗口本来就窄时会被一直判成没布局, 反而每轮都去补重绘。</summary>
        private const float MinUsableWidth = 1f;

        /// <summary>
        /// 当前视图宽度。容器自己可能还没走完布局(读到 0), 这时**不能**顺手退回定值:
        /// 同一份面板会变成"首帧用定值、之后某次重绘(比如点了一次页签)又用比例值",
        /// 上方那一整段(标签列宽 / 控件起点 / 行距观感)就会整体挪一次。
        /// 因此往上找已经布局过的祖先, 让首帧就拿到和后续一致的值; 取**最上层**那一个而不是最近的,
        /// 来源统一 —— 面板自己布局完之后宽度会与祖先略有出入, 换来换去同样算不出同一个列宽。
        /// </summary>
        public float ViewWidth
        {
            get
            {
                var width = ResolveWidth();
                if (width > 0f)
                {
                    m_LastKnownWidth = width;
                    m_WidthWasUsable = true;
                    return width;
                }

                return m_LastKnownWidth;   // 整条链都没布局(第一个绘制调用): 沿用上次的值, 没有则 0
            }
        }

        /// <summary>沿父链找最上层的有效宽度; 都没布局过时返回 0。</summary>
        private float ResolveWidth()
        {
            var width = 0f;
            for (var element = m_Root; element != null; element = element.parent)
            {
                var candidate = element.resolvedStyle.width;
                if (float.IsNaN(candidate) == false && candidate >= MinUsableWidth) width = candidate;
            }

            return width;
        }

        //-------------------------------------- 生命周期 --------------------------------------

        public void BeginPanel()
        {
            // 注意: 这里不能清 m_ClickedKey。IMGUI 的点击与消费在同一个 OnGUI 帧内, 每帧清一次是对的;
            // 保留模式下点击发生在两次绘制之间, 在绘制开头清就会把点击抹掉, Button() 永远读不到。
            // 消费时机改到 EndPanel(这一轮没被消费 = 对应按钮这次没画到, 丢弃)
            m_Scopes.Clear();
            PushAsScope(m_Root);

            // 上一轮若被异常打断(引擎只在最外层 catch), 这些栈会留着半截, 之后整块面板都会用错色/错宽,
            // 而且再也回不来。每轮开头归位: 最坏只影响这一轮的余下部分, 不会污染下一轮。
            // m_ChangedElements 不能在这里清 —— 它记的是"两轮之间"用户改过的控件, 清了就丢改动。
            m_Colors.Clear();
            m_Enabled.Clear();
            m_Labels.Clear();
            m_Checks.Clear();
            m_LabelWidth = null;
            m_WidthWasUsable = false;
        }

        public void EndPanel()
        {
            Pop();
            m_Scopes.Clear();

            m_ClickedKey = null;   // 这一轮没被 Button()/Toolbar() 取走: 按钮已经不在树上, 丢掉避免误触发

            // 没被任何 EndCheck 消费的改动说明对应控件这次没画到(折叠/隐藏/已移除): 丢掉,
            // 否则下次会被误判成"被改动"而写值
            m_ChangedElements.Clear();

            // 这一轮全程没读到宽度 = 调用时布局还没完成(见 ViewWidth): 补排一次重绘, 让面板在用户动手
            // 之前就用真实宽度重算一遍。每段"没宽度"只补一次 —— 根元素量不到尺寸(面板被隐藏之类)时
            // 一直自排队会把重绘排成死循环; 拿到宽度后计数归零。
            if (m_WidthWasUsable)
            {
                m_WidthRetry = 0;
            }
            else if (m_WidthRetry++ == 0)
            {
                RequestRepaint();
            }
        }

        /// <summary>
        /// 请求重绘: 排到下一次事件循环里再跑重载动作。
        /// 必须延后 —— 这个请求多半是在控件回调里发出的, 当场重绘等于在事件处理中重入布局与销毁自身。
        /// </summary>
        public void RequestRepaint()
        {
            if (m_RedrawAction == null || m_RedrawScheduled) return;

            m_RedrawScheduled = true;
            m_Root.schedule.Execute(() =>
            {
                m_RedrawScheduled = false;
                m_RedrawAction();
            });
        }

        //-------------------------------------- 容器 --------------------------------------

        public void BeginVertical(bool boxed, float width = 0f)
        {
            // Box 与纯 VisualElement 分别取: 混在一个 Func<VisualElement> 里就无法按精确类型复用
            var element = boxed ? Take(() => new Box(), null) : Take(() => new VisualElement(), null);
            element.style.flexDirection = FlexDirection.Column;
            if (boxed)
            {
                // 对齐 IMGUI box 的呼吸感: 分组之间留空隙, 框内留内边距(默认是零, 分组全贴在一起)
                element.style.marginTop    = BoxOuterSpacing;
                element.style.marginBottom = BoxOuterSpacing;
                element.style.paddingTop    = BoxInnerSpacing;
                element.style.paddingBottom = BoxInnerSpacing;
                element.style.paddingLeft   = BoxInnerSpacing * 1.5f;
                element.style.paddingRight  = BoxInnerSpacing * 1.5f;
            }

            if (width > 0f) element.style.width = width;
            PushAsScope(element);
        }

        public void EndVertical() => Pop();

        public void BeginHorizontal(bool boxed = false)
        {
            var element = Take(() => new VisualElement(), null);
            element.style.flexDirection = FlexDirection.Row;
            element.style.flexGrow      = 1f;
            PushAsScope(element);
        }

        public void EndHorizontal() => Pop();

        public void BeginIndent()
        {
            var element = Take(() => new VisualElement(), null);
            element.style.flexDirection = FlexDirection.Column;
            element.style.paddingLeft   = IndentWidth;
            PushAsScope(element);
        }

        public void EndIndent() => Pop();

        public void BeginScroll(ref Vector2 scroll, float maxHeight)
        {
            var view = Take(() => new ScrollView(ScrollViewMode.Vertical), null);
            view.style.maxHeight = maxHeight;

            // UIElements 自己维护滚动位置: 首帧用会话里的值播种, 之后以控件为准(不再来回搬)
            if (scroll != view.scrollOffset) view.scrollOffset = scroll;
            scroll = view.scrollOffset;

            PushAsScope(view.contentContainer);
        }

        public void EndScroll() => Pop();

        public void Space(float pixels)
        {
            Take(() => new VisualElement(), null).style.height = pixels;
        }

        public void FlexibleSpace()
        {
            var spacer = Take(() => new VisualElement(), null);
            spacer.style.flexGrow = 1f;
        }

        public void Separator(float thickness, Color color)
        {
            var line = Take(() => new VisualElement(), null);
            line.style.height          = thickness;
            line.style.backgroundColor = color;
            line.style.flexShrink      = 0f;
        }

        private const float IndentWidth = 15f;

        //-------------------------------------- 状态栈 --------------------------------------

        public void PushColor(Color color)
        {
            m_Colors.Push(color);
        }

        public void PopColor()
        {
            if (m_Colors.Count > 0) m_Colors.Pop();
        }

        public void PushEnabled(bool enabled)
        {
            m_Enabled.Push(enabled);
        }

        public void PopEnabled()
        {
            if (m_Enabled.Count > 0) m_Enabled.Pop();
        }

        public void PushLabelWidth(float? width)
        {
            m_Labels.Push(m_LabelWidth);
            if (width.HasValue) m_LabelWidth = width.Value;
        }

        public void PopLabelWidth()
        {
            m_LabelWidth = m_Labels.Count > 0 ? m_Labels.Pop() : null;
        }

        public void BeginCheck()
        {
            // 这一段所在的容器: DrawValueMember 的结构是 BeginHorizontal → BeginCheck → 控件,
            // 所以控件及其内部子控件(Toolbar 页签等)都在它下面
            m_Checks.Push(m_Scopes.Count > 0 ? m_Scopes[m_Scopes.Count - 1].Container : m_Root);
        }

        public bool EndCheck()
        {
            if (m_Checks.Count == 0) return false;

            // 只消费"属于这一段"的改动: 改动元素必须落在本次区间那个容器里。
            // 不能用 key 字符串前缀判定 —— KeyOf 的中间层用的是已经推进过的 cursor(Toolbar 里 bar 占了
            // 第 0 位, 页签 toggle 的路径就成了 "…/1/0", 与 BeginCheck 时的 "…/0" 对不上), 一嵌套就失配
            var container = m_Checks.Pop();
            var changed   = false;
            var hit       = new List<VisualElement>();

            foreach (var element in m_ChangedElements)
            {
                if (IsUnder(element, container)) hit.Add(element);
            }

            foreach (var element in hit)
            {
                m_ChangedElements.Remove(element);
                changed = true;
            }

            return changed;
        }

        /// <summary>元素是否是 container 本身或其后代(含被从树上摘下来的: 只比引用链)。</summary>
        private static bool IsUnder(VisualElement element, VisualElement container)
        {
            for (var e = element; e != null; e = e.hierarchy.parent)
            {
                if (ReferenceEquals(e, container)) return true;
            }

            return false;
        }

        /// <summary>控件被用户改动: 记下这个控件并排一次绘制(否则引擎永远看不到这次改动)。</summary>
        private void NoteChanged(VisualElement control)
        {
            m_ChangedElements.Add(control);
            RequestRepaint();
        }

        //-------------------------------------- 文本 --------------------------------------

        public void Label(string text, in PanelTextSpec spec)
        {
            var label = Take(() => new Label(), null);
            label.text  = text ?? "";
            ApplyTextSpec(label, spec);
        }

        public void KeyValueLabel(string label, string value)
        {
            var row = Take(() => new VisualElement(), null);
            row.style.flexDirection = FlexDirection.Row;
            PushAsScope(row);

            var key = Take(() => new Label(), null);
            key.text     = label ?? "";
            key.style.minWidth   = m_LabelWidth ?? AutoLabelWidth;  // 列宽下限, 与控件行的标签列同宽
            key.style.width      = StyleKeyword.Null;               // 长文字按内容撑开, 不做截断
            key.style.whiteSpace = WhiteSpace.NoWrap;
            key.style.overflow   = Overflow.Visible;
            key.style.unityFontStyleAndWeight = FontStyle.Normal;

            var val = Take(() => new Label(), null);
            val.text  = value ?? "";
            val.style.flexGrow = 1f;

            row.style.marginBottom = RowSpacing;
            Pop();
        }

        public void HelpBox(string message, PanelMessageKind kind)
        {
            var box = Take(() => new UnityEngine.UIElements.HelpBox("", ToHelpBoxType(kind)), null);
            box.text = message ?? "";
            box.style.marginBottom = RowSpacing * 2f;
        }

        private static HelpBoxMessageType ToHelpBoxType(PanelMessageKind kind)
        {
            switch (kind)
            {
                case PanelMessageKind.Info:    return HelpBoxMessageType.Info;
                case PanelMessageKind.Warning: return HelpBoxMessageType.Warning;
                case PanelMessageKind.Error:   return HelpBoxMessageType.Error;
                default:                       return HelpBoxMessageType.None;
            }
        }

        /// <summary>
        /// 按钮图标: 各版本的 Button 不一定有 iconImage, 这里统一塞一个 Image 子元素并按需增删
        /// (USS 里它排在文字左侧, 与 IMGUI 的 ImageLeft 观感一致)。非 Texture2D 的资源这里显示不出来,
        /// 这类图标只在 IMGUI 后端有效。
        /// </summary>
        private static void SetIcon(Button button, Texture icon)
        {
            var image = button.Q<Image>();
            if (icon == null)
            {
                image?.RemoveFromHierarchy();
                return;
            }

            if (image == null)
            {
                image = new Image();
                button.Add(image);
            }

            if (icon is Texture2D texture) image.image = texture;
        }

        /// <summary>标签列的默认宽度(没有 [LabelWidth] 时的下限)。
        /// 必须统一锁宽: 各控件 USS 自带的 label min-width 互不相同(Popup 120 / LongField 更宽 / Toggle 按内容),
        /// 不锁的话控件起点参差不齐, 对不齐 IMGUI 的控件列。</summary>
        private const float DefaultLabelWidth = 150f;

        /// <summary>IMGUI 的默认标签宽是按检视器宽度算的比例(实测约 0.35), 不是定值 ——
        /// 定值在宽窄窗口下都对不上 IMGUI 的控件列位置。</summary>
        private const float LabelWidthFraction = 0.35f;

        /// <summary>比例标签宽的上限(再宽的面板标签列也不至于拉得太开)。</summary>
        private const float MaxLabelWidth = 300f;

        /// <summary>输入区统一的文字左留白(Popup/TextField/Toggle 各自 USS 的 padding 不一致, 统一掉)。</summary>
        private const float InputPaddingLeft = 4f;

        /// <summary>当前标签列宽: [LabelWidth] 优先, 否则按面板宽度取比例; 首帧宽度未知时退回定值。</summary>
        private float AutoLabelWidth
        {
            get
            {
                var width = ViewWidth;
                if (float.IsNaN(width) || width < 200f) return DefaultLabelWidth;
                return Mathf.Clamp(width * LabelWidthFraction, DefaultLabelWidth, MaxLabelWidth);
            }
        }

        /// <summary>mini 文本的字号(对应 IMGUI 的 miniLabel)。</summary>
        private const float MiniFontSize = 11f;

        /// <summary>行与行之间的空隙(对齐 IMGUI 的默认 spacing 观感)。</summary>
        private const float RowSpacing = 2f;

        /// <summary>box 分组与外界的空隙。</summary>
        private const float BoxOuterSpacing = 5f;

        /// <summary>box 分组框内的留白。</summary>
        private const float BoxInnerSpacing = 5f;

        private void ApplyTextSpec(Label label, in PanelTextSpec spec)
        {
            label.style.unityTextAlign = spec.Align == PanelAlign.Center
                ? TextAnchor.MiddleCenter
                : spec.Align == PanelAlign.Right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

            label.style.unityFontStyleAndWeight = spec.Bold ? FontStyle.Bold : FontStyle.Normal;
            label.style.whiteSpace              = spec.Wrap ? WhiteSpace.Normal : WhiteSpace.NoWrap;
            if (spec.Width > 0f) label.style.width = spec.Width;
            if (spec.Height > 0f) label.style.height = spec.Height;
            label.style.marginBottom = RowSpacing;

            // 居中/右对齐的标题要跨整行才有"居中"可言: IMGUI 的 GUILayout.Label 占满可用宽,
            // 而这里 Label 宽度默认是内容宽, 不撑开的话 MiddleCenter 只是内容内居中(观感偏左)
            if (spec.Align != PanelAlign.Left && spec.Width <= 0f) label.style.flexGrow = 1f;
            else label.style.flexGrow = 0f;

            // mini(表格头/集合计数等次要文字)用更小的字号; UIElements 没有 EditorStyles.miniLabel 的等价物,
            // 但 richText 没有对应实现 —— IMGUI 那边支持的富文本在这里按纯文本显示。
            if (spec.Mini) label.style.fontSize = MiniFontSize;
            else label.style.fontSize = StyleKeyword.Null;
        }

        //-------------------------------------- 控件 --------------------------------------

        public bool Button(string text, in PanelButtonSpec spec)
        {
            var button = Take(() =>
            {
                var made = new Button();
                made.clicked += () =>
                {
                    m_ClickedKey = made.userData as string;
                    // IMGUI 是立即模式, 点击当帧 OnGUI 重跑就能消费; 保留模式必须主动排一次绘制,
                    // 否则点击永远传不到引擎
                    RequestRepaint();
                };
                return made;
            }, null);

            button.text = text ?? "";
            if (string.IsNullOrEmpty(spec.Tooltip) == false) button.tooltip = spec.Tooltip;
            SetIcon(button, spec.Icon);
            if (spec.Height > 0f) button.style.height = spec.Height;
            if (spec.Width > 0f) button.style.width = spec.Width;

            // 对齐 IMGUI: ExpandWidth(默认 true) → 占满可用宽(横排时平分), 否则缩回内容宽
            if (spec.ExpandWidth && spec.Width <= 0f)
            {
                button.style.alignSelf = Align.Stretch;
                button.style.flexGrow  = 1f;
            }
            else
            {
                button.style.alignSelf = Align.FlexStart;
                button.style.flexGrow  = 0f;
            }

            button.style.marginBottom = RowSpacing;

            // IMGUI: 背景 = 控件纹理 × GUI.color(暗一档), 字 = contentColor × GUI.color(本色, 亮)。
            // 显式背景(spec.HasBackground)与色栈都要过 TextureTinted 压暗, 直接铺原色会亮得刺眼
            if (spec.HasBackground) button.style.backgroundColor = TextureTinted(spec.Background);
            else if (m_Colors.Count > 0) button.style.backgroundColor = TextureTinted(m_Colors.Peek());

            if (m_Colors.Count > 0) button.style.color = m_Colors.Peek();

            if (m_ClickedKey != null && m_ClickedKey == button.userData as string)
            {
                m_ClickedKey = null;
                // 按钮的效果(换面板/改数据/增删元素)要再画一次才显示得出来: 引擎的 Mutate 只通知宿主
                // Undo/写脏, 不排重绘 —— IMGUI 由下一帧 OnGUI 兜住, 保留模式这里必须主动排
                RequestRepaint();
                return true;
            }

            return false;
        }

        public bool Foldout(bool expanded, string label)
        {
            var foldout = Take(() =>
            {
                var made = new Foldout();
                made.RegisterValueChangedCallback(_ => RequestRepaint());   // 折叠变化要重绘才被引擎看到
                return made;
            }, null);

            foldout.text = label ?? "";
            foldout.SetValueWithoutNotify(expanded);
            foldout.style.marginBottom = RowSpacing;
            return foldout.value;
        }

        public string TextField(string label, string value, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new UnityEngine.UIElements.TextField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.SetValueWithoutNotify(value ?? "");
            return field.value;
        }

        public long IntegerField(string label, long value, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new LongField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.SetValueWithoutNotify(value);
            return field.value;
        }

        public double FloatField(string label, double value, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new DoubleField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.SetValueWithoutNotify(value);
            return field.value;
        }

        public float Slider(string label, float value, float min, float max, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new Slider();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.lowValue  = min;
            field.highValue = max;
            field.SetValueWithoutNotify(value);
            return field.value;
        }

        public bool ToggleField(string label, bool value, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new UnityEngine.UIElements.Toggle();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.SetValueWithoutNotify(value);
            return field.value;
        }

        public int Popup(string label, int selected, string[] options, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new PopupField<string>();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.choices = new List<string>(options ?? System.Array.Empty<string>());
            var index = System.Math.Clamp(selected, 0, System.Math.Max(0, field.choices.Count - 1));
            field.SetValueWithoutNotify(index >= 0 && index < field.choices.Count ? field.choices[index] : "");
            return field.index;
        }

        public int Toolbar(int selected, string[] options, float width = 0f)
        {
            var bar = Take(() => new UnityEditor.UIElements.Toolbar(), null);
            // Toolbar 本体也要占满可用宽: 它在横排里 flexGrow 默认 0 会缩成内容宽,
            // 内部 toggle 的 flexGrow=1 就只能平分那一小块
            bar.style.flexGrow     = 1f;
            bar.style.marginBottom = RowSpacing;
            // 分段式页签: 容器不留内边距, 项与项才能紧贴成一条(IMGUI Toolbar 的样子)
            bar.style.paddingLeft  = 0f;
            bar.style.paddingRight = 0f;
            PushAsScope(bar);

            var result   = selected;
            // 点击消费后, 循环后面迭代里"当前选中项"的 value 不许把结果覆盖回旧值
            var consumed = false;
            for (var i = 0; i < (options?.Length ?? 0); i++)
            {
                var index = i;
                var toggle = Take(() =>
                {
                    var made = new UnityEditor.UIElements.ToolbarToggle();
                    made.RegisterValueChangedCallback(evt =>
                    {
                        // 点击交给 Toolbar() 消费(返回新 index); 记进改动集合让 EndCheck 判定写值
                        if (evt.newValue) m_ClickedKey = made.userData as string;
                        NoteChanged(made);
                    });
                    return made;
                }, null);

                toggle.text = options[i];
                // IMGUI 的 Toolbar 项: 平分整行、紧贴无间隙、扁高、直角、文字居中;
                // UITK 的 ToolbarToggle 默认内容宽 + 自带圆角和项间距, 逐项归一对齐
                toggle.style.flexGrow       = 1f;
                toggle.style.marginLeft     = 0f;
                toggle.style.marginRight    = 0f;
                toggle.style.height         = 20f;
                toggle.style.borderBottomLeftRadius  = 0f;
                toggle.style.borderBottomRightRadius = 0f;
                toggle.style.borderTopLeftRadius     = 0f;
                toggle.style.borderTopRightRadius    = 0f;
                toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
                toggle.SetValueWithoutNotify(index == selected);

                // 消费点击(与 Button 同一语义: 取到一次即清掉; 点到当前项不算切换, 与 IMGUI 一致)。
                // consumed 守卫: 点击被消费后, 后面"当前选中项"的 else if 不能再把 result 改回去 ——
                // 否则点"选中项之前"的页签(选中第 1 项时点第 0 项)会在 i=1 被覆盖回旧值, 表现为切不动
                if (m_ClickedKey != null && m_ClickedKey == toggle.userData as string)
                {
                    m_ClickedKey = null;
                    result   = index;
                    consumed = true;
                }
                else if (!consumed && toggle.value)
                {
                    result = index;
                }
            }

            Pop();
            return result;
        }

        public UnityEngine.Object ObjectField(string label, UnityEngine.Object value, Type type, float width = 0f)
        {
            var field = Take(() =>
            {
                var made = new UnityEditor.UIElements.ObjectField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.objectType = type;
            field.SetValueWithoutNotify(value);
            return field.value;
        }

        public object StructField(string label, object value, Type type, float width = 0f)
        {
            if (type == typeof(Vector2))    return Struct<Vector2Field, Vector2>(label, value, width);
            if (type == typeof(Vector3))    return Struct<Vector3Field, Vector3>(label, value, width);
            if (type == typeof(Vector4))    return Struct<Vector4Field, Vector4>(label, value, width);
            if (type == typeof(Vector2Int)) return Struct<Vector2IntField, Vector2Int>(label, value, width);
            if (type == typeof(Vector3Int)) return Struct<Vector3IntField, Vector3Int>(label, value, width);

            // Quaternion 没有专用控件: 与 IMGUI 一致按欧拉角编辑
            if (type == typeof(Quaternion))
            {
                var field = Struct<Vector3Field, Vector3>(label, value is Quaternion q ? q.eulerAngles : Vector3.zero,
                    width);
                return Quaternion.Euler(field);
            }

            if (type == typeof(Rect))      return Struct<RectField, Rect>(label, value, width);
            if (type == typeof(RectInt))   return Struct<RectIntField, RectInt>(label, value, width);
            if (type == typeof(Bounds))    return Struct<BoundsField, Bounds>(label, value, width);
            if (type == typeof(BoundsInt)) return Struct<BoundsIntField, BoundsInt>(label, value, width);

            if (type == typeof(LayerMask))
            {
                var field = Take(() =>
                {
                    var made = new UnityEditor.UIElements.LayerMaskField();
                    made.RegisterValueChangedCallback(_ => NoteChanged(made));
                    return made;
                }, null);

                ApplyLabelAndWidth(field, label, width);
                var mask = value is LayerMask layerMask ? layerMask.value : 0;
                field.SetValueWithoutNotify(mask);
                return (LayerMask)field.value;
            }

            if (type == typeof(AnimationCurve))
            {
                var field = Take(() =>
                {
                    var made = new UnityEditor.UIElements.CurveField();
                    made.RegisterValueChangedCallback(_ => NoteChanged(made));
                    return made;
                }, null);

                ApplyLabelAndWidth(field, label, width);
                field.SetValueWithoutNotify(value as AnimationCurve);
                return field.value;
            }

            if (type == typeof(Gradient))
            {
                var field = Take(() =>
                {
                    var made = new UnityEditor.UIElements.GradientField();
                    made.RegisterValueChangedCallback(_ => NoteChanged(made));
                    return made;
                }, null);

                ApplyLabelAndWidth(field, label, width);
                field.SetValueWithoutNotify(value as Gradient);
                return field.value;
            }

            return value;
        }

        /// <summary>
        /// 复合值类型的控件。逐个走 <see cref="Take{T}"/>: 类型由泛型参数给定, 复用判定与别的控件一样
        /// (同一位置仍是同类控件就复用, 值显式刷进去)。
        /// </summary>
        private TValue Struct<TField, TValue>(string label, object value, float width)
            where TField : BaseField<TValue>, new()
        {
            var field = Take(() =>
            {
                var made = new TField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            var typed = value is TValue typedValue ? typedValue : default;
            field.SetValueWithoutNotify(typed);
            return field.value;
        }

        //-------------------------------------- 内部: 元素复用 --------------------------------------

        /// <summary>当前容器内取一个元素: 类型相符就复用, 否则替换。key 用它在树里的位置路径。</summary>
        private T Take<T>(Func<T> create, Action<T> apply) where T : VisualElement
        {
            var scope = m_Scopes[m_Scopes.Count - 1];
            var key   = KeyOf(scope);

            if (scope.Cursor < scope.Container.childCount)
            {
                // 复用判定必须精确类型, 不能用 is T: 行容器取的是 VisualElement, 而 Button / Box / Label
                // 统统都是 VisualElement。用 is 会把上一轮留在这一位置的按钮当成"行"复用 —— 后面画的内容
                // 全变成这个按钮的子元素, 视觉上就是叠在按钮上互相遮挡, 类型也全乱了
                if (scope.Container[scope.Cursor].GetType() == typeof(T))
                {
                    var hit = (T)scope.Container[scope.Cursor];
                    scope.Cursor++;
                    hit.userData = key;
                    ApplyState(hit);
                    apply?.Invoke(hit);
                    return hit;
                }

                // 同一个位置换了别的控件: 掉的不是后半棵树, 只有这一个
                scope.Container.RemoveAt(scope.Cursor);
            }

            var made = create();
            made.userData = key;
            // 插回原位而不是追加到末尾: 游标之后可能还有上一轮留下的元素(末尾由 Pop 裁),
            // 追加会让新元素排到它们后面, 顺序错位
            scope.Container.Insert(scope.Cursor, made);
            scope.Cursor++;
            ApplyState(made);
            apply?.Invoke(made);
            return made;
        }

        /// <summary>
        /// IMGUI 的 GUI.color 是乘在控件的中灰纹理上的, 出来的是"暗一档"的彩色;
        /// UIElements 这里铺纯色, 不压暗的话就是刺眼的亮青/亮绿。
        /// 且 IMGUI 不同控件的底纹深浅不同: 下拉框/开关亮一档, 文本/数字输入框更暗, 这里按类别给两档系数。
        /// </summary>
        private const float ControlTintBrightness = 0.5f;

        /// <summary>文本/数字输入框的压暗系数(IMGUI 的 textfield 纹理比 popup 深一截)。</summary>
        private const float TextTintBrightness = 0.32f;

        /// <summary>
        /// 文本/数字类字段的 USS 类。不能用输入区的 "unity-text-field__input" 判别 ——
        /// LongField/IntegerField 的输入区顶层不带它, 判别会落空导致全部走亮档。
        /// 这些类挂在字段元素自身上, 是确定的。
        /// </summary>
        private static readonly string[] TextFieldClasses =
            { "unity-text-field", "unity-long-field", "unity-integer-field", "unity-double-field" };

        /// <summary>按钮的底色压暗(按钮纹理亮度在两类控件之间)。</summary>
        private const float TintBrightness = 0.55f;

        /// <summary>把业务色压暗到 IMGUI 控件纹理的等效亮度(保持 alpha)。</summary>
        private static Color TextureTinted(Color c, float brightness = TintBrightness) =>
            new(c.r * brightness, c.g * brightness, c.b * brightness, c.a);

        /// <summary>输入区底色: 按控件类别选压暗档位, 复刻 IMGUI "下拉框亮、数字/文本输入框暗"的层次。</summary>
        private static Color InputTinted(VisualElement field, Color tint)
        {
            foreach (var cls in TextFieldClasses)
            {
                if (field.ClassListContains(cls))
                {
                    return TextureTinted(tint, TextTintBrightness);
                }
            }

            return TextureTinted(tint, ControlTintBrightness);
        }

        private void ApplyState(VisualElement element)
        {
            if (m_Colors.Count > 0) element.style.color = m_Colors.Peek();
            else element.style.color = StyleKeyword.Null;

            // IMGUI 的 GUI.color 会乘进控件纹理(下拉框/输入框的底色一起变色); UIElements 的 color
            // 只影响文字, 这里显式把 BaseField 的输入区底色也染上, 检视器里那排彩色下拉框才对得上。
            // 色栈弹空时要清回默认(StyleKeyword.Null 交还 USS), 否则复用的控件残留上一次的颜色
            if (element.ClassListContains("unity-base-field"))
            {
                var input = element.Q(null, BaseField<string>.inputUssClassName);
                if (input != null)
                {
                    // 底色按控件类别压暗到 IMGUI 纹理乘算的等效亮度; 色栈弹空时清回默认。
                    // 输入区文字也要显式染(USS 对它有显式字色, 继承不进来), 对齐 IMGUI 的亮色字
                    input.style.backgroundColor = m_Colors.Count > 0
                        ? InputTinted(element, m_Colors.Peek()) : StyleKeyword.Null;
                    input.style.color = m_Colors.Count > 0 ? m_Colors.Peek() : StyleKeyword.Null;

                    // 各控件 USS 的 input margin/padding 互不相同(Popup/TextField/Toggle 差几个像素),
                    // 只锁标签宽对不齐输入区起点: 这里归零 + 统一文字左留白, 把起点钉死
                    input.style.marginLeft  = 0f;
                    input.style.paddingLeft = InputPaddingLeft;
                }

                // 标签文字保持默认字色, 不跟着色栈走: 只有输入区染色的观感才和原来一致,
                // 整列字段名被染成彩色会变成另一副样子(色栈弹空后元素是按位置复用的, 所以显式写 Null 清回默认)
                var fieldLabel = element.Q<Label>(null, BaseField<string>.labelUssClassName);
                if (fieldLabel != null) fieldLabel.style.color = StyleKeyword.Null;
            }
            else if (m_LabelWidth.HasValue)
            {
                var label = element.Q<Label>();
                if (label != null) label.style.minWidth = m_LabelWidth.Value;
            }

            if (m_Enabled.Count > 0) element.SetEnabled(m_Enabled.Peek());
        }

        private void ApplyLabelAndWidth<T>(BaseField<T> field, string label, float width)
        {
            field.label = label ?? "";

            // 标签列走类型化的 labelElement 属性(必然生效), 不经 Q 按类名查找 —— 查不到就整列对不齐。
            // 这里只锁"下限": 短标签统一成列宽(各行控件列对齐), 长标签按内容把列撑开 —— 后面的控件框
            // 相应变短, 文字完整单行显示(与 IMGUI 一致: IMGUI 也是长标签占宽、控件变短, 不截断也不折行)
            var labelElement = field.labelElement;
            labelElement.style.minWidth    = m_LabelWidth ?? AutoLabelWidth;
            labelElement.style.width       = StyleKeyword.Null;   // 清掉可能残留的锁宽, 允许按内容增长
            labelElement.style.flexShrink  = 0f;
            labelElement.style.flexGrow    = 0f;
            labelElement.style.marginLeft  = 0f;
            labelElement.style.whiteSpace  = WhiteSpace.NoWrap;
            labelElement.style.overflow    = Overflow.Visible;

            // IMGUI 的控件默认占满标签右侧的可用宽(UIElements 的 BaseField 默认是内容宽,
            // 不撑开的话下拉框只剩文字那么大); 显式给了宽度才定宽
            if (width > 0f)
            {
                field.style.width    = width;
                field.style.flexGrow = 0f;
            }
            else
            {
                field.style.flexGrow = 1f;
            }

            field.style.marginBottom = RowSpacing;
        }

        private void PushAsScope(VisualElement container)
        {
            m_Scopes.Add(new Scope { Container = container, Cursor = 0 });
        }

        private void Pop()
        {
            if (m_Scopes.Count == 0) return;

            // 游标之后的子元素是上次留下来的: 这一次没画到, 说明它们对应的成员已经消失/被隐藏
            var scope = m_Scopes[m_Scopes.Count - 1];
            while (scope.Container.childCount > scope.Cursor)
            {
                scope.Container.RemoveAt(scope.Container.childCount - 1);
            }

            m_Scopes.RemoveAt(m_Scopes.Count - 1);
        }

        private string KeyOf(Scope next)
        {
            m_KeyBuilder.Length = 0;
            for (var i = 0; i < m_Scopes.Count; i++)
            {
                if (i > 0) m_KeyBuilder.Append('/');
                m_KeyBuilder.Append(ReferenceEquals(m_Scopes[i], next) ? next.Cursor : m_Scopes[i].Cursor);
            }

            return m_KeyBuilder.ToString();
        }
    }
}
#endif
