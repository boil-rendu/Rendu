#if UNITY_2021_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Draw.Editor
{
    /// <summary>
    /// UI Toolkit 后端: 把 <see cref="IDriver"/> 的调用序列落到一棵 VisualElement 树上。
    ///
    /// 与 IMGUI 侧的关键差别 —— 立即模式的调用序列要落进保留模式的树, 因此这里是**按位置复用**:
    /// 每次 Take 一个元素时, 先看当前容器在游标处的子元素是不是想要的类型, 是就复用(只更新内容与状态),
    /// 不是才换掉; 容器结束时把游标之后多余的子元素删掉。好处是折叠/条件显隐变化时只动受影响的那一段,
    /// 正在输入的输入框不会因为整树重建而丢焦点。
    ///
    /// 这也意味着**宿主必须每次面板数据变化后重新调用一次 DrawObject**(与 IMGUI 的每帧 OnGUI 同理):
    /// 按钮点击最后由引擎换面板状态时也会走 <see cref="RequestRepaint"/> → 宿主的自绘入口。
    /// </summary>
    public sealed class UitkDriver : IDriver
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

            // 首帧 m_Root 还没走完布局, resolvedStyle.width 读不到 (0);
            // 排一次"布局完成后"重绘, 首次画出来的就是 m_Root 自己的真实宽度, 而不是按祖先宽度猜。
            // 检视器宿主里 m_Root 就是面板根, 窗口宿主里 m_Root 是右半列(嵌在 ScrollView 里),
            // 靠祖先宽度猜在窗口里会拿到整窗宽度(包含左菜单区), 把右列的字段列宽算错。
            m_Root.RegisterCallback<GeometryChangedEvent>(OnFirstGeometry);
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

        /// <summary>这一轮绘制里是否读到过能用的宽度(没有就说明布局还没完成, 见 <see cref="EndDraw"/>)。</summary>
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
        /// 因此往上找已经布局过的祖先(见 <see cref="ResolveWidth"/>)。首帧 m_Root 还没布局时取
        /// 到的可能是祖先的宽度(窗口宿主里 rootVisualElement 是整窗宽), 这时 <see cref="OnFirstGeometry"/>
        /// 会在 m_Root 真正布局完后补一次重绘, 下次就走 m_Root 自己的宽度了。
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

                return m_LastKnownWidth;   // 整条链都没布局(例如面板被隐藏): 沿用上次的值, 没有则 0
            }
        }

        /// <summary>
        /// 沿父链取**最近的**有效宽度: m_Root 自己 > 父 > 爷..., 遇到第一个就停。
        ///
        /// 不能用"最上层"(取循环最后一个): 检视器宿主里 m_Root ≈ 面板根, 二者几乎一样, 取最上层
        /// 是为了让首帧和后续帧来源统一(面板与根的宽度差几像素会导致标签列挪一下); **窗口宿主里
        /// m_Root 是右半列, 嵌在 ScrollView 里, rootVisualElement 是整窗宽度(含左菜单)**, 取最上层
        /// 就会把右列字段宽度算成"0.35 × 整窗", 标签列吃掉大半, 弹窗/按钮被裁。
        ///
        /// 取最近的: 布局完成后 m_Root 自己的 resolvedStyle.width 就是它所在的那一列的真实宽度, 检视器
        /// 和窗口都对。首帧 m_Root 还没布局(width=0)就往上一层找一个落地了的祖先当兜底, 随后
        /// <see cref="OnFirstGeometry"/> 触发一次重绘, 下次就是 m_Root 自己的宽度。
        /// </summary>
        private float ResolveWidth()
        {
            for (var element = m_Root; element != null; element = element.parent)
            {
                var candidate = element.resolvedStyle.width;
                if (float.IsNaN(candidate) == false && candidate >= MinUsableWidth) return candidate;
            }

            return 0f;
        }

        /// <summary>首帧布局完成后触发一次重绘, 让 m_Root 自己的真实宽度生效。一次性, 触发完就摘。</summary>
        private void OnFirstGeometry(GeometryChangedEvent evt)
        {
            m_Root.UnregisterCallback<GeometryChangedEvent>(OnFirstGeometry);
            RequestRepaint();
        }

        //-------------------------------------- 生命周期 --------------------------------------

        public void BeginDraw()
        {
            // 注意: 这里不能清 m_ClickedKey。IMGUI 的点击与消费在同一个 OnGUI 帧内, 每帧清一次是对的;
            // 保留模式下点击发生在两次绘制之间, 在绘制开头清就会把点击抹掉, Button() 永远读不到。
            // 消费时机改到 EndDraw(这一轮没被消费 = 对应按钮这次没画到, 丢弃)
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

        public void EndDraw()
        {
            Pop();
            m_Scopes.Clear();

            m_ClickedKey = null;   // 这一轮没被 Button()/Toolbar() 取走: 按钮已经不在树上, 丢掉避免误触发

            // 没被任何 EndCheck 消费的改动说明对应控件这次没画到(折叠/隐藏/已移除): 丢掉,
            // 否则下次会被误判成"被改动"而写值
            m_ChangedElements.Clear();

            // 这一轮全程没读到宽度 = 调用时整个面板链都没布局(例如窗口被隐藏、根元素被折叠之类,
            // 不是常见的"首帧还没布局"那种 —— 那种走 <see cref="OnFirstGeometry"/>):
            // 补排一次重绘, 避免面板永远卡在拿不到宽度的状态。每段只补一次, 免得一直量不到尺寸的根
            // 元素把重绘排成死循环; 拿到宽度后计数归零。
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
            ResetContainerResidue(element);
            element.style.flexDirection = FlexDirection.Column;
            if (boxed)
            {
                // 四边一律取 IMGUI box 的 margin/padding: 主题自带的("unity-box" 的 USS)被显式值覆盖,
                // 不再出现 UITK 分组比 IMGUI 宽一圈/松一截的观感
                element.style.marginTop    = BoxOuterSpacing;
                element.style.marginBottom = BoxOuterSpacing;
                element.style.marginLeft   = BoxOuterSpacing;
                element.style.marginRight  = BoxOuterSpacing;
                element.style.paddingTop    = BoxInnerSpacing;
                element.style.paddingBottom = BoxInnerSpacing;
                element.style.paddingLeft   = BoxInnerSpacing;
                element.style.paddingRight  = BoxInnerSpacing;
            }

            if (width > 0f) element.style.width = width;
            else element.style.width = StyleKeyword.Null;   // 上一轮画过带锁宽的分组, 残留会把这组钉死在旧宽上
            PushAsScope(element);
        }

        public void EndVertical() => Pop();

        public void BeginHorizontal(bool boxed = false)
        {
            var element = Take(() => new VisualElement(), null);
            ResetContainerResidue(element);
            element.style.flexDirection = FlexDirection.Row;
            element.style.flexGrow      = 1f;
            PushAsScope(element);
        }

        public void EndHorizontal() => Pop();

        public void BeginIndent()
        {
            var element = Take(() => new VisualElement(), null);
            ResetContainerResidue(element);
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
            var spacer = Take(() => new VisualElement(), null);
            ResetContainerResidue(spacer);
            spacer.style.height = pixels;
        }

        public void FlexibleSpace()
        {
            var spacer = Take(() => new VisualElement(), null);
            ResetContainerResidue(spacer);
            spacer.style.flexGrow = 1f;
        }

        public void Separator(float thickness, Color color)
        {
            var line = Take(() => new VisualElement(), null);
            ResetContainerResidue(line);
            line.style.height          = thickness;
            line.style.backgroundColor = color;
            line.style.flexShrink      = 0f;
        }

        /// <summary>
        /// 容器/占位元素复用前清掉其他角色留下的残留样式。
        ///
        /// 元素按"位置 + 类型"复用, 同一个位置切页签(或切模块)前后可能是完全不同的东西:
        /// 上一轮的 Space(定高)/Separator(定高+底色)/FlexibleSpace(伸展)/Indent(左内边距)/
        /// 带锁宽的纵向分组, 这一轮作为行或分组复用, 这些值若不清掉, 行就被钉死在别人的尺寸上
        /// —— 定高的行装不下 20px 的字段, 内容溢出来叠到下一行, 切几次页签面板越来越烂。
        /// 各 Begin*/占位方法在调用它之后马上写入自己需要的值, 这里只负责清场。
        /// </summary>
        private static void ResetContainerResidue(VisualElement element)
        {
            element.style.width           = StyleKeyword.Null;
            element.style.height          = StyleKeyword.Null;
            element.style.maxHeight       = StyleKeyword.Null;
            element.style.flexGrow        = StyleKeyword.Null;
            element.style.flexShrink      = StyleKeyword.Null;
            element.style.alignSelf       = StyleKeyword.Null;
            element.style.marginLeft      = StyleKeyword.Null;
            element.style.marginRight     = StyleKeyword.Null;
            element.style.paddingLeft     = StyleKeyword.Null;
            element.style.paddingRight    = StyleKeyword.Null;
            element.style.paddingTop      = StyleKeyword.Null;
            element.style.paddingBottom   = StyleKeyword.Null;
            element.style.backgroundColor = StyleKeyword.Null;
        }

        private const float IndentWidth = DrawMetrics.IndentWidth;

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

        public void Label(string text, in DrawTextSpec spec)
        {
            // emoji 没有字形, 走内联贴图: 一行里文本段用 Label, emoji 段用 Image
            if (DrawEmoji.Contains(text))
            {
                LabelWithEmoji(text, spec);
                return;
            }

            var label = Take(() => new Label(), null);
            label.text  = text ?? "";
            ApplyTextSpec(label, spec);
        }

        /// <summary>把"文字 + emoji"切成一段段画在同一行里。</summary>
        private void LabelWithEmoji(string text, in DrawTextSpec spec)
        {
            var row = Take(() => new VisualElement(), null);
            ResetContainerResidue(row);
            row.style.flexDirection = FlexDirection.Row;
            // IMGUI 的横向 GUILayout 组里子元素一律顶对齐: "键:\n值"这类带换行的文本段把行撑高后,
            // 定高的 emoji 贴图与后面的文字都贴在行顶(错位只有字体盒自身那 1~2px)。
            // UITK 默认 alignItems=Stretch: 定高 Image 贴顶, 而被拉伸到整行高的文字 Label 按
            // MiddleLeft 垂直居中 —— "已停止"会明显掉到行中间, 与 IMGUI 对不上。这里显式顶对齐。
            // 每轮都写: ResetContainerResidue 只清 alignSelf, 复用来的元素可能带着别处的 alignItems。
            row.style.alignItems = Align.FlexStart;
            row.style.marginBottom  = RowSpacing;
            PushAsScope(row);

            var runs = DrawEmoji.Split(text);
            for (int i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                if (run.IsEmoji)
                {
                    var image = Take(() => new Image(), null);
                    image.image        = run.Texture;
                    image.style.width  = DrawEmoji.Size;
                    image.style.height = DrawEmoji.Size;
                    image.style.marginRight = 2f;
                }
                else
                {
                    if (string.IsNullOrEmpty(run.Text)) continue;

                    var part = Take(() => new Label(), null);
                    part.text = run.Text;
                    ApplyTextSpec(part, spec);
                    part.style.marginBottom = 0f;   // 行间距由外层 row 给
                }
            }

            Pop();
        }

        public void KeyValueLabel(string label, string value)
        {
            var row = Take(() => new VisualElement(), null);
            ResetContainerResidue(row);
            row.style.flexDirection = FlexDirection.Row;
            PushAsScope(row);

            var key = Take(() => new Label(), null);
            key.text     = label ?? "";
            key.style.minWidth   = m_LabelWidth ?? AutoLabelWidth;  // 列宽下限, 与控件行的标签列同宽
            key.style.width      = StyleKeyword.Null;               // 长文字按内容撑开, 不做截断
            key.style.whiteSpace = WhiteSpace.NoWrap;
            key.style.overflow   = Overflow.Visible;
            key.style.unityFontStyleAndWeight = FontStyle.Normal;

            if (DrawEmoji.Contains(value))
            {
                var runs = DrawEmoji.Split(value ?? "");
                for (int i = 0; i < runs.Count; i++)
                {
                    var run = runs[i];
                    if (run.IsEmoji)
                    {
                        var emoji = Take(() => new Image(), null);
                        emoji.image        = run.Texture;
                        emoji.style.width  = DrawEmoji.Size;
                        emoji.style.height = DrawEmoji.Size;
                        emoji.style.marginRight = 2f;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(run.Text)) continue;

                        var part = Take(() => new Label(), null);
                        part.text = run.Text;
                        part.style.flexGrow = 1f;
                    }
                }
            }
            else
            {
                var val = Take(() => new Label(), null);
                val.text  = value ?? "";
                val.style.flexGrow = 1f;
            }

            row.style.marginBottom = RowSpacing;
            Pop();
        }

        public void HelpBox(string message, DrawMessageKind kind)
        {
            var box = Take(() => new UnityEngine.UIElements.HelpBox("", ToHelpBoxType(kind)), null);
            box.text = message ?? "";
            box.style.marginBottom = RowSpacing * 2f;
        }

        public void ProgressBar(float value, string title, float height)
        {
            var bar = Take(() => new UnityEngine.UIElements.ProgressBar(), null);

            // 高/低值显式钉成 0~1: 各版本 ProgressBar 的默认值并不一致, 写死免得进度对不上
            bar.lowValue  = 0f;
            bar.highValue = 1f;
            bar.value     = Mathf.Clamp01(value);
            bar.title     = title ?? "";

            if (height > 0f) bar.style.height = height;
            bar.style.marginBottom = RowSpacing;
        }

        private static HelpBoxMessageType ToHelpBoxType(DrawMessageKind kind)
        {
            switch (kind)
            {
                case DrawMessageKind.Info:    return HelpBoxMessageType.Info;
                case DrawMessageKind.Warning: return HelpBoxMessageType.Warning;
                case DrawMessageKind.Error:   return HelpBoxMessageType.Error;
                default:                       return HelpBoxMessageType.None;
            }
        }

        /// <summary>
        /// 按钮图标: 各版本的 Button 不一定有 iconImage, 这里统一塞一个 Image 子元素并按需增删。
        /// 注意按钮自绘的文字不认识子元素 —— 布局/绘制都不会给它让位, 直接塞会叠在文字上(穿模);
        /// 所以图标钉死在左侧一小块留白里, 按钮的 paddingLeft 让出这块区域, 观感与 IMGUI 的 ImageLeft 一致。
        /// 非 Texture2D 的资源这里显示不出来, 这类图标只在 IMGUI 后端有效。
        /// </summary>
        private const float ButtonIconSize  = 16f;
        private const float ButtonIconLeft  = 6f;
        private const float ButtonIconPadLeft = ButtonIconLeft + ButtonIconSize + 4f;

        private static void SetIcon(Button button, Texture icon)
        {
            var image = button.Q<Image>();
            if (icon == null)
            {
                if (image != null)
                {
                    image.RemoveFromHierarchy();
                    button.style.paddingLeft = StyleKeyword.Null;   // 没图标了, 左留白交还主题
                }
                return;
            }

            if (image == null)
            {
                image = new Image
                {
                    style =
                    {
                        position  = Position.Absolute,      // 不参与布局: 文字不会因它重排
                        left      = ButtonIconLeft,
                        top       = Length.Percent(50f),    // 垂直居中: 钉在中线再上移半个自身
                        translate = new Translate(0f, Length.Percent(-50f)),
                        width     = ButtonIconSize,
                        height    = ButtonIconSize,
                    }
                };
                button.Add(image);
                button.style.paddingLeft = ButtonIconPadLeft;   // 文字区让开图标
            }

            if (icon is Texture2D texture) image.image = texture;
        }

        /// <summary>标签列宽的下限(EditorGUIUtility.labelWidth 取不到时用。</summary>
        private const float DefaultLabelWidth = DrawMetrics.LabelWidthDefault;

        /// <summary>标签列宽的上限(再宽的面板标签列也不至于拉得太开)。</summary>
        private const float MaxLabelWidth = DrawMetrics.LabelWidthMax;

        /// <summary>
        /// 输入区的文字左留白。按 IMGUI 统一即为 0: EditorGUILayout 的字段紧贴标签列右边缘,
        /// 加留白反而与 IMGUI 错开。各控件 USS 自带的 padding 不一致由 USS 自己处理。
        /// </summary>
        private const float InputPaddingLeft = DrawMetrics.InputPaddingLeft;

        /// <summary>
        /// 单行字段的最小高度。正常有标签的字段行高由标签行盒撑起; HideLabel 的字段标签没有内容,
        /// 这条主题下输入区自己不带固有高, 整行会塌成一条薄片(弹窗文字被裁半截)。
        /// 这里给所有字段一个与主题单行控件一致的硬下限兜底, 与旁边正常行同高。
        /// </summary>
        // 这几个是现场量 IMGUI 的运行时值: 用属性而非 static readonly, 免得在样式体系就绪前
        // 就被初始化成兜底值、之后再也不刷新
        private static float FieldMinHeight => DrawMetrics.FieldMinHeight;

        /// <summary>当前标签列宽: [LabelWidth] 优先, 否则按面板宽度取比例; 首帧宽度未知时退回定值。</summary>
        /// <summary>跟随 IMGUI 的标签列宽(EditorGUIUtility.labelWidth), 不自己算比例。</summary>
        private static float AutoLabelWidth => Mathf.Clamp(DrawMetrics.LabelWidth, DefaultLabelWidth, MaxLabelWidth);

        /// <summary>mini 文本的字号(对应 IMGUI 的 miniLabel)。</summary>
        private static float MiniFontSize => DrawMetrics.MiniFontSize;

        /// <summary>行与行之间的空隙(取 IMGUI 的 standardVerticalSpacing)。</summary>
        private static float RowSpacing => DrawMetrics.RowSpacing;

        /// <summary>box 分组与外界的空隙(取 IMGUI 的 box 内边距)。</summary>
        private static float BoxOuterSpacing => DrawMetrics.BoxSpacing;

        /// <summary>box 分组框内的留白(取 IMGUI 的 box 内边距)。</summary>
        private static float BoxInnerSpacing => DrawMetrics.BoxSpacing;

        private void ApplyTextSpec(Label label, in DrawTextSpec spec)
        {
            label.style.unityTextAlign = spec.Align == DrawAlign.Center
                ? TextAnchor.MiddleCenter
                : spec.Align == DrawAlign.Right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

            label.style.unityFontStyleAndWeight = spec.Bold ? FontStyle.Bold : FontStyle.Normal;
            label.style.whiteSpace              = spec.Wrap ? WhiteSpace.Normal : WhiteSpace.NoWrap;
            // 元素按位置复用: 同一位置上一轮可能画过带锁宽/锁高的别的文字(表格头/定高标题), 条件
            // 不成立时必须显式清回 Null 交还 USS, 否则残留会把这一轮的文字钉在别人的尺寸上
            label.style.width  = spec.Width  > 0f ? spec.Width  : StyleKeyword.Null;
            label.style.height = spec.Height > 0f ? spec.Height : StyleKeyword.Null;
            // 垂直间距唯一来源是 RowSpacing: 主题给 Label 自带的 margin 全部显式归零,
            // 否则 IMGUI(standardVerticalSpacing)与 UITK(主题 margin + RowSpacing)行距对不上
            label.style.marginTop    = 0f;
            label.style.marginBottom = RowSpacing;
            label.style.marginLeft   = 0f;
            label.style.marginRight  = 0f;

            // IMGUI 的 GUILayout.Label 样式 stretchWidth=true: 一律占满本行剩余宽(锁宽的除外) ——
            // 横排里会把后面的内容(别的文本段 / FlexibleSpace / 控件)推到右边。UIElements 的 Label
            // 默认是内容宽, 这里用 flexGrow=1 复刻同一行为; 只给 Center/Right 撑开的话,
            // "键:\n🔴 值"这类带 emoji 的分段文字会全部挤在左边, 与 IMGUI 对不上
            label.style.flexGrow = spec.Width > 0f ? 0f : 1f;

            // 开关富文本: 与 IMGUI 后端的 GUISettings.richText 对齐(富文本标记由引擎在 ToTextSpec 里声明)。
            // 不设的话 <color> / <b> 之类标记会被当成普通字符原样显示出来。
            label.enableRichText = spec.RichText;

            // mini(表格头/集合计数等次要文字)用更小的字号; UIElements 没有 EditorStyles.miniLabel 的等价物
            if (spec.Mini) label.style.fontSize = MiniFontSize;
            else label.style.fontSize = StyleKeyword.Null;
        }

        //-------------------------------------- 控件 --------------------------------------

        public bool Button(string text, in DrawButtonSpec spec)
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

            // 按钮里的 emoji 转成图标: 保留图案, 不把豆腐块留在文字里
            var buttonText = text ?? "";
            var buttonIcon = spec.Icon;
            if (buttonIcon == null)
            {
                var emoji = DrawEmoji.First(buttonText);
                if (emoji != null)
                {
                    buttonIcon = emoji;
                    buttonText = DrawEmoji.Strip(buttonText);
                }
            }

            button.text = buttonText;
            if (string.IsNullOrEmpty(spec.Tooltip) == false) button.tooltip = spec.Tooltip;
            SetIcon(button, buttonIcon);
            // 同 Label: 复用的按钮可能带着上一轮的锁宽/锁高(比如别处的定高小按钮), 条件不成立要清掉
            button.style.height = spec.Height > 0f ? spec.Height : StyleKeyword.Null;
            button.style.width  = spec.Width  > 0f ? spec.Width  : StyleKeyword.Null;

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

            button.style.marginTop    = 0f;   // 主题 margin 归零, 行距唯一来源是 RowSpacing
            button.style.marginBottom = RowSpacing;

            // IMGUI: 背景 = 控件纹理 × GUI.color(暗一档), 字 = contentColor × GUI.color(本色, 亮)。
            // 显式背景(spec.HasBackground)与色栈都要过 TextureTinted 压暗, 直接铺原色会亮得刺眼
            if (spec.HasBackground) button.style.backgroundColor = TextureTinted(spec.Background);
            else if (m_Colors.Count > 0) button.style.backgroundColor = TextureTinted(m_Colors.Peek());
            else button.style.backgroundColor = StyleKeyword.Null;   // 复用残留: 别处的绿/蓝按钮色要清掉

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
            foldout.style.marginTop    = 0f;   // 主题 margin 归零, 行距唯一来源是 RowSpacing
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

        public Enum EnumFlagsField(string label, Enum value, float width = 0f)
        {
            if (value == null) return null;

            // 多选枚举用 UnityEditor.UIElements 的 EnumFlagsField: 它就是 Unity 自己的"位组合多选"控件,
            // 与 IMGUI 的 EditorGUILayout.EnumFlagsField 是同一套语义
            // 全限定: EnumFlagsField 在 UnityEditor.UIElements 里(本文件只 using 了 UnityEngine.UIElements)
            // 必须把枚举类型传给构造: 无参构造的字段不知道自己是哪个枚举, SetValueWithoutNotify 绑不上,
            // 会一直显示默认的 "Nothing"(值 0)。IMGUI 那版能从 value 推断类型, UITK 不行。
            var field = Take(() =>
            {
                var made = new UnityEditor.UIElements.EnumFlagsField(value);
                // 保留模式: 用户改勾选必须靠这个回调进 m_ChangedElements, 引擎才会把新值写回模型;
                // 少了它, 下一帧 SetValueWithoutNotify 用旧模型值一覆盖, 多选就丢了(与 Toggle/Popup 同一套路)
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            ApplyLabelAndWidth(field, label, width);
            field.SetValueWithoutNotify(value);
            return field.value;
        }

        public int Toolbar(int selected, string[] options, float width = 0f)
        {
            var bar = Take(() => new UnityEditor.UIElements.Toolbar(), null);
            // Toolbar 本体也要占满可用宽: 它在横排里 flexGrow 默认 0 会缩成内容宽,
            // 内部 toggle 的 flexGrow=1 就只能平分那一小块
            bar.style.flexGrow     = 1f;
            bar.style.marginTop    = 0f;   // 主题 margin 归零, 行距唯一来源是 RowSpacing
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

                    // 塌行保护: 输入区高度给下限(见 FieldMinHeight), HideLabel 字段不再塌成薄片
                    input.style.minHeight = FieldMinHeight;
                }

                // 标签文字保持默认字色, 不跟着色栈走: 只有输入区染色的观感才和原来一致,
                // 整列字段名被染成彩色会变成另一副样子(色栈弹空后元素是按位置复用的, 所以显式写 Null 清回默认)
                var fieldLabel = element.Q<Label>(null, BaseField<string>.labelUssClassName);
                if (fieldLabel != null) fieldLabel.style.color = StyleKeyword.Null;
            }
            else
            {
                // 普通元素里的第一个 Label(标题/表格头/HelpBox 文字等): 有压栈列宽就对齐它,
                // 没有则清掉 —— 同一位置上一轮可能画过带列宽的别的文字, 残留会把标题钉歪
                var label = element.Q<Label>();
                if (label != null)
                {
                    label.style.minWidth = m_LabelWidth.HasValue ? m_LabelWidth.Value : StyleKeyword.Null;
                }
            }

            if (m_Enabled.Count > 0) element.SetEnabled(m_Enabled.Peek());
        }

        private void ApplyLabelAndWidth<T>(BaseField<T> field, string label, float width)
        {
            // 空串触发 Unity 的 no-label 变体(标签元素从布局里拿掉, 控件占满整行)—— 与 IMGUI 的
            // GUIContent.none 同义; 直接把 label 透传过去, 不要手动 display/塞空格干扰它。
            field.label = label ?? "";

            // 标签列走类型化的 labelElement 属性(必然生效), 不经 Q 按类名查找 —— 查不到就整列对不齐。
            // 这里只锁"下限": 短标签统一成列宽(各行控件列对齐), 长标签按内容把列撑开 —— 后面的控件框
            // 相应变短, 文字完整单行显示(与 IMGUI 一致: IMGUI 也是长标签占宽、控件变短, 不截断也不折行)
            // HideLabel 走的是 no-label 变体, labelElement 已经被拿掉, 下面的样式写到不存在的元素上没影响。
            var labelElement = field.labelElement;
            labelElement.style.minWidth    = m_LabelWidth ?? AutoLabelWidth;
            labelElement.style.width       = StyleKeyword.Null;   // 清掉可能残留的锁宽
            labelElement.style.flexShrink  = 0f;
            labelElement.style.flexGrow    = 0f;
            labelElement.style.marginLeft  = 0f;
            labelElement.style.marginTop   = 0f;   // 行高由控件行自己定, 标签元素不带额外 margin
            labelElement.style.marginBottom = 0f;
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
                // 复用的字段可能带着上一轮别的控件留下的锁宽: 不清掉就被钉死在旧宽上, 弹窗文字被裁
                field.style.width    = StyleKeyword.Null;
                field.style.flexGrow = 1f;
            }

            // 同 Label: 主题给 BaseField 自带的 margin 归零, 行距唯一来源是 RowSpacing
            field.style.marginTop    = 0f;
            field.style.marginBottom = RowSpacing;
            field.style.marginLeft   = 0f;
            field.style.marginRight  = 0f;

            // 塌行保护: HideLabel 时 Unity 把标签元素从布局里拿掉(no-label 变体), 字段行高就只剩
            // 输入区自己撑 —— 某些主题下输入区不带固有高, 整行塌成薄片、弹窗文字被裁。这里给字段
            // 本身一个与单行控件一致的硬下限, 无论塌陷在哪一层都能兜住
            field.style.minHeight = FieldMinHeight;
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
