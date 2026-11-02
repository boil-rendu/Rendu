using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 后端: 把驱动原语的调用序列落到一棵 VisualElement 树上。
    ///
    /// 与 的关键差别 —— 立即模式的调用序列要落进保留模式的树, 因此这里是**按位置复用**:
    /// 每次取一个元素时, 先看当前容器在游标处的子元素是不是想要的类型, 是就复用(只更新内容与状态),
    /// 不是才换掉; 容器结束时把游标之后多余的子元素删掉。好处是折叠/条件显隐变化时只动受影响的那一段,
    /// 正在输入的输入框不会因为整树重建而丢焦点。
    ///
    /// 这也意味着**宿主必须每次面板数据变化后重新调用一次 DrawObject**(与 每帧 OnGUI 同理):
    /// 按钮点击最后由引擎换面板状态时也会走 <see cref="RequestRepaint"/> → 宿主的自绘入口。
    ///
    /// ── 本类只负责"控件词汇表 + 生命周期" ─────────────────────────────────
    /// 保留模式那套机制与"画什么控件"无关, 却横切在每个绘制方法里, 因此拆成三个协作者:
    ///   - <see cref="Tree"/>    元素调和: 作用域栈 + 按位置复用(本后端的根本机制);
    ///   - <see cref="Styler"/>  样式施加: 复用元素时重刷颜色/禁用态/标签列宽, 不刷就会残留上一轮的值;
    ///   - <see cref="StateStack"/> 外观/交互状态栈, 并对外暴露栈顶给上面两者读取。
    /// 三者之间的依赖是单向的(Tree → Styler → StateStack), 本类只做装配。
    /// ───────────────────────────────────────────────────────────────────
    /// </summary>
    public class Driver
    {
        private readonly VisualElement m_Root;

        /// <summary>本次 "绘制" 里被点中的按钮的 key(路径); 取到一次即消费掉 —— 对齐 "返回 true 一次"。</summary>
        private string m_ClickedKey;

        /// <summary>
        /// 自上一次绘制以来被用户改动过的控件(记元素本身)。
        /// 保留模式的关键差别: 改动发生在 OnGUI 执行期间, BeginChangeCheck/EndChangeCheck 能直接
        /// 比出差值; 这里改动发生在**两次绘制之间**, 只记次数的话下一次 BeginCheck 时计数已经是新值,
        /// 永远比不出变化(值就写不进去)。所以记下被改的元素, EndCheck 按"是否属于这一段"来消费。
        /// </summary>
        private readonly HashSet<VisualElement> m_ChangedElements = new();

        /// <summary>外观/交互状态栈(颜色/启用/标签列宽), 独立成类以便复用元素时重刷与读取顶值。</summary>
        private readonly StateStack m_StateStack = new();

        /// <summary>样式施加: 复用每个元素时按状态栈顶重刷颜色/禁用态/标签列宽。</summary>
        private readonly Styler m_Styler;

        /// <summary>元素调和: 作用域栈 + 按位置复用(保留模式与 根本差别都在这里)。</summary>
        private readonly Tree m_Tree;

        /// <summary>BeginCheck 压入的是"这一段"所在的容器(不是计数)。</summary>
        private readonly Stack<VisualElement> m_Checks = new();

        /// <param name="root">挂树的位置, 一般是 <c>EditorWindow.rootVisualElement</c>; 不允许为空。</param>
        /// <param name="requestRedraw">
        /// 重新绘制整个面板的动作(一般是"再调一次 DrawObject"那个闭包)。 靠每帧 OnGUI 天然重绘,
        ///  是保留模式, 没有它就做不到"折叠之后子树消失 / 增删元素后重排"; 不传时退化成
        /// "只在宿主自己重绘时才更新"。
        /// </param>
        public Driver(VisualElement root, Action requestRedraw = null)
        {
            m_Root      = root ?? throw new ArgumentNullException(nameof(root),
                "后端必须挂在一棵真实的 VisualElement 树上; 没有宿主窗口的场合请走 CreateInspectorGUI 自己提供 Driver。");
            m_RedrawAction = requestRedraw;
            m_Styler       = new Styler(m_StateStack);
            m_Tree         = new Tree(m_Styler);

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

        /// <summary>是否正处在一次绘制过程内(BeginDraw..EndDraw) —— 临时诊断用, 用来抓"绘制中又请求重绘"的环。</summary>
        private bool m_InDraw;

        /// <summary>绘制中请求重绘(探测喂自己)的连续预算: 超过上限就停止喂食, 避免不确定时机的无限红绘环。</summary>
        private const int MaxProbeRepaints = 30;

        /// <summary>当前剩余预算; 外部触发(交互/缩放/选中/首帧)会重置回 <see cref="MaxProbeRepaints"/>。</summary>
        private int m_ProbeRepaintBudget = MaxProbeRepaints;

        //-------------------------------------- 度量 --------------------------------------

        // 布局度量一律以保留模式控件规格为基准(见 RDTheme): 这里不留任何写死的像素值,
        // 换皮肤/换 DPI 时跟着一起变。
        public virtual float LineHeight      => RDTheme.FieldMinHeight;
        public virtual float ScrollbarHeight => RDTheme.ScrollbarHeight;

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
        public virtual float ViewWidth
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

        /// <summary>ViewWidth 沿父链解析的本来就是当前容器的真实宽度, 直接复用。</summary>
        public virtual float ContentWidth => ViewWidth;

        /// <summary>滚动区横向 inset: 与 ScrollView 同一套(滚动区左右 padding + 竖向滚动条宽)。</summary>
        public virtual float ScrollViewInsets => RDTheme.ScrollViewInsets;

        /// <summary>分组盒横向 inset: 与 box 样式同一套(左右 padding 之和)。</summary>
        public virtual float BoxInsets => RDTheme.BoxInsets;

        /// <summary>保留模式: 宽度沿父链解析, 没有"加探测控件"的副作用, 直接返回当前可用宽。</summary>
        public virtual float MeasureContentWidth() => ContentWidth;

        /// <summary>保留模式: 不用加探针控件, 直接返回当前容器的可用宽。</summary>
        public virtual float ProbeWidth() => ContentWidth;

        /// <summary>
        /// 当前写指针的纵坐标(面板坐标): 表格用它和视口下沿相减, 算数据区还能占多高。
        /// 保留模式没有 Layout/Repaint 之分, 读到的是上一次布局结算的位置 ——
        /// 与 "量到 → 缓存 → 重绘"殊途同归, 排版用的都是已结算的值。
        /// </summary>
        /// <summary>最近一次 <see cref="BeginScroll"/> 创建的滚动区 VE: 表格用它到布局完成后挂几何回调填高(避免整树重建抖动)。</summary>
        public VisualElement ActiveScroll { get; private set; }

        public virtual float MeasureContentTop() => m_Tree.CurrentCursorBottom();

        /// <summary>
        /// 当前视口可用下沿: 沿父链找最近的 ScrollView, 取它的底边并折算滚动位置 ——
        /// 与 <see cref="MeasureContentTop"/> 同一坐标系, 两者相减与滚动无关。
        /// 没有滚动区(独立检视器那种面板)时返回"量不到", 表格退回按行数估高度。
        /// </summary>
        public virtual float AvailableBottom
        {
            get
            {
                // 落在 ScrollView 里的面板才有"剩下的高度"可言: 窗口宿主的内容区就是滚动区视口
                for (var element = m_Root; element != null; element = element.parent)
                {
                    if (!(element is ScrollView scroll)) continue;

                    var rect = scroll.worldBound;
                    if (rect.height <= 1f) return 0f;

                    return rect.yMax - scroll.scrollOffset.y;
                }

                return 0f;
            }
        }

        /// <summary>
        /// 滚动区视口宽: 只认**当前容器(contentContainer)自己**已结算的宽, 不往祖先退。
        ///
        /// 不能用 <see cref="ContentWidth"/>: 它会沿父链退到"第一个布局过的祖先"当兜底。
        /// 表格重建后内容组还没布局时, 退到的是表格盒子宽(没扣 padding / 竖向滚动条),
        /// 比真实视口宽出一截; 下一帧内容组布局好又是真实视口宽 —— 两个值每帧交替,
        /// <c>TableRenderer.MeasureTableRowWidth</c> 就每帧都当成"新的视口宽"再 RequestRepaint,
        /// 形成重绘死环(实测 ~30 帧才被探测预算截停)。
        ///
        /// 未布局时返回 0: 调用方据此跳过本次测量, 沿用上一帧缓存, 表格退回"按盒子层数估内缩"
        /// 的老路 —— 稳定不抖, 只是右侧可能差几像素, 远好过整树反复重建。
        /// </summary>
        public virtual float MeasureScrollViewportWidth()
        {
            var container = m_Tree.CurrentContainer;
            if (container == null) return 0f;

            var width = container.resolvedStyle.width;
            if (float.IsNaN(width) || width < MinUsableWidth) return 0f;

            var padding = container.resolvedStyle.paddingLeft + container.resolvedStyle.paddingRight;
            return Mathf.Max(width - padding, 0f);
        }

        /// <summary>
        /// 沿父链取**最近的**有效宽度: 当前最内层容器 > 父 > 爷..., 遇到第一个就停。
        ///
        /// 起点必须是"当前所在的那一层"(见 <see cref="Tree.CurrentContainer"/>), 不能图省事从 m_Root 开始:
        /// 嵌套盒(表格盒/分组盒)有自己的 padding, 从 m_Root 一路量上来拿到的是外层容器的宽,
        /// 当前这层的可用宽被算宽 —— 表格均分列宽据此反算就虚高, 数据行比滚动区宽, 撑出横向滚动条
        /// (实际踩过这个坑: 一旦算宽, 左右会出现两条水平滚动条)。
        /// 首帧当前容器还没布局(width=0)时才往祖先退, 找到第一个落地了的尺寸当兜底。
        /// 取到的还要扣掉该容器自身的左右 padding —— 调用方(列宽/分隔线)要的是"内容宽"。
        /// </summary>
        private float ResolveWidth()
        {
            // 从当前最内层容器量起(不是 m_Root): 嵌套盒有自己的 padding, 从外层量会把当前这层
            // 的可用宽算宽 —— 表格据此反算列宽就虚高, 撑出横向滚动条。首帧没布局才往祖先退
            var start = m_Tree.CurrentContainer ?? m_Root;
            for (var element = start; element != null; element = element.parent)
            {
                var candidate = element.resolvedStyle.width;
                if (float.IsNaN(candidate) == false && candidate >= MinUsableWidth)
                {
                    // 容器自身的左右 padding 扣掉: 调用方(列宽/分隔线)要的是"内容宽"
                    var padding = element.resolvedStyle.paddingLeft + element.resolvedStyle.paddingRight;
                    return Mathf.Max(candidate - padding, 0f);
                }
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

        public virtual void BeginDraw()
        {
            m_InDraw = true;

            // 注意: 这里不能清 m_ClickedKey。点击与消费在同一个 OnGUI 帧内, 每帧清一次是对的;
            // 保留模式下点击发生在两次绘制之间, 在绘制开头清就会把点击抹掉, Button() 永远读不到。
            // 消费时机改到 EndDraw(这一轮没被消费 = 对应按钮这次没画到, 丢弃)
            m_Tree.PushAsScope(m_Root);

            // 上一轮若被异常打断(引擎只在最外层 catch), 这些栈会留着半截, 之后整块面板都会用错色/错宽,
            // 而且再也回不来。每轮开头归位: 最坏只影响这一轮的余下部分, 不会污染下一轮。
            // m_ChangedElements 不能在这里清 —— 它记的是"两轮之间"用户改过的控件, 清了就丢改动。
            m_StateStack.Reset();
            m_Checks.Clear();
            m_WidthWasUsable = false;

            // 条目区间与手柄登记只活一轮(BeginReorder/EndReorderItem 会重新登记)。
            // 不清的话换对象/换面板会按引用哈希攒下一堆已不在树上的元素。
            // 拖拽状态在元素自己身上, 与这两张表无关, 所以这里清表不影响"拖到一半重绘"。
            m_ReorderItems.Clear();
            m_ReorderHandles.Clear();
        }

        public virtual void EndDraw()
        {
            m_Tree.Pop();
            m_Tree.Reset();

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
                UnityEngine.Debug.Log("[ContentJitter] EndDraw width retry");
                RequestRepaint();
            }

            m_InDraw = false;
        }

        /// <summary>
        /// 请求重绘: 排到下一次事件循环里再跑重载动作。
        /// 必须延后 —— 这个请求多半是在控件回调里发出的, 当场重绘等于在事件处理中重入布局与销毁自身。
        /// </summary>
        public virtual void RequestRepaint()
        {
            if (m_RedrawAction == null || m_RedrawScheduled) return;

            if (m_InDraw)
            {
                // 绘制中请求重绘 = 探测喂自己(测量→缓存→重绘)。正常收敛几帧就停; 若卡在来回摆
                // (表格按"可用下沿 − 当前位置"填高时尤其容易踩), 到预算上限就停, 避免不确定时机的
                // 无限红绘环。预算用尽后这一轮不再喂食, 直到有外部触发重置。
                if (m_ProbeRepaintBudget-- <= 0) return;
                var fr = new StackTrace().GetFrame(1);
                UnityEngine.Debug.Log($"[ContentJitter-Draw] RequestRepaint during draw from {fr?.GetMethod()?.DeclaringType?.Name}.{fr?.GetMethod()?.Name} (budget {m_ProbeRepaintBudget})");
            }
            else
            {
                // 外部触发(交互/缩放/选中/首帧自愈): 重置预算, 允许新一轮收敛。
                m_ProbeRepaintBudget = MaxProbeRepaints;
            }

            m_RedrawScheduled = true;

            m_Root.schedule.Execute(() =>
            {
                m_RedrawScheduled = false;
                m_RedrawAction();
            });
        }

        //-------------------------------------- 容器 --------------------------------------

        public virtual void BeginVertical(bool boxed, float width = 0f, bool expandHeight = false, bool expandWidth = false)
        {
            // Box 与纯 VisualElement 分别取: 混在一个 Func<VisualElement> 里就无法按精确类型复用
            var element = boxed ? m_Tree.Take(() => new Box(), null) : m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(element);
            element.style.flexDirection = FlexDirection.Column;
            var hint = LayoutPolicy.ForBox(boxed, width, expandHeight, expandWidth);
            // 横排里撑满行高: 默认 align-items 已是 stretch, 显式写死防父容器改了或缺省
            if (hint.FillHeight) element.style.alignSelf = Align.Stretch;
            // 横排里吃满剩余宽度: 成员块在横排里必须靠它才不会被压成 0 宽( ExpandWidth 对应)
            if (hint.FillWidth)
            {
                // 占满宽: 主轴/交叉轴的翻译交给 Layout, 竖排里不会被拉高
                element.style.flexShrink = 1f;
                Layout.FillWidth(element);
            }
            if (boxed)
            {
                // 与  BoxStyle() 同一套: 只保留纵向外边距, 横向那一份在面板边缘
                // 就是一条纯浪费的空白带(左边与菜单隔出十几像素, 右边白吃掉一列宽)
                element.style.marginTop    = hint.MarginTop;
                element.style.marginBottom = hint.MarginBottom;
                element.style.marginLeft   = 0f;
                element.style.marginRight  = 0f;
                element.style.paddingTop    = hint.PaddingTop;
                element.style.paddingBottom = hint.PaddingBottom;
                element.style.paddingLeft   = hint.PaddingLeft;
                element.style.paddingRight  = hint.PaddingRight;
            }

            // 定宽要与  Width + MaxWidth 双锁一致: 只写 width, 子元素的最小宽仍能把组撑大
            // (表格单元格就是这样顶宽整行、顶出横向滚动条的)
            if (hint.Width > 0f)
            {
                element.style.width    = hint.Width;
                element.style.maxWidth = hint.MaxWidth;
            }
            else
            {
                element.style.width    = StyleKeyword.Null;   // 上一轮画过带锁宽的分组, 残留会把这组钉死在旧宽上
                element.style.maxWidth = StyleKeyword.Null;
            }
            m_Tree.PushAsScope(element);
        }

        public virtual void EndVertical() => m_Tree.Pop();

        public virtual void BeginHorizontal(bool boxed = false)
        {
            // Box 与纯 VisualElement 分别取: 混在一个 Func 里就无法按精确类型复用(见 Tree.Take)
            var element = boxed ? m_Tree.Take(() => new Box(), null) : m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(element);
            element.style.flexDirection = FlexDirection.Row;
            // 行本身吃满可用宽: 横排父里是 flexGrow; 竖排父里 flexGrow 会把这行拉高
            Layout.FillWidth(element);
            if (boxed)
            {
                // 表头行: 与  HeaderRowStyle 同观感 —— 比表格盒亮一档的底色 +
                // 上下 1px 边线, 24px 高(20 文字 + 2 内边距 + 2 边线), 不是四边框的 box
                element.style.backgroundColor   = RDTheme.PanelHeaderBackground;
                element.style.borderTopWidth    = RDTheme.PanelHeaderBorderThickness;
                element.style.borderBottomWidth = RDTheme.PanelHeaderBorderThickness;
                element.style.borderTopColor    = RDTheme.PanelHeaderBorder;
                element.style.borderBottomColor = RDTheme.PanelHeaderBorder;
                element.style.borderLeftWidth   = 0f;
                element.style.borderRightWidth  = 0f;
                element.style.marginTop    = 0f;
                element.style.marginBottom = 0f;
                element.style.marginLeft   = 0f;
                element.style.marginRight  = 0f;
                element.style.paddingTop    = RDTheme.PanelHeaderPaddingVertical;
                element.style.paddingBottom = RDTheme.PanelHeaderPaddingVertical;
                // 左右不留 padding: 表头的列宽与数据行一致, 竖分隔线才能对齐
                element.style.paddingLeft   = 0f;
                element.style.paddingRight  = 0f;
            }

            m_Tree.PushAsScope(element);
        }

        public virtual void EndHorizontal() => m_Tree.Pop();

        public virtual void BeginIndent()
        {
            var element = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(element);
            element.style.flexDirection = FlexDirection.Column;
            element.style.paddingLeft   = RDTheme.IndentWidth;
            m_Tree.PushAsScope(element);
        }

        public virtual void EndIndent() => m_Tree.Pop();

        public virtual void BeginScroll(ref Vector2 scroll, float maxHeight)
        {
            var view = m_Tree.Take(() => new ScrollView(ScrollViewMode.Vertical), null);
            view.style.maxHeight = maxHeight;
            ActiveScroll = view;

            // 只有竖向滚动条: 横向不滚, 内容再宽也只在原处裁掉。
            // 不关的话底部会凭空多出一条横条, 两种后端一眼就不一样
            view.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            // 竖向滚动条必须 AlwaysVisible: 它的显隐会直接改变滚动区视口宽,
            // 而表格拿"视口宽"去均分列宽(见 TableRenderer.MeasureTableRowWidth)。
            // 显隐一跳, 视口宽就 751↔704 抖一下, 每帧都触发 RequestRepaint → 重绘反馈环。
            // 常驻(哪怕空表也留轨道)换视口宽恒定, 列宽一次收敛, 不再重绘。
            view.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;

            // UIElements 自己维护滚动位置: 首帧用会话里的值播种, 之后以控件为准(不再来回搬)
            if (scroll != view.scrollOffset) view.scrollOffset = scroll;
            scroll = view.scrollOffset;

            m_Tree.PushAsScope(view.contentContainer);
        }

        public virtual void EndScroll() => m_Tree.Pop();

        public virtual void Space(float pixels)
        {
            var spacer = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(spacer);
            // 不可压缩: flex 项默认会收缩, 空间紧时 Space 会被压成 0, 行距整片糊在一起
            spacer.style.height     = pixels;
            spacer.style.flexShrink = 0f;
        }

        public virtual void FlexibleSpace()
        {
            var spacer = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(spacer);
            spacer.style.flexGrow = 1f;
        }

        public virtual void Separator(float thickness, Color color)
        {
            var line = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(line);
            line.style.height          = thickness;
            line.style.backgroundColor = color;
            line.style.flexShrink      = 0f;
        }

        /// <summary>
        /// 分节表头: 一行有底色的横条。与 同高同色(见 <see cref="RDTheme"/>),
        /// 换后端不换分节比例; 上下 1px 边线把横条与相邻内容切干净。
        /// </summary>
        public virtual void PanelHeader(string text, DrawAlign align, bool bold)
        {
            var bar = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(bar);
            bar.style.flexDirection     = FlexDirection.Row;
            bar.style.alignItems        = Align.Center;
            bar.style.height            = RDTheme.PanelHeaderHeight;
            bar.style.flexShrink        = 0f;
            bar.style.backgroundColor   = RDTheme.PanelHeaderBackground;
            bar.style.borderTopWidth    = RDTheme.PanelHeaderBorderThickness;
            bar.style.borderBottomWidth = RDTheme.PanelHeaderBorderThickness;
            bar.style.borderTopColor    = RDTheme.PanelHeaderBorder;
            bar.style.borderBottomColor = RDTheme.PanelHeaderBorder;
            bar.style.marginTop         = 0f;
            bar.style.marginBottom      = RowSpacing;
            m_Tree.PushAsScope(bar);

            var label = m_Tree.Take(() => new Label(), null);
            label.text = text ?? "";
            ApplyTextSpec(label, DrawTextSpec.Of(align, bold));
            // 横条自身已给高度与上下留白: 文字行不再带行距, 否则 20px 的条装不下
            label.style.marginTop    = 0f;
            label.style.marginBottom = 0f;
            label.style.paddingLeft  = 6f;
            label.style.paddingRight = 6f;

            m_Tree.Pop();
        }

        /// <summary>
        /// 卡片: 比单元格亮一档的底色 + 一条下边线(与 同色同形, 见 <see cref="RDTheme"/>)。
        ///
        /// 左右不留线、顶部不叠框: 参考界面的条目是一条通栏横带, 条目之间只隔一道横线。
        /// </summary>
        public virtual void BeginCard()
        {
            var card = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(card);
            card.style.flexDirection     = FlexDirection.Column;
            // 卡片吃满宽: 竖排父里只有 alignSelf, 给 flexGrow 就会跟着单元格多余高度一起长高
            Layout.FillWidth(card);
            card.style.flexShrink        = 0f;
            card.style.backgroundColor   = RDTheme.CardBackground;
            card.style.borderBottomWidth = RDTheme.CardBorderThickness;
            card.style.borderBottomColor = RDTheme.CardBorder;
            card.style.paddingLeft       = RDTheme.CardPadding;
            card.style.paddingRight      = RDTheme.CardPadding;
            card.style.paddingTop        = RDTheme.CardPaddingTop;
            card.style.paddingBottom     = RDTheme.CardPaddingBottom;
            card.style.marginTop         = 0f;
            card.style.marginBottom      = 0f;
            m_Tree.PushAsScope(card);
        }

        public virtual void EndCard()
        {
            m_Tree.Pop();
        }

        /// <summary>
        /// 表格列之间的竖线: 定高时按 height 居中, 否则 alignSelf = Stretch 撑满行高。
        /// </summary>
        public virtual void ColumnSeparator(float height = 0f)
        {
            var line = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(line);
            line.style.width           = 1f;
            line.style.flexGrow        = 0f;
            line.style.flexShrink      = 0f;
            line.style.alignSelf       = height > 0f ? Align.Center : Align.Stretch;
            line.style.height          = height > 0f ? height : StyleKeyword.Null;
            line.style.marginTop       = 0f;
            line.style.marginBottom    = 0f;
            line.style.marginLeft      = 0f;
            line.style.marginRight     = 0f;
            line.style.backgroundColor = RDTheme.TableHeaderLineColor;
        }

        /// <summary>
        /// 表头上可拖拽的列边界: 元素宽 5px、左右各负 2px 外边距 —— 布局仍只占 1px(与
        /// <see cref="ColumnSeparator"/> 等宽, 列不会因此被撑宽), 抓取区却有 5px:
        /// 1px 的目标在  里几乎点不中。线本身是元素里居中的 1px 子元素,
        /// 所以抓取区透明、只有线有色。
        /// </summary>
        public virtual float ColumnResizer(float height)
        {
            var handle = m_Tree.Take(() => new ColumnResizeHandle(RequestRepaint), null);
            Styler.ResetContainerResidue(handle);
            handle.style.width           = RDTheme.ColumnResizeHitWidth;
            handle.style.marginLeft      = -(RDTheme.ColumnResizeHitWidth - 1f) * 0.5f;
            handle.style.marginRight     = -(RDTheme.ColumnResizeHitWidth - 1f) * 0.5f;
            handle.style.flexGrow        = 0f;
            handle.style.flexShrink      = 0f;
            handle.style.flexDirection   = FlexDirection.Row;
            handle.style.justifyContent  = Justify.Center;
            handle.style.alignSelf       = height > 0f ? Align.Center : Align.Stretch;
            handle.style.height          = height > 0f ? height : StyleKeyword.Null;
            handle.style.marginTop       = 0f;
            handle.style.marginBottom    = 0f;

            // 拖拽发生在两次绘制之间: 手柄把增量攒着, 由下一次绘制取走并作用到列宽上。
            // 攒着而不是"当场改": 列宽的上下限与可用宽只有渲染器知道。
            // 拿到增量后由渲染器请求重绘, 这里不再重复排一次。
            return handle.ConsumeDelta();
        }

        //-------------------------------------- 拖拽排序 --------------------------------------

        /// <summary>各列表本轮登记的条目元素(下标 → 元素): 落点按它们的 worldBound 算。</summary>
        private readonly Dictionary<int, List<(int index, VisualElement element)>> m_ReorderItems = new();

        /// <summary>各列表本轮的手柄元素: 哪个手柄被按着, 由手柄自己记着(见 ReorderHandleElement)。</summary>
        private readonly Dictionary<int, List<ReorderHandleElement>> m_ReorderHandles = new();

        /// <summary>落点线: 复用一个元素, 拖到哪条缝就挂到那一条上(绝对定位, 不参与布局)。</summary>
        private DropLine m_DropLine;

        /// <summary>源条目上的压暗遮罩: 与落点线一样是"临时子元素", 下一轮绘制自动被裁掉。</summary>
        private DimOverlay m_Dim;

        /// <summary>跟着指针走的浮层卡片(挂在最后一条上, 才盖得住其它条目)。</summary>
        private DragGhost m_Ghost;

        public virtual void BeginReorder(int listId)
        {
            if (m_ReorderItems.TryGetValue(listId, out var items) == false)
            {
                items = new List<(int, VisualElement)>();
                m_ReorderItems[listId] = items;
            }

            items.Clear();

            if (m_ReorderHandles.TryGetValue(listId, out var handles) == false)
            {
                handles = new List<ReorderHandleElement>();
                m_ReorderHandles[listId] = handles;
            }

            handles.Clear();
        }

        public virtual void ReorderHandle(int listId, int index, float width, string ghost)
        {
            // 手柄是**专属类型**: 元素按精确类型复用, 混进通用的 VisualElement 会被下一轮当成
            // 行容器借走, "12px 宽 + 撑满行高 + 指针回调"就残留到某个成员行上
            var handle = m_Tree.Take(() => new ReorderHandleElement(RequestRepaint), null);
            Styler.ResetContainerResidue(handle);
            handle.style.width           = width;
            handle.style.flexGrow        = 0f;
            handle.style.flexShrink      = 0f;
            handle.style.flexDirection   = FlexDirection.Column;
            handle.style.justifyContent  = Justify.Center;
            // 横排里撑满条目高: 按下它的任何一处都算"抓住这一条"(与  ExpandHeight 对应)
            handle.style.alignSelf       = Align.Stretch;
            handle.style.marginTop       = 0f;
            handle.style.marginBottom    = 0f;
            handle.style.marginLeft      = 0f;
            handle.style.marginRight     = 0f;

            ApplyTextSpec(handle.Glyph, DrawTextSpec.Of(DrawAlign.Center));
            handle.Bind(listId, index, ghost);

            if (m_ReorderHandles.TryGetValue(listId, out var handles)) handles.Add(handle);
        }

        public virtual void EndReorderItem(int listId, int index)
        {
            // 条目刚 EndVertical, 刚关掉的那个容器就是它(见 Tree.LastPopped)
            var element = m_Tree.LastPopped;
            if (element == null) return;
            if (m_ReorderItems.TryGetValue(listId, out var items) == false) return;

            items.Add((index, element));
        }

        public virtual ReorderDrag PollReorder(int listId)
        {
            if (m_ReorderHandles.TryGetValue(listId, out var handles) == false) return ReorderDrag.None;

            ReorderHandleElement source = null;
            for (var i = 0; i < handles.Count; i++)
            {
                if (handles[i].Dragging && handles[i].ListId == listId) source = handles[i];
            }

            if (source == null) return ReorderDrag.None;
            if (m_ReorderItems.TryGetValue(listId, out var items) == false || items.Count == 0)
            {
                return ReorderDrag.None;
            }

            var target = InsertionAt(items, source.PointerY);

            if (source.Released)
            {
                // 先清状态再交结果: 引擎这一帧就改数据, 下一次绘制不该再报同一次拖拽
                var dragged = source.Index;
                source.EndDrag();
                return new ReorderDrag(dragged, target, true);
            }

            DrawDropLine(items, target);
            DrawDragVisual(items, source);
            return new ReorderDrag(source.Index, target, false);
        }

        /// <summary>
        /// 拖动中的"拿起来"观感: 源条目盖一层半透明遮罩(它已经不在原位), 指针右下角浮一张小卡片。
        ///
        /// 遮罩而不是改 opacity: 浮层是条目的子元素, 改 opacity 会把浮层一起弄淡。
        /// 浮层挂到**最后一条**上: 兄弟节点按顺序绘制, 挂在自己身上会被后面的条目盖住。
        /// 两样都是"本轮临时塞进去的子元素", 下一轮条目调和时会被当多余子元素裁掉, 拖完自然消失。
        /// </summary>
        private void DrawDragVisual(List<(int index, VisualElement element)> items, ReorderHandleElement source)
        {
            var sourceItem = FindItem(items, source.Index);
            if (sourceItem == null) return;

            m_Dim ??= new DimOverlay();
            if (m_Dim.parent != sourceItem)
            {
                m_Dim.RemoveFromHierarchy();
                sourceItem.Add(m_Dim);
            }
            m_Dim.BringToFront();

            var host = items[items.Count - 1].element;

            m_Ghost ??= new DragGhost();
            if (m_Ghost.parent != host)
            {
                m_Ghost.RemoveFromHierarchy();
                host.Add(m_Ghost);
            }

            var origin = host.worldBound;

            m_Ghost.Text        = source.Ghost;
            m_Ghost.style.left  = source.PointerX - origin.x + RDTheme.DragGhostPointerOffsetX;
            m_Ghost.style.top   = source.PointerY - origin.y + RDTheme.DragGhostPointerOffsetY;
            m_Ghost.style.width = Mathf.Clamp(sourceItem.worldBound.width * RDTheme.DragGhostWidthRatio,
                                              RDTheme.DragGhostMinWidth, RDTheme.DragGhostMaxWidth);
            m_Ghost.BringToFront();
        }

        /// <summary>按下标找条目元素; 找不到返回 null。</summary>
        private static VisualElement FindItem(List<(int index, VisualElement element)> items, int index)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].index == index) return items[i].element;
            }

            return null;
        }

        /// <summary>
        /// 指针落在哪条缝里: 逐个条目比中线, 第一个"指针在中线之上"的条目前面就是落点;
        /// 全比完还在下面就是最后一条之后。区间由 <see cref="EndReorderItem"/> 按绘制顺序追加, 下标递增。
        /// </summary>
        private static int InsertionAt(List<(int index, VisualElement element)> items, float pointerY)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (pointerY < items[i].element.worldBound.center.y) return items[i].index;
            }

            return items[items.Count - 1].index + 1;
        }

        /// <summary>
        /// 落点线: 挂到落点那一条的上沿(落在末尾时是末尾那条的下沿), 2px 蓝线。
        ///
        /// 挂进条目元素而不是按父容器坐标算: 卡片有内边距、列表体还可能套在滚动区里,
        /// 手算的 top 会随宿主偏移; 挂进条目后 -1px 正好压在缝上。
        /// 条目调和时会把这条线当"多余子元素"裁掉, 拖拽结束后自然消失。
        /// </summary>
        private void DrawDropLine(List<(int index, VisualElement element)> items, int target)
        {
            VisualElement host;
            var atBottom = false;

            if (target <= items[0].index)
            {
                host = items[0].element;
            }
            else if (target > items[items.Count - 1].index)
            {
                host = items[items.Count - 1].element;
                atBottom = true;
            }
            else
            {
                host = items[items.Count - 1].element;
                for (var i = 0; i < items.Count; i++)
                {
                    if (items[i].index != target) continue;
                    host = items[i].element;
                    break;
                }
            }

            m_DropLine ??= new DropLine();
            if (m_DropLine.parent != host)
            {
                m_DropLine.RemoveFromHierarchy();
                host.Add(m_DropLine);
            }

            m_DropLine.style.top    = atBottom ? StyleKeyword.Auto : -1f;
            m_DropLine.style.bottom = atBottom ? -1f : StyleKeyword.Auto;
            m_DropLine.BringToFront();
        }

        /// <summary>
        /// 拖拽手柄: 十几像素宽的竖列 + 居中的 "≡", 自带拖拽状态。
        ///
        /// 状态挂在元素上而不是 Driver 字段里(与 <see cref="ColumnResizeHandle"/> 同一个理由):
        /// 元素按位置复用, 同一条手柄跨帧是同一个实例, 拖拽中途重绘也不会丢; 而拖的是哪张列表、
        /// 哪一条, 由每次绘制重新绑定。
        /// </summary>
        private sealed class ReorderHandleElement : VisualElement
        {
            private readonly Action m_OnMoved;

            /// <summary>本手柄画的 "≡": 外观由外层每次绘制按文本规格刷。</summary>
            public Label Glyph { get; }

            /// <summary>本帧绑定到的列表、条目下标与浮层摘要。</summary>
            private int    m_ListId;
            private int    m_Index;
            private string m_Ghost;

            /// <summary>正被按着拖。</summary>
            public bool Dragging { get; private set; }

            /// <summary>松手了但引擎还没取走(取走后由 <see cref="EndDrag"/> 清掉)。</summary>
            public bool Released { get; private set; }

            /// <summary>指针坐标(panel 坐标系), 与条目元素的 worldBound 同一坐标系。</summary>
            public float PointerY { get; private set; }
            public float PointerX { get; private set; }

            public int Index => m_Index;

            /// <summary>浮层上那句摘要(引擎随手柄一起给)。</summary>
            public string Ghost => m_Ghost;

            /// <summary>本帧绑定到的列表; 与 <see cref="PollReorder"/> 的 listId 不符就不算它的拖拽。</summary>
            public int ListId => m_ListId;

            public ReorderHandleElement(Action onMoved)
            {
                m_OnMoved = onMoved;

                Glyph = new Label("≡");
                Add(Glyph);

                RegisterCallback<PointerDownEvent>(OnPointerDown);
                RegisterCallback<PointerMoveEvent>(OnPointerMove);
                RegisterCallback<PointerUpEvent>(OnPointerUp);
                RegisterCallback<PointerCaptureOutEvent>(OnCaptureOut);
            }

            /// <summary>每次绘制重新绑定: 同一位置上一帧可能是另一张列表的第几条。</summary>
            public void Bind(int listId, int index, string ghost)
            {
                m_ListId = listId;
                m_Index  = index;
                m_Ghost  = ghost;
            }

            /// <summary>引擎取走松手后清状态, 否则一次松手会被反复报告, 数据挪好几下。</summary>
            public void EndDrag()
            {
                Dragging = false;
                Released = false;
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0) return;

                Dragging = true;
                Released = false;
                PointerX = evt.position.x;
                PointerY = evt.position.y;

                // 捕获指针: 拖出手柄列后仍能收到 Move/Up, 否则拖快了就"松手"
                this.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (Dragging == false) return;

                PointerX = evt.position.x;
                PointerY = evt.position.y;
                evt.StopPropagation();

                // 保留模式没有"每帧 OnGUI": 不排重绘, 落点线要等下一次别的原因的重绘才会动
                m_OnMoved?.Invoke();
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                if (Dragging == false) return;

                PointerX = evt.position.x;
                PointerY = evt.position.y;
                Released = true;
                if (this.HasPointerCapture(evt.pointerId)) this.ReleasePointer(evt.pointerId);
                m_OnMoved?.Invoke();
                evt.StopPropagation();
            }

            private void OnCaptureOut(PointerCaptureOutEvent evt)
            {
                // 捕获被系统收走(窗口失焦等)且没走 PointerUp: 这次拖拽作废, 别让手柄一直"按着"
                if (Released == false) Dragging = false;
            }
        }

        /// <summary>
        /// 落点线: 2px 蓝线, 绝对定位在落点那条的上沿/下沿, 不参与布局。
        ///
        /// 单独一个类型: 元素按精确类型复用, 混进通用的 VisualElement 会被下一轮当成行容器取走,
        /// "绝对定位 + 2px 高"就残留到某个成员行上(那一行会整个塌掉)。
        /// </summary>
        private sealed class DropLine : VisualElement
        {
            public DropLine()
            {
                pickingMode = PickingMode.Ignore;
                style.position        = Position.Absolute;
                style.left            = 0f;
                style.right           = 0f;
                style.height          = 2f;
                style.backgroundColor = RDTheme.DropIndicatorColor;
            }
        }

        /// <summary>
        /// 拖拽浮层: 卡片底色 + 1px 边框 + 一行摘要, 跟着指针走, 不参与布局。
        ///
        /// 单独一个类型(与 <see cref="DropLine"/> 同一个理由): 元素按精确类型复用,
        /// 混进通用的 VisualElement 会被下一轮拿去当行容器, 绝对定位与底色就残留到别的控件上。
        /// </summary>
        private sealed class DragGhost : VisualElement
        {
            private readonly Label m_Text;

            public string Text
            {
                get => m_Text.text;
                set => m_Text.text = value;
            }

            public DragGhost()
            {
                pickingMode = PickingMode.Ignore;

                style.position        = Position.Absolute;
                style.height          = RDTheme.GhostHeight;
                style.backgroundColor = RDTheme.CardBackground;
                style.borderTopWidth    = RDTheme.CardBorderThickness;
                style.borderBottomWidth = RDTheme.CardBorderThickness;
                style.borderLeftWidth   = RDTheme.CardBorderThickness;
                style.borderRightWidth  = RDTheme.CardBorderThickness;
                style.borderTopColor    = RDTheme.CardBorder;
                style.borderBottomColor = RDTheme.CardBorder;
                style.borderLeftColor   = RDTheme.CardBorder;
                style.borderRightColor  = RDTheme.CardBorder;
                style.borderTopLeftRadius     = RDTheme.GhostCornerRadius;
                style.borderTopRightRadius    = RDTheme.GhostCornerRadius;
                style.borderBottomLeftRadius  = RDTheme.GhostCornerRadius;
                style.borderBottomRightRadius = RDTheme.GhostCornerRadius;

                m_Text = new Label
                {
                    pickingMode = PickingMode.Ignore,
                };
                m_Text.style.unityTextAlign = TextAnchor.MiddleLeft;
                m_Text.style.overflow       = Overflow.Hidden;
                m_Text.style.marginLeft     = RDTheme.DragGhostTextPadding;
                m_Text.style.marginRight    = RDTheme.DragGhostTextPadding;
                Add(m_Text);
            }
        }

        /// <summary>
        /// 源条目的压暗遮罩: 铺满条目的一层半透明黑, 不参与布局、不拦截指针。
        ///
        /// 单独一个类型同样是为了按精确类型复用, 不把"绝对定位 + 四边贴满"的样式漏给别的控件。
        /// </summary>
        private sealed class DimOverlay : VisualElement
        {
            public DimOverlay()
            {
                pickingMode = PickingMode.Ignore;

                style.position        = Position.Absolute;
                style.left            = 0f;
                style.top             = 0f;
                style.right           = 0f;
                style.bottom          = 0f;
                style.backgroundColor = RDTheme.DragSourceDimOverlay;
            }
        }

        /// <summary>
        /// 列边界手柄: 一根 1px 竖线 + 自己的拖拽状态。
        ///
        /// 状态必须挂在元素上而不是 Driver 的字段里: 元素按位置复用, 同一条边界跨帧是同一个实例,
        /// 拖拽中途重绘(改列宽会触发重绘)也不会丢 m_LastX; 换成 Driver 字段就得自己记"拖的是哪条"。
        /// </summary>
        private sealed class ColumnResizeHandle : VisualElement
        {
            /// <summary>自上一次被取走以来累计的横向位移(像素)。</summary>
            private float m_PendingDelta;

            private bool  m_Dragging;
            private float m_LastX;

            /// <summary>
            /// 拖拽中请求重绘。保留模式没有"每帧 OnGUI": 不主动排重绘, 增量要等下一次别的原因
            /// 触发的绘制才会被取走 —— 用户看到的就是"拖了半天不动, 松手后才跳一格"。
            /// </summary>
            private readonly Action m_OnDragged;

            public ColumnResizeHandle(Action onDragged)
            {
                m_OnDragged = onDragged;

                var line = new VisualElement();
                line.style.width           = 1f;
                line.style.flexGrow        = 0f;
                line.style.flexShrink      = 0f;
                line.style.alignSelf       = Align.Stretch;
                line.style.backgroundColor = RDTheme.TableHeaderLineColor;
                Add(line);

                RegisterCallback<PointerDownEvent>(OnPointerDown);
                RegisterCallback<PointerMoveEvent>(OnPointerMove);
                RegisterCallback<PointerUpEvent>(OnPointerUp);
                RegisterCallback<PointerCaptureOutEvent>(_ => m_Dragging = false);
            }

            /// <summary>取走累计增量并清零: 增量只能被应用一次。</summary>
            public float ConsumeDelta()
            {
                var delta = m_PendingDelta;
                m_PendingDelta = 0f;
                return delta;
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0) return;
                m_Dragging = true;
                m_LastX    = evt.position.x;
                // 捕获指针: 拖出 5px 抓取区后仍能收到 Move/Up, 否则拖快了就"松手"
                this.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (m_Dragging == false) return;
                m_PendingDelta += evt.position.x - m_LastX;
                m_LastX         = evt.position.x;
                evt.StopPropagation();

                // 排一次重绘, 增量才会被取走并变成新的列宽(排重绘本身是延后合并的, 不重入)
                if (Mathf.Abs(m_PendingDelta) >= 1f) m_OnDragged?.Invoke();
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                if (m_Dragging == false) return;
                m_Dragging = false;
                if (this.HasPointerCapture(evt.pointerId)) this.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        //-------------------------------------- 状态栈 --------------------------------------
        // 六个成员只是转调: 真正的栈在 StateStack 里, 因为元素复用时要读栈顶重刷外观 ——
        // 那是渲染逻辑, 不该伸手进 Driver 的私有字段。

        public virtual void PushColor(Color color) => m_StateStack.PushColor(color);

        public virtual void PopColor() => m_StateStack.PopColor();

        public virtual void PushEnabled(bool enabled) => m_StateStack.PushEnabled(enabled);

        public virtual void PopEnabled() => m_StateStack.PopEnabled();

        public virtual void PushLabelWidth(float? width) => m_StateStack.PushLabelWidth(width);

        public virtual void PopLabelWidth() => m_StateStack.PopLabelWidth();

        public virtual void BeginCheck()
        {
            // 这一段所在的容器: DrawValueMember 的结构是 BeginHorizontal → BeginCheck → 控件,
            // 所以控件及其内部子控件(Toolbar 页签等)都在它下面
            m_Checks.Push(m_Tree.CurrentContainer ?? m_Root);
        }

        public virtual bool EndCheck()
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
            UnityEngine.Debug.Log($"[ContentJitter] NoteChanged {control.GetType().Name}");
            m_ChangedElements.Add(control);
            RequestRepaint();
        }

        /// <summary>
        /// 该控件是否有"两次绘制之间"用户造成、还没被 EndCheck 消费的改动。
        ///
        /// 保留模式与立即模式的本质差别: 改动发生在 OnGUI 执行期间, 同一个帧里
        /// BeginChangeCheck/EndChangeCheck 直接比得出差值; 这里改动发生在两次绘制之间 ——
        /// 用户点完/输完那一刻控件里已经是新值, 而引擎手里的模型值还是旧的。
        /// 所以各控件**不能无条件 SetValueWithoutNotify(模型值)**: 那会把用户刚弄的内容
        /// 在随后的重绘里当场抹掉, 并把旧值当"新值"交回引擎写回模型 ——
        /// 表现就是控件能点能选, 但瞬间弹回旧值, 等于什么都没发生。
        /// 有未消费改动时跳过覆写, 把用户的当前值交给引擎; 下次绘制模型已是新值, 再正常同步。
        /// </summary>
        private bool HasPendingChange(VisualElement element) => m_ChangedElements.Contains(element);

        //-------------------------------------- 文本 --------------------------------------

        public virtual void Label(string text, in DrawTextSpec spec)
        {
            // emoji 没有字形, 走内联贴图: 一行里文本段用 Label, emoji 段用 Image
            if (DrawEmoji.Contains(text))
            {
                LabelWithEmoji(text, spec);
                return;
            }

            var label = m_Tree.Take(() => new Label(), null);
            label.text  = text ?? "";
            ApplyTextSpec(label, spec);
        }

        /// <summary>把"文字 + emoji"切成一段段画在同一行里。</summary>
        private void LabelWithEmoji(string text, in DrawTextSpec spec)
        {
            var row = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(row);
            row.style.flexDirection = FlexDirection.Row;
            // 横向排组里子元素一律顶对齐: "键:\n值"这类带换行的文本段把行撑高后,
            // 定高的 emoji 贴图与后面的文字都贴在行顶(错位只有字体盒自身那 1~2px)。
            // 默认 alignItems=Stretch: 定高 Image 贴顶, 而被拉伸到整行高的文字 Label 按
            // MiddleLeft 垂直居中 —— "已停止"会明显掉到行中间, 与预期对不上。这里显式顶对齐。
            // 每轮都写: ResetContainerResidue 只清 alignSelf, 复用来的元素可能带着别处的 alignItems。
            row.style.alignItems = Align.FlexStart;
            row.style.marginBottom  = RowSpacing;
            m_Tree.PushAsScope(row);

            var runs = DrawEmoji.Split(text);
            for (int i = 0; i < runs.Count; i++)
            {
                var run = runs[i];
                if (run.IsEmoji)
                {
                    var image = m_Tree.Take(() => new Image(), null);
                    image.image        = run.Texture;
                    image.style.width  = DrawEmoji.Size;
                    image.style.height = DrawEmoji.Size;
                    image.style.marginRight = 2f;
                }
                else
                {
                    if (string.IsNullOrEmpty(run.Text)) continue;

                    var part = m_Tree.Take(() => new Label(), null);
                    part.text = run.Text;
                    ApplyTextSpec(part, spec);
                    part.style.marginBottom = 0f;   // 行间距由外层 row 给
                }
            }

            m_Tree.Pop();
        }

        public virtual void KeyValueLabel(string label, string value)
        {
            var row = m_Tree.Take(() => new VisualElement(), null);
            Styler.ResetContainerResidue(row);
            row.style.flexDirection = FlexDirection.Row;
            m_Tree.PushAsScope(row);

            var key = m_Tree.Take(() => new Label(), null);
            key.text     = label ?? "";
            key.style.minWidth   = m_StateStack.LabelWidth ?? Styler.AutoLabelWidth;  // 列宽下限, 与控件行的标签列同宽
            key.style.width      = StyleKeyword.Null;               // 长文字按内容撑开, 不做截断
            key.style.whiteSpace = WhiteSpace.NoWrap;
            // 与  LabelField 同观感: 标签列放不下就裁掉, 让它溢到值上会读成两个词粘一起
            key.style.overflow     = Overflow.Hidden;
            key.style.textOverflow = TextOverflow.Ellipsis;
            key.style.unityFontStyleAndWeight = FontStyle.Normal;

            if (DrawEmoji.Contains(value))
            {
                var runs = DrawEmoji.Split(value ?? "");
                for (int i = 0; i < runs.Count; i++)
                {
                    var run = runs[i];
                    if (run.IsEmoji)
                    {
                        var emoji = m_Tree.Take(() => new Image(), null);
                        emoji.image        = run.Texture;
                        emoji.style.width  = DrawEmoji.Size;
                        emoji.style.height = DrawEmoji.Size;
                        emoji.style.marginRight = 2f;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(run.Text)) continue;

                        var part = m_Tree.Take(() => new Label(), null);
                        part.text = run.Text;
                        Layout.FillWidth(part);
                    }
                }
            }
            else
            {
                var val = m_Tree.Take(() => new Label(), null);
                val.text  = value ?? "";
                Layout.FillWidth(val);
                // 同字段: 值列允许收缩并裁字, 否则长文本会把整行/整块面板顶宽
                val.style.minWidth     = 0f;
                val.style.overflow     = Overflow.Hidden;
                val.style.textOverflow = TextOverflow.Ellipsis;
            }

            row.style.marginBottom = RowSpacing;
            m_Tree.Pop();
        }

        public virtual void HelpBox(string message, DrawMessageKind kind)
        {
            var box = m_Tree.Take(() => new UnityEngine.UIElements.HelpBox("", ToHelpBoxType(kind)), null);
            box.text = message ?? "";
            box.style.marginBottom = RowSpacing * 2f;
        }

        public virtual void ProgressBar(float value, string title, float height)
        {
            var bar = m_Tree.Take(() => new UnityEngine.UIElements.ProgressBar(), null);

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
        /// 所以图标钉死在左侧一小块留白里, 按钮的 paddingLeft 让出这块区域, 观感与  ImageLeft 一致。
        /// 非 Texture2D 的资源这里显示不出来, 这类图标只在  后端有效。
        /// </summary>
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
                        left      = RDTheme.ButtonIconLeft,
                        top       = Length.Percent(50f),    // 垂直居中: 钉在中线再上移半个自身
                        translate = new Translate(0f, Length.Percent(-50f)),
                        width     = RDTheme.ButtonIconSize,
                        height    = RDTheme.ButtonIconSize,
                    }
                };
                button.Add(image);
                button.style.paddingLeft = RDTheme.ButtonIconPadLeft;   // 文字区让开图标
            }

            if (icon is Texture2D texture) image.image = texture;
        }

        /// <summary>mini 文本的字号(对应  miniLabel)。</summary>
        private static float MiniFontSize => RDTheme.MiniFontSize;

        /// <summary>行与行之间的空隙(取  standardVerticalSpacing)。</summary>
        private static float RowSpacing => RDTheme.RowSpacing;

        private void ApplyTextSpec(Label label, in DrawTextSpec spec)
        {
            label.style.unityTextAlign = spec.Align == DrawAlign.Center
                ? TextAnchor.MiddleCenter
                : spec.Align == DrawAlign.Right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

            label.style.unityFontStyleAndWeight = spec.Bold ? FontStyle.Bold : FontStyle.Normal;
            label.style.whiteSpace              = spec.Wrap ? WhiteSpace.Normal : WhiteSpace.NoWrap;
            var hint = LayoutPolicy.ForLabel(spec);
            // 元素按位置复用: 同一位置上一轮可能画过带锁宽/锁高的别的文字(表格头/定高标题), 条件
            // 不成立时必须显式清回 Null 交还 USS, 否则残留会把这一轮的文字钉在别人的尺寸上
            label.style.width  = hint.Width  > 0f ? hint.Width  : StyleKeyword.Null;
            label.style.height = hint.Height > 0f ? hint.Height : StyleKeyword.Null;

            // 上限宽(min=0, max=w): 与  MinWidth(0)+MaxWidth(w) 同义。
            // 定宽会把外层容器的最小宽一起撑大 —— 表格表头/长文本列能把整块面板顶出一条横向滚动条,
            // 上限宽不会。当前文字不需要上限宽时必须清回 Null, 否则复用元素钉在上一轮的宽度上。
            label.style.maxWidth = hint.MaxWidth > 0f ? hint.MaxWidth : StyleKeyword.Null;
            if (hint.MaxWidth > 0f) label.style.minWidth = 0f;
            // 垂直间距唯一来源是 RowSpacing: 主题给 Label 自带的 margin 全部显式归零,
            // 否则 (standardVerticalSpacing) 与 (主题 margin + RowSpacing) 行距对不上
            label.style.marginTop    = 0f;
            label.style.marginBottom = RowSpacing;
            label.style.marginLeft   = 0f;
            label.style.marginRight  = 0f;

            // 标签样式 stretchWidth=true: 横排里占满本行剩余宽(锁宽的除外),
            // 把后面的内容(别的文本段 / FlexibleSpace / 控件)推到右边。
            // 但 flexGrow 是"主轴"增长: 横排(Row)里才是宽; 竖排(Column)里会变成"长高" ——
            // 标签被撑成一条大块, 行距观感全毁, 盖住/挤走后面的控件(pickingMode 默认 Position,
            // 被盖住就等于吃掉点击)。竖排里的横向占满是 cross-axis, 交给默认的 alignSelf=Stretch。
            // 因此占满宽统一交给 Layout(FillWidth 会按父容器方向翻译成 flexGrow 或 alignSelf)。
            if (hint.FillWidth) Layout.FillWidth(label);
            else label.style.flexGrow = 0f;

            // 开关富文本: 与 GUISettings.richText 对齐(富文本标记由引擎在 ToTextSpec 里声明)。
            // 不设的话 <color> / <b> 之类标记会被当成普通字符原样显示出来。
            label.enableRichText = spec.RichText;

            // mini(表格头/集合计数等次要文字)用更小的字号; UIElements 没有 mini 标签的现成等价字号
            if (spec.Mini) label.style.fontSize = MiniFontSize;
            else label.style.fontSize = StyleKeyword.Null;
        }

        /// <summary>
        /// 工具栏/分段页签里一枚项的几何: 平分整行、紧贴无间隙、扁高、直角、文字居中。
        /// Toolbar 项靠 toolbarButton 皮肤自带这套几何, ToolbarToggle 默认
        /// 内容宽 + 自带圆角和项间距, 必须逐项归一对齐。"平分"这一条读 <see cref="LayoutPolicy.ForToolbarItem"/>。
        /// </summary>
        private static void ApplyToolbarToggleGeometry(UnityEditor.UIElements.ToolbarToggle toggle)
        {
            var hint = LayoutPolicy.ForToolbarItem();
            if (hint.FillWidth) Layout.FillWidth(toggle);
            toggle.style.marginLeft     = 0f;
            toggle.style.marginRight    = 0f;
            toggle.style.height         = RDTheme.ToolbarHeight;
            toggle.style.borderBottomLeftRadius  = 0f;
            toggle.style.borderBottomRightRadius = 0f;
            toggle.style.borderTopLeftRadius     = 0f;
            toggle.style.borderTopRightRadius    = 0f;
            toggle.style.unityTextAlign = TextAnchor.MiddleCenter;
        }

        //-------------------------------------- 控件 --------------------------------------

        public virtual bool Button(string text, in DrawButtonSpec spec)
        {
            var button = m_Tree.Take(() =>
            {
                var made = new Button();
                made.clicked += () =>
                {
                    m_ClickedKey = made.userData as string;
                    //  是立即模式, 点击当帧 OnGUI 重跑就能消费; 保留模式必须主动排一次绘制,
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
            var hint = LayoutPolicy.ForButton(spec);
            // 同 Label: 复用的按钮可能带着上一轮的锁宽/锁高(比如别处的定高小按钮), 条件不成立要清掉。
            // 高度缺省取量出来的  按钮高: 交还给主题的话两种字体/主题下按钮会各走一套高度
            button.style.height = hint.Height > 0f ? hint.Height : RDTheme.ButtonHeight;
            button.style.width  = hint.Width  > 0f ? hint.Width  : StyleKeyword.Null;

            // 定宽按钮不可压缩: flex 项默认会被压窄 —— 集合头的 "+"、翻页箭头、条目的 "×"
            // 都是定宽小键, 空间一紧就被削掉半个字形
            button.style.minWidth   = hint.Width > 0f ? hint.Width : StyleKeyword.Null;
            button.style.flexShrink = hint.Width > 0f ? 0f : StyleKeyword.Null;

            // 对齐 : ExpandWidth(默认 true) → 占满可用宽(横排时平分), 否则缩回内容宽
            if (hint.FillWidth)
            {
                // 竖排里给 flexGrow 等于"跟着单元格多余高度一起长高"(按钮被拉成 35px 的就是它)
                Layout.FillWidth(button);
            }
            else
            {
                Layout.Fixed(button);
            }

            // 外边距照抄 按钮皮肤: 相邻按钮之间的缝正是两条边距拼出来的, 只把主题
            // margin 归零的话整排按钮会贴成一条(与  明显不同)。
            // 无底纹键与定宽键在 走的是 margin=0 的紧凑样式, 这里同样不留横向间距。
            var buttonMargin = hint.MarginLeft;   // ForButton 已算好: 无底纹/定宽 → 0, 否则 ButtonMargin
            button.style.marginTop    = buttonMargin > 0f ? buttonMargin : RowSpacing;
            button.style.marginBottom = buttonMargin > 0f ? buttonMargin : RowSpacing;
            button.style.marginLeft   = buttonMargin;
            button.style.marginRight  = buttonMargin;

            // 无底纹键(集合头的 + / 翻页箭头 / 条目 ×): 只留字形 —— 主题按钮的底纹图片与边框都清掉,
            // 语义与  FramelessButtonStyle 一致。复用元素时要逐项清, 否则上一轮的框会留着。
            if (spec.Frameless)
            {
                button.style.backgroundImage   = new StyleBackground(StyleKeyword.None);
                button.style.backgroundColor   = StyleKeyword.None;
                button.style.borderTopWidth    = 0f;
                button.style.borderBottomWidth = 0f;
                button.style.borderLeftWidth   = 0f;
                button.style.borderRightWidth  = 0f;
                button.style.paddingLeft       = 2f;
                button.style.paddingRight      = 2f;
                button.style.paddingTop        = 0f;
                button.style.paddingBottom     = 0f;
            }
            // : 背景 = 控件纹理 × 底纹灰度(暗一档), 字 = contentColor × 底纹灰度(本色, 亮)。
            // 显式背景(spec.HasBackground)与色栈都要过 TextureTinted 压暗, 直接铺原色会亮得刺眼
            else if (spec.HasBackground) button.style.backgroundColor = Styler.ButtonTinted(spec.Background);
            else if (m_StateStack.CurrentColor.HasValue) button.style.backgroundColor = Styler.ButtonTinted(m_StateStack.CurrentColor.Value);
            else button.style.backgroundColor = StyleKeyword.Null;   // 复用残留: 别处的绿/蓝按钮色要清掉

            if (m_StateStack.CurrentColor.HasValue) button.style.color = m_StateStack.CurrentColor.Value;

            if (m_ClickedKey != null && m_ClickedKey == button.userData as string)
            {
                m_ClickedKey = null;
                // 按钮的效果(换面板/改数据/增删元素)要再画一次才显示得出来: 引擎的 Mutate 只通知宿主
                // Undo/写脏, 不排重绘 ——  由下一帧 OnGUI 兜住, 保留模式这里必须主动排
                RequestRepaint();
                return true;
            }

            return false;
        }

        public virtual bool Foldout(bool expanded, string label)
        {
            var foldout = m_Tree.Take(() =>
            {
                var made = new Foldout();
                // 记进改动集合而不只排重绘: 折叠状态也走 HasPendingChange 那套"不拿旧值覆写"的逻辑
                made.RegisterValueChangedCallback(_ => NoteChanged(made));   // 折叠变化要重绘才被引擎看到
                return made;
            }, null);

            foldout.text = label ?? "";
            if (HasPendingChange(foldout) == false) foldout.SetValueWithoutNotify(expanded);
            foldout.style.marginTop    = 0f;   // 主题 margin 归零, 行距唯一来源是 RowSpacing
            foldout.style.marginBottom = RowSpacing;
            return foldout.value;
        }

        public virtual string TextField(string label, string value, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new UnityEngine.UIElements.TextField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value ?? "");
            return field.value;
        }

        public virtual long IntegerField(string label, long value, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new LongField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value);
            return field.value;
        }

        public virtual double FloatField(string label, double value, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new DoubleField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value);
            return field.value;
        }

        public virtual float Slider(string label, float value, float min, float max, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new Slider();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            field.lowValue  = min;
            field.highValue = max;
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value);
            return field.value;
        }

        //-------------------------------------- 自绘控件(Phase 4) --------------------------------------

        /// <summary>
        /// 是否用**自绘**控件替换宿主的 Toggle / Slider。
        ///
        /// 宿主控件的勾选框与滑块取自 USS 主题, 与  GUISkin 天然是两套; 自绘版按 <see cref="RDTheme"/>
        /// 的令牌自己画, 只为把这两类"纯交互控件"的观感拉齐(它们不涉及文本编辑与浮层, 自绘的代价可控)。
        ///
        /// 行为与原生控件等价: 点击/拖动/禁用/标签列宽/未消费改动不覆写, 都走同一套机制。
        /// **没有**自绘的是需要宿主能力的那些 —— TextField/IntegerField/FloatField(文本编辑与 IME)、
        /// Popup(下拉浮层)、EnumFlags(位组合菜单)、ObjectField/StructField(对象选择与复合控件),
        /// 它们仍委托给 UIElements 原生控件。
        ///
        /// 自绘版观感不对时把它置 false 即退回原生控件, 其余代码不动。
        /// </summary>
        public static bool UseSelfDrawnControls { get; set; } = true;

        /// <summary>自绘复选框(不经宿主 Toggle)。返回用户当前值。</summary>
        internal bool PaintedToggleField(string label, bool value, float width)
        {
            var element = m_Tree.Take(() =>
            {
                var made = new PaintedToggle();
                made.UserToggled = () => NoteChanged(made);
                return made;
            }, null);

            SyncPaintedControl(element, label, width);

            // 与原生控件同一套规则: 有未消费的用户改动时不拿模型旧值覆写(见 HasPendingChange)
            if (HasPendingChange(element) == false) element.SetChecked(value);
            return element.Checked;
        }

        /// <summary>自绘滑杆(不经宿主 Slider)。返回用户当前值。</summary>
        internal float PaintedSliderField(string label, float value, float min, float max, float width)
        {
            var element = m_Tree.Take(() =>
            {
                var made = new PaintedSlider();
                made.UserDragged = () => NoteChanged(made);
                return made;
            }, null);

            SyncPaintedControl(element, label, width);
            element.SetRange(min, max);

            if (HasPendingChange(element) == false) element.SetValue(value);
            return element.Value;
        }

        /// <summary>
        /// 自绘控件的行布局: 与 <see cref="Styler.ApplyLabelAndWidth{T}"/> 同义 ——
        /// 标签列 + 控件占满/定宽 + 固定行高 + 归零外边距。区别只是元素不是 BaseField, 标签得自己排。
        /// </summary>
        private void SyncPaintedControl(PaintedControl element, string label, float width)
        {
            // 标签列宽走状态栈: 表格单元格里是 0(也就是只画控件), 与普通字段的表现一致
            var labelWidth = m_StateStack.LabelWidth ?? Styler.AutoLabelWidth;
            element.SyncLabel(label, labelWidth);

            var hint = LayoutPolicy.ForField(width);
            if (hint.Width > 0f)
            {
                element.style.width    = hint.Width;
                element.style.flexGrow = 0f;
            }
            else
            {
                element.style.width = StyleKeyword.Null;
                Layout.FillWidth(element);
            }

            // 行高与行距以  为基准(与其它字段同一套), 否则自绘控件会在行里偏高/偏低
            element.style.height       = RDTheme.FieldMinHeight;
            element.style.minHeight    = RDTheme.FieldMinHeight;
            element.style.marginTop    = 0f;
            element.style.marginBottom = RDTheme.RowSpacing;
            element.style.marginLeft   = 0f;
            element.style.marginRight  = 0f;
        }

        public virtual bool ToggleField(string label, bool value, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new UnityEngine.UIElements.Toggle();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value);
            return field.value;
        }

        public virtual int Popup(string label, int selected, string[] options, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new PopupField<string>();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            field.choices = new List<string>(options ?? System.Array.Empty<string>());
            var index = System.Math.Clamp(selected, 0, System.Math.Max(0, field.choices.Count - 1));
            // 有未消费的下拉改动时不能拿模型旧值覆写: 会把用户刚选的项弹回旧值(表现为"点了没反应"),
            // 并把旧 index 当新值交回引擎。choices 每轮重设不影响值, 可以照常设。
            if (HasPendingChange(field) == false)
                field.SetValueWithoutNotify(index >= 0 && index < field.choices.Count ? field.choices[index] : "");
            return field.index;
        }

        public virtual Enum EnumFlagsField(string label, Enum value, float width = 0f)
        {
            if (value == null) return null;

            // 多选枚举用 UnityEditor.UIElements 的 EnumFlagsField: 即 Unity 自己的"位组合多选"控件。
            // 全限定: EnumFlagsField 在 UnityEditor.UIElements 里(本文件只 using 了 UnityEngine.UIElements)
            // 必须把枚举类型传给构造: 无参构造的字段不知道自己是哪个枚举, SetValueWithoutNotify 绑不上,
            // 会一直显示默认的 "Nothing"(值 0)。该实现无法从 value 推断类型。
            var field = m_Tree.Take(() =>
            {
                var made = new UnityEditor.UIElements.EnumFlagsField(value);
                // 保留模式: 用户改勾选必须靠这个回调进 m_ChangedElements, 引擎才会把新值写回模型;
                // 少了它, 下一帧 SetValueWithoutNotify 用旧模型值一覆盖, 多选就丢了(与 Toggle/Popup 同一套路)
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value);
            return field.value;
        }

        //-------------------------------------- 自绘分段工具条(Phase 4) --------------------------------------

        /// <summary>自绘分段工具条(单选)。返回选中下标。</summary>
        internal int PaintedToolbarField(string[] options, int selected, float width)
        {
            var bar = m_Tree.Take(() =>
            {
                var made = new PaintedToolbar();
                made.UserClicked = () => NoteChanged(made);
                return made;
            }, null);

            SyncToolbarGeometry(bar, width);
            bar.SetSegments(options);

            // 刚点过的那一段: 视觉与返回值立刻切过去(模型由 ChangeApplier 在这一帧末尾写回)
            if (HasPendingChange(bar))
            {
                var clicked = bar.TakeClick();
                if (clicked >= 0)
                {
                    bar.SetSelected(clicked);
                    return clicked;
                }
            }

            bar.SetSelected(selected);
            return selected;
        }

        /// <summary>自绘分段工具条(多选页签)。返回本帧被点击的下标(无点击 -1)。</summary>
        internal int PaintedMultiToolbarField(string[] options, bool[] states, float width)
        {
            var bar = m_Tree.Take(() =>
            {
                var made = new PaintedToolbar();
                made.UserClicked = () => NoteChanged(made);
                return made;
            }, null);

            SyncToolbarGeometry(bar, width);
            bar.SetSegments(options);

            if (HasPendingChange(bar))
            {
                var clicked = bar.TakeClick();
                if (clicked >= 0)
                {
                    bar.SetStates(states, clicked);
                    return clicked;
                }
            }

            bar.SetStates(states, -1);
            return -1;
        }

        /// <summary>
        /// 工具条的几何: 占满可用宽(或定宽)、扁高、归零外边距。
        /// 与 <see cref="ApplyToolbarToggleGeometry"/> 同一套数字, 只是元素换成了自绘的一条。
        /// </summary>
        private static void SyncToolbarGeometry(PaintedToolbar bar, float width)
        {
            if (width > 0f)
            {
                bar.style.width    = width;
                bar.style.flexGrow = 0f;
            }
            else
            {
                bar.style.width = StyleKeyword.Null;
                Layout.FillWidth(bar);
            }

            bar.style.height       = RDTheme.ToolbarHeight;
            bar.style.marginTop    = 0f;
            bar.style.marginBottom = RowSpacing;
            bar.style.marginLeft   = 0f;
            bar.style.marginRight  = 0f;
            bar.style.paddingLeft  = 0f;
            bar.style.paddingRight = 0f;
        }

        public virtual int Toolbar(int selected, string[] options, float width = 0f)
        {
            var bar = m_Tree.Take(() => new UnityEditor.UIElements.Toolbar(), null);
            // Toolbar 本体也要占满可用宽: 它在横排里 flexGrow 默认 0 会缩成内容宽,
            // 内部 toggle 的 flexGrow=1 就只能平分那一小块。竖排父里改用 alignSelf(见 FillWidth)
            Layout.FillWidth(bar);
            bar.style.marginTop    = 0f;   // 主题 margin 归零, 行距唯一来源是 RowSpacing
            bar.style.marginBottom = RowSpacing;
            // 分段式页签: 容器不留内边距, 项与项才能紧贴成一条( Toolbar 的样子)
            bar.style.paddingLeft  = 0f;
            bar.style.paddingRight = 0f;
            m_Tree.PushAsScope(bar);

            var result   = selected;
            // 点击消费后, 循环后面迭代里"当前选中项"的 value 不许把结果覆盖回旧值
            var consumed = false;
            for (var i = 0; i < (options?.Length ?? 0); i++)
            {
                var index = i;
                var toggle = m_Tree.Take(() =>
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
                ApplyToolbarToggleGeometry(toggle);
                // 同上: 刚点过的页签不能拿旧 selected 覆写, 否则视觉上弹回旧项(消费靠 m_ClickedKey, 与值无关)
                if (HasPendingChange(toggle) == false) toggle.SetValueWithoutNotify(index == selected);

                // 消费点击(与 Button 同一语义: 取到一次即清掉; 点到当前项不算切换, 与  一致)。
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

            m_Tree.Pop();
            return result;
        }

        public virtual int ToolbarMulti(string[] options, bool[] states, float width = 0f)
        {
            var bar = m_Tree.Take(() => new UnityEditor.UIElements.Toolbar(), null);
            // 与 Toolbar() 同一套容器样式: 占满可用宽、页签紧贴成一条
            Layout.FillWidth(bar);
            bar.style.marginTop    = 0f;
            bar.style.marginBottom = RowSpacing;
            bar.style.paddingLeft  = 0f;
            bar.style.paddingRight = 0f;
            m_Tree.PushAsScope(bar);

            var result = -1;
            for (var i = 0; i < (options?.Length ?? 0); i++)
            {
                var index = i;
                var toggle = m_Tree.Take(() =>
                {
                    var made = new UnityEditor.UIElements.ToolbarToggle();
                    made.RegisterValueChangedCallback(evt =>
                    {
                        // 多选: 点亮/熄灭两次方向的点击都记(位的取反由引擎算), 取到即消费
                        m_ClickedKey = made.userData as string;
                        NoteChanged(made);
                    });
                    return made;
                }, null);

                toggle.text = options[i];
                ApplyToolbarToggleGeometry(toggle);
                if (HasPendingChange(toggle) == false)
                    toggle.SetValueWithoutNotify(index < (states?.Length ?? 0) && states[index]);

                if (m_ClickedKey != null && m_ClickedKey == toggle.userData as string)
                {
                    m_ClickedKey = null;
                    result = index;
                }
            }

            m_Tree.Pop();
            return result;
        }

        public virtual UnityEngine.Object ObjectField(string label, UnityEngine.Object value, Type type, float width = 0f)
        {
            var field = m_Tree.Take(() =>
            {
                var made = new UnityEditor.UIElements.ObjectField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            field.objectType = type;
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value);
            return field.value;
        }

        public virtual object StructField(string label, object value, Type type, float width = 0f)
        {
            if (type == typeof(Vector2))    return Struct<Vector2Field, Vector2>(label, value, width);
            if (type == typeof(Vector3))    return Struct<Vector3Field, Vector3>(label, value, width);
            if (type == typeof(Vector4))    return Struct<Vector4Field, Vector4>(label, value, width);
            if (type == typeof(Vector2Int)) return Struct<Vector2IntField, Vector2Int>(label, value, width);
            if (type == typeof(Vector3Int)) return Struct<Vector3IntField, Vector3Int>(label, value, width);

            // Quaternion 没有专用控件: 与  一致按欧拉角编辑
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
                var field = m_Tree.Take(() =>
                {
                    var made = new UnityEditor.UIElements.LayerMaskField();
                    made.RegisterValueChangedCallback(_ => NoteChanged(made));
                    return made;
                }, null);

                m_Styler.ApplyLabelAndWidth(field, label, width);
                var mask = value is LayerMask layerMask ? layerMask.value : 0;
                if (HasPendingChange(field) == false) field.SetValueWithoutNotify(mask);
                return (LayerMask)field.value;
            }

            if (type == typeof(AnimationCurve))
            {
                var field = m_Tree.Take(() =>
                {
                    var made = new UnityEditor.UIElements.CurveField();
                    made.RegisterValueChangedCallback(_ => NoteChanged(made));
                    return made;
                }, null);

                m_Styler.ApplyLabelAndWidth(field, label, width);
                if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value as AnimationCurve);
                return field.value;
            }

            if (type == typeof(Gradient))
            {
                var field = m_Tree.Take(() =>
                {
                    var made = new UnityEditor.UIElements.GradientField();
                    made.RegisterValueChangedCallback(_ => NoteChanged(made));
                    return made;
                }, null);

                m_Styler.ApplyLabelAndWidth(field, label, width);
                if (HasPendingChange(field) == false) field.SetValueWithoutNotify(value as Gradient);
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
            var field = m_Tree.Take(() =>
            {
                var made = new TField();
                made.RegisterValueChangedCallback(_ => NoteChanged(made));
                return made;
            }, null);

            m_Styler.ApplyLabelAndWidth(field, label, width);
            var typed = value is TValue typedValue ? typedValue : default;
            if (HasPendingChange(field) == false) field.SetValueWithoutNotify(typed);
            return field.value;
        }

        //-------------------------------------- 自绘控件的元素实现 --------------------------------------

        /// <summary>
        /// 自绘控件的公共骨架: 一行 [标签列][控件自身]。
        ///
        /// 不用 BaseField 的原因: BaseField 的输入区外观来自 USS 主题, 而我们正是要绕开主题。
        /// 代价是标签列、行高、宽度都得自己按 <see cref="Styler"/> 的同一套规则排(见 SyncPaintedControl),
        /// 换来的是勾选框/滑杆的颜色尺寸只由 <see cref="RDTheme"/> 决定。
        /// </summary>
        private abstract class PaintedControl : VisualElement
        {
            private readonly Label m_Label;

            protected PaintedControl()
            {
                // 复用的元素可能带着上一轮别的控件的宽高/外边距, 先清一遍(与其它容器同一套路)
                Styler.ResetContainerResidue(this);

                style.flexDirection = FlexDirection.Row;
                style.alignItems    = Align.Center;
                style.flexShrink    = 0f;

                m_Label = new Label { pickingMode = PickingMode.Ignore };
                m_Label.style.flexShrink  = 0f;
                m_Label.style.flexGrow    = 0f;
                m_Label.style.whiteSpace  = WhiteSpace.NoWrap;
                m_Label.style.marginLeft  = 0f;
                m_Label.style.marginRight = 0f;
                Add(m_Label);
            }

            /// <summary>
            /// 同步标签列。labelWidth = 0(表格单元格) 与空标签都表示"只画控件" ——
            ///  labelWidth=0 就是把标签列整段拿掉, 这里必须同义, 否则单元格里的控件会被推歪。
            /// </summary>
            public void SyncLabel(string text, float labelWidth)
            {
                var show = string.IsNullOrEmpty(text) == false && labelWidth > 0f;

                m_Label.style.display  = show ? DisplayStyle.Flex : DisplayStyle.None;
                m_Label.text           = show ? text : "";
                m_Label.style.minWidth = show ? labelWidth : 0f;
            }
        }

        /// <summary>
        /// 自绘复选框: 14×14 方框 + 勾 + 标签列。
        ///
        /// 只有方框接收点击(与  Toggle 一致: 点标签不切换), 切换后自己维护显示状态,
        /// 并把"用户改过"这件事交给驱动的 <c>NoteChanged</c> 登记。
        /// </summary>
        private sealed class PaintedToggle : PaintedControl
        {
            private readonly VisualElement m_Box;
            private readonly Label         m_Check;

            /// <summary>用户点击时触发(驱动在创建时接到 NoteChanged 上)。</summary>
            public Action UserToggled;

            public bool Checked { get; private set; }

            public PaintedToggle()
            {
                m_Box = new VisualElement();
                m_Box.style.width           = RDTheme.CheckboxSize;
                m_Box.style.height          = RDTheme.CheckboxSize;
                m_Box.style.flexShrink      = 0f;
                m_Box.style.backgroundColor = RDTheme.CheckboxBackground;

                m_Box.style.borderTopWidth    = RDTheme.CheckboxBorderThickness;
                m_Box.style.borderBottomWidth = RDTheme.CheckboxBorderThickness;
                m_Box.style.borderLeftWidth   = RDTheme.CheckboxBorderThickness;
                m_Box.style.borderRightWidth  = RDTheme.CheckboxBorderThickness;
                m_Box.style.borderTopColor    = RDTheme.CheckboxBorder;
                m_Box.style.borderBottomColor = RDTheme.CheckboxBorder;
                m_Box.style.borderLeftColor   = RDTheme.CheckboxBorder;
                m_Box.style.borderRightColor  = RDTheme.CheckboxBorder;

                m_Box.RegisterCallback<PointerDownEvent>(OnPointerDown);

                // 勾用字形而不是贴图: 一个 ✓ 在 14px 的框里居中, 缩放/DPI 都不用另做资源
                m_Check = new Label("✓") { pickingMode = PickingMode.Ignore };
                m_Check.style.position        = Position.Absolute;
                m_Check.style.left            = 0f;
                m_Check.style.top             = 0f;
                m_Check.style.right           = 0f;
                m_Check.style.bottom          = 0f;
                m_Check.style.unityTextAlign  = TextAnchor.MiddleCenter;
                m_Check.style.fontSize        = RDTheme.CheckboxCheckFontSize;
                m_Check.style.color           = RDTheme.CheckboxCheck;
                m_Box.Add(m_Check);

                Add(m_Box);
            }

            /// <summary>按模型值同步显示(有待消费改动时不调用, 见 <c>HasPendingChange</c>)。</summary>
            public void SetChecked(bool value)
            {
                Checked = value;
                m_Check.style.visibility = value ? Visibility.Visible : Visibility.Hidden;
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0) return;

                SetChecked(Checked == false);
                UserToggled?.Invoke();
                evt.StopPropagation();
            }
        }

        /// <summary>
        /// 自绘滑杆: 一条轨道 + 一个圆滑块, 按下/拖动改值。
        ///
        /// 几何换算(端点/越界/退化区间)全在 <see cref="PaintedSliderMath"/> 里, 这里只负责事件与元素位置。
        /// </summary>
        private sealed class PaintedSlider : PaintedControl
        {
            private readonly VisualElement m_Area;
            private readonly VisualElement m_Track;
            private readonly VisualElement m_Thumb;

            private float m_Min;
            private float m_Max;
            private float m_Value;
            private bool  m_Dragging;

            /// <summary>用户拖动时触发(驱动在创建时接到 NoteChanged 上)。</summary>
            public Action UserDragged;

            public float Value => m_Value;

            public PaintedSlider()
            {
                m_Area = new VisualElement();
                m_Area.style.flexGrow         = 1f;
                m_Area.style.height           = RDTheme.FieldMinHeight;
                m_Area.style.justifyContent   = Justify.Center;   // 轨道纵向居中
                m_Area.RegisterCallback<PointerDownEvent>(OnPointerDown);
                m_Area.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                m_Area.RegisterCallback<PointerUpEvent>(OnPointerUp);
                m_Area.RegisterCallback<PointerCaptureOutEvent>(_ => m_Dragging = false);
                m_Area.RegisterCallback<GeometryChangedEvent>(_ => UpdateThumb());

                m_Track = new VisualElement { pickingMode = PickingMode.Ignore };
                m_Track.style.height          = RDTheme.SliderTrackHeight;
                m_Track.style.backgroundColor = RDTheme.SliderTrack;
                m_Area.Add(m_Track);

                m_Thumb = new VisualElement { pickingMode = PickingMode.Ignore };
                var size = RDTheme.SliderThumbSize;
                m_Thumb.style.position           = Position.Absolute;
                m_Thumb.style.width              = size;
                m_Thumb.style.height             = size;
                m_Thumb.style.top                = (RDTheme.FieldMinHeight - size) * 0.5f;
                m_Thumb.style.borderTopLeftRadius     = size * 0.5f;
                m_Thumb.style.borderTopRightRadius    = size * 0.5f;
                m_Thumb.style.borderBottomLeftRadius  = size * 0.5f;
                m_Thumb.style.borderBottomRightRadius = size * 0.5f;
                m_Thumb.style.backgroundColor    = RDTheme.SliderThumb;
                m_Thumb.style.borderTopWidth     = 1f;
                m_Thumb.style.borderBottomWidth  = 1f;
                m_Thumb.style.borderLeftWidth    = 1f;
                m_Thumb.style.borderRightWidth   = 1f;
                m_Thumb.style.borderTopColor     = RDTheme.SliderThumbBorder;
                m_Thumb.style.borderBottomColor  = RDTheme.SliderThumbBorder;
                m_Thumb.style.borderLeftColor    = RDTheme.SliderThumbBorder;
                m_Thumb.style.borderRightColor   = RDTheme.SliderThumbBorder;
                m_Area.Add(m_Thumb);

                Add(m_Area);
            }

            /// <summary>同步区间; 区间变化时把当前值夹回范围内。</summary>
            public void SetRange(float min, float max)
            {
                if (m_Min.Equals(min) && m_Max.Equals(max)) return;

                m_Min = min;
                m_Max = max;
                m_Value = Clamp(m_Value);
                UpdateThumb();
            }

            /// <summary>按模型值同步位置(有待消费改动时不调用, 见 <c>HasPendingChange</c>)。</summary>
            public void SetValue(float value)
            {
                m_Value = Clamp(value);
                UpdateThumb();
            }

            private float Clamp(float value)
            {
                if (m_Max <= m_Min) return m_Min;
                if (value < m_Min) return m_Min;
                if (value > m_Max) return m_Max;
                return value;
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                if (evt.button != 0) return;

                m_Dragging = true;
                m_Area.CapturePointer(evt.pointerId);
                DragTo(evt.localPosition.x);
                evt.StopPropagation();
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (m_Dragging == false) return;

                DragTo(evt.localPosition.x);
                evt.StopPropagation();
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                if (m_Dragging == false) return;

                m_Dragging = false;
                m_Area.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }

            /// <summary>指针 x(相对轨道区左缘) → 值。滑块以中心对准指针, 两端刚好贴边。</summary>
            private void DragTo(float x)
            {
                var travel = m_Area.contentRect.width - RDTheme.SliderThumbSize;
                var value  = PaintedSliderMath.ValueFromX(x - RDTheme.SliderThumbSize * 0.5f, travel, m_Min, m_Max);

                if (value.Equals(m_Value)) return;

                m_Value = value;
                UpdateThumb();
                UserDragged?.Invoke();
            }

            private void UpdateThumb()
            {
                var travel = m_Area.contentRect.width - RDTheme.SliderThumbSize;
                m_Thumb.style.left = PaintedSliderMath.ThumbOffset(m_Value, m_Min, m_Max, travel);
            }
        }

        /// <summary>
        /// 自绘分段工具条: 一排按钮, 选中段铺 <see cref="RDTheme.ToolbarToggleOnColor"/> 的按压色。
        ///
        /// 为什么不用宿主的 <c>Toolbar</c> / <c>ToolbarToggle</c>: 后者的"选中"是 toggle 的 on 态,
        /// 暗色主题下几乎看不出来, 而且每段还带标签列 —— 单选枚举页签会变成
        /// "标签 + 空方框", 与蓝底分段完全不是一回事。
        /// </summary>
        private sealed class PaintedToolbar : VisualElement
        {
            private readonly List<Button> m_Segments = new();
            private int m_ClickedIndex = -1;

            /// <summary>用户点击某一段时触发(驱动在创建时接到 NoteChanged 上)。</summary>
            public Action UserClicked;

            public PaintedToolbar()
            {
                Styler.ResetContainerResidue(this);
                style.flexDirection = FlexDirection.Row;
                style.alignItems    = Align.Stretch;
                style.flexShrink    = 0f;
            }

            /// <summary>
            /// 按选项数复用/补齐分段。只增删尾部, 所以点击闭包里捕获的下标始终稳定;
            /// 段数变化(枚举改了)时才真的创建/移除元素。
            /// </summary>
            public void SetSegments(string[] options)
            {
                var count = options?.Length ?? 0;

                while (m_Segments.Count > count)
                {
                    Remove(m_Segments[m_Segments.Count - 1]);
                    m_Segments.RemoveAt(m_Segments.Count - 1);
                }

                while (m_Segments.Count < count)
                {
                    var index   = m_Segments.Count;
                    var segment = new Button();
                    ApplySegmentStyle(segment, first: index == 0);
                    segment.clicked += () => OnSegmentClicked(index);
                    Add(segment);
                    m_Segments.Add(segment);
                }

                for (var i = 0; i < count; i++) m_Segments[i].text = options[i] ?? "";
            }

            /// <summary>单选高亮: 只有选中段用按压色, 其余交还主题(default)。</summary>
            public void SetSelected(int index)
            {
                for (var i = 0; i < m_Segments.Count; i++)
                {
                    m_Segments[i].style.backgroundColor = i == index
                        ? Styler.ButtonTinted(RDTheme.ToolbarToggleOnColor)
                        : StyleKeyword.Null;
                }
            }

            /// <summary>
            /// 多选高亮: states 亮着的段用按压色; <paramref name="toggledIndex"/> 是这一帧刚点的那一段,
            /// 先就地反相(模型下一帧才更新), 否则点了要等一帧才看见亮灭。
            /// </summary>
            public void SetStates(bool[] states, int toggledIndex)
            {
                for (var i = 0; i < m_Segments.Count; i++)
                {
                    var on = states != null && i < states.Length && states[i];
                    if (i == toggledIndex) on = !on;

                    m_Segments[i].style.backgroundColor = on
                        ? Styler.ButtonTinted(RDTheme.ToolbarToggleOnColor)
                        : StyleKeyword.Null;
                }
            }

            /// <summary>取走并清空这一帧的点击(取一次即消费, 与驱动里 m_ClickedKey 同一语义)。</summary>
            public int TakeClick()
            {
                var clicked = m_ClickedIndex;
                m_ClickedIndex = -1;
                return clicked;
            }

            private void OnSegmentClicked(int index)
            {
                m_ClickedIndex = index;
                UserClicked?.Invoke();
            }

            private static void ApplySegmentStyle(Button segment, bool first)
            {
                segment.style.marginLeft   = 0f;
                segment.style.marginRight  = 0f;
                segment.style.marginTop    = 0f;
                segment.style.marginBottom = 0f;
                segment.style.height       = RDTheme.ToolbarHeight;
                segment.style.flexGrow     = 1f;   // 平分整行
                segment.style.flexShrink   = 1f;

                // 分段紧贴成一条: 圆角归零; 相邻段只留一条竖线, 否则并排两条边就是 2px 粗线
                segment.style.borderTopLeftRadius     = 0f;
                segment.style.borderTopRightRadius    = 0f;
                segment.style.borderBottomLeftRadius  = 0f;
                segment.style.borderBottomRightRadius = 0f;
                segment.style.borderLeftWidth         = first ? 1f : 0f;

                segment.style.unityTextAlign = TextAnchor.MiddleCenter;
            }
        }

    }
}
