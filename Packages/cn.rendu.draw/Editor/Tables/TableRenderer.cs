using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rendering;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Tables
{
    /// <summary>
    /// 表格渲染: [TableList] 的表头 / 数据行 / 列宽分发。
    ///
    /// 表格是这套引擎里最讲究排版的一块 —— 列宽一旦算错就会撑出横向滚动条,
    /// 因此它把"列宽怎么算""什么时候量宽""量到的宽存在哪"这些知识都收在本地,
    /// 不散落到别的渲染器里。
    ///
    /// 单元格里的成员怎么画, 交给 <see cref="IMemberDrawHost"/>(与其它路径同一套规则)。
    /// </summary>
    internal sealed class TableRenderer
    {
        private const int TableIndexColumnWidth = 34;
        private const int TableRemoveColumnWidth = 24;

        /// <summary>弹性均分列的宽度下限: 窗口太窄时宁可出横向滚动条, 也不把列压得没法看。</summary>
        private const int MinTableColumnWidth = 90;

        /// <summary>
        /// 用户拖拽列宽的下限: 比均分下限更松 —— 均分是机器算的, 拖拽是用户明说要窄一点。
        /// 但仍留一条底线: 拖到 0 宽等于"这列没了", 而表格没有复位入口, 用户找不回来。
        /// </summary>
        private const int MinDragColumnWidth = 48;

        /// <summary>
        /// 均分列宽时额外留的横向余量(像素), 只在"量不到数据行宽、只能按层估"的路径上用
        /// (见 <see cref="DistributeTableColumns"/>): 兜"索引/删除列之外"的零头
        /// (行内分隔线 1px、皮肤边距取整误差)。留太少列宽虚高, 数据行比滚动区宽 → 多一条横向滚动条。
        /// </summary>
        private const int TableHorizontalMargin = 4;

        /// <summary>
        /// 首帧(还没量到可用宽)时的兜底宽: 宁可先窄一点, 也不能第一帧就把面板撑出横向滚动条。
        /// 量到真值后会立刻按真值重排。
        /// </summary>
        private const float DefaultTableContentWidth = 400f;

        /// <summary>
        /// 表格外面套着的分组盒层数(成员分组盒 + 表格自己的盒), 同样只在"按层估"的路径上用:
        /// 量不到数据行宽时按层扣掉盒子内缩。多扣一点只是表格窄几像素(右侧留白),
        /// 少扣则数据行比滚动区宽 → 横向滚动条。
        ///
        /// 只数**数据行外面**的层: 表头行虽然是带框横排, 但左右 padding 已归零
        /// (见旧实现里的 HeaderRowStyle / Driver.BeginHorizontal), 不占横向位置 ——
        /// 再给它留一层就是把边距多估一份, 表格会窄出一条看不见用途的空白。
        /// </summary>
        private const int TableBoxLevels = 2;

        /// <summary>表格至少留几行数据位。空表只留 1 行时, 表头 + 横向滚动条一占就只剩一条缝, 什么也看不清。</summary>
        private const int MinTableVisibleRows = 3;

        /// <summary>单元格内独立按钮(撑满列宽)的上下文标记。</summary>
        private const float CellRowFullWidth = -1f;

        /// <summary>列间竖分隔线的宽度: 参考界面的表格是网格, 只有横线时不看列宽也能把三列读串。</summary>
        private const float TableColumnDividerWidth = 1f;

        /// <summary>
        /// 单元格内按钮组相邻按钮之间**布局占掉**的缝(像素):  横排的标准行距。
        /// 定宽按钮的样式边距已在后端归零(见旧实现里的 CompactButtonStyle), 这里只需让出标准行距 ——
        /// 少算它, 6 个键一排会比列宽多出 10px, 把整行顶出横向滚动条。
        /// </summary>
        private const float CellButtonGap = 2f;

        private readonly DrawContext m_Ctx;
        private readonly WidgetBuilder m_Builder;
        private readonly ValueAccessor m_Values;
        private readonly ILabelFormatter m_Labels;
        private readonly TableSchema m_Schema;
        private readonly VisibilityEvaluator m_Visibility;
        private readonly CollectionMutator m_Mutator;
        private readonly ListPager m_Pager;
        private readonly IDiagnosticSink m_Sink;
        private readonly IMemberDrawHost m_Drawer;

        public TableRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values, ILabelFormatter labels,
                             TableSchema schema, VisibilityEvaluator visibility, CollectionMutator editor,
                             ListPager pager, IDiagnosticSink sink, IMemberDrawHost drawer)
        {
            m_Ctx        = ctx;
            m_Builder    = builder;
            m_Values     = values;
            m_Labels     = labels;
            m_Schema     = schema;
            m_Visibility = visibility;
            m_Mutator     = editor;
            m_Pager      = pager;
            m_Sink       = sink;
            m_Drawer     = drawer;
        }

        /// <summary>取某个元素类型的列定义。</summary>
        public List<TableColumn> GetColumns(Type elementType)
        {
            return m_Schema.Get(elementType);
        }

        /// <summary>
        /// 表格行尾的删除按钮: 宽度对齐删除列, 无底纹字形(与列表条目的 × 同一枚字形)。
        ///
        /// 定宽仍取删除列宽: 表头那行用 <see cref="TableRemoveColumnWidth"/> 留了空格, 二者必须同宽。
        /// </summary>
        private static readonly DrawButtonSpec TableRemoveButtonSpec =
            DrawButtonSpec.Of(width: TableRemoveColumnWidth, expandWidth: false).WithFrameless();


        /// <summary>
        /// 表格绘制。列来自元素类型自身的成员(已按 PropertyOrder 排序), 列宽取成员的 [TableColumnWidth];
        /// 表头按顶层分组属性合并成一条带(分组表头), 下面再画一行列名。
        /// </summary>
        public void RenderTable(object target, CachedMember member, IList list, EditorWindow host, int depth,
                                 TableListAttribute table, List<TableColumn> cachedColumns)
        {
            // 列宽是"这张表这次绘制"的结果: 拖拽与均分都会改写它。列定义本身按元素类型缓存、
            // 所有同类型表格共用一份, 所以这里拿一份浅拷贝来画, 缓存始终保留声明值。
            var columns = CloneColumns(cachedColumns, member);

            var readOnly = table.IsReadOnly || member.GetAttr<ReadOnlyAttribute>() != null;
            var showIndex = table.ShowIndexLabels;
            var showRemove = readOnly == false && table.HideToolbar == false;
            var expanded = table.AlwaysExpanded || m_Ctx.Session.GetToggle(target, member.RawName, true);

            m_Builder.BeginVertical(true);

            if (table.AlwaysExpanded)
            {
                // 观感要求: 左侧标题, 右侧 "N items" 计数。
                // 计数行与 [HideLabel] 无关: 标签只藏名字、不藏计数。
                m_Builder.BeginHorizontal();
                m_Builder.Label(m_Labels.LabelOrEmpty(member), RDTheme.CountedHeader);
                m_Builder.FlexibleSpace();
                m_Builder.Label(m_Labels.CountedText(list.Count), DrawTextSpec.Of(mini: true));
                m_Builder.EndHorizontal();
            }
            else
            {
                m_Builder.BeginHorizontal();
                // 折叠头的读/画/比较/写回见 CollapseState(与列表、字典共用同一份实现)。
                expanded = CollapseState.Build(m_Ctx, target, member.RawName, true,
                                               m_Labels.CountedLabel(member, list.Count), m_Builder);
                m_Builder.FlexibleSpace();
                if (showRemove)
                {
                    m_Builder.Button("+", RDTheme.AddButton, () =>
                    {
                        m_Mutator.AddElement(target, member, list, member.GetAttr<ListDrawerSettingsAttribute>());
                        m_Ctx.Driver.RequestRepaint();
                    });
                }

                m_Builder.EndHorizontal();
            }

            if (expanded)
            {
                // 表格分页: 仅当显式 ShowPaging(外层 TableList 默认用 DrawScrollView 滚动, 不分页)
                var pageEnabled = table.ShowPaging;
                var pageSize    = table.NumberOfItemsPerPage > 0 ? table.NumberOfItemsPerPage : 15;

                // 页数/页索引/本页区间: 与列表共用同一份计算(边界不变式见 PagingRange)。
                var page      = PagingRange.Compute(list.Count, pageSize, pageEnabled,
                                                    m_Ctx.Session.GetPage(target, member.RawName));
                var pageCount = page.PageCount;
                var pageIndex = page.PageIndex;
                var start     = page.Start;
                var end       = page.End;

                var useScroll = table.DrawScrollView && !pageEnabled;
                var scroll = m_Ctx.Session.GetScroll(target, member.RawName);

                // 可用宽: 排版用会话里缓存的"上一帧量到的值"(见 MeasureTableRowWidth)。
                //
                // 表格自己开滚动区时, 量的是**滚动区视口宽**(padding 与竖向滚动条已扣掉),
                // 列宽合计照着它排就是严丝合缝; 没开滚动区(分页表格)时没有这一处可量,
                // 仍按根节点量到的宽估。
                //
                // 时机: 滚动区矩形只有 Repaint 事件是结算后的值(Layout 事件里是 0),
                // 所以量到 → 缓存 → 请求重绘, 排版用上一帧的值。
                // 矩形是只读的: 不往滚动区里加探针控件, 否则滚动区自己的布局(含竖向滚动条)会被打乱。
                var root   = m_Ctx.RootWidth;
                var cached = m_Ctx.Session.GetContentWidth(target, member.RawName);

                // 没开自己的滚动区时量不到"数据行的宽": 退回根节点量到的宽(宿主报的视口宽 /
                // 面板内容宽), 表格外面那些盒子的内缩由 DistributeTableColumns 按层估。
                if (useScroll == false && root > 1f && Mathf.Abs(root - cached) > 0.5f)
                {
                    m_Ctx.Session.SetContentWidth(target, member.RawName, ClampTableWidth(root));
                    m_Ctx.Driver.RequestRepaint();
                }

                // 首帧(还没量到)先按根宽兜一个, 别让表格从兜底宽起排
                var contentWidth = cached > 1f ? cached : root > 1f ? ClampTableWidth(root) : DefaultTableContentWidth;

                // 硬上界: 表格可用宽不超过窗口宽。正常情况下根宽 = 面板视口宽(≤ 窗口宽),
                // 这一步不生效; 万一还有哪处定宽子控件把面板内容宽顶大了, 这里把越界值夹回窗口宽 ——
                // 表格最多退化成"一屏宽 + 一条横向滚动条", 不会再变成几千像素的巨表。
                contentWidth = ClampTableWidth(contentWidth);

                // cached 有值才说明"数据行的宽"真量到过(见 MeasureTableRowWidth);
                // 后端量不了、或还没量到, 仍走"按层估内缩"的老路。
                var rowWidthMeasured = useScroll && cached > 1f;

                // 表头固定: 画在滚动区外, 数据行滚动时表头不动
                DistributeTableColumns(target, columns, showIndex, showRemove, contentWidth,
                                       rowWidthMeasured, useScroll);
                DrawTableHeader(target, columns, showIndex, showRemove);
                m_Sink.TableColumns(ColumnWidths(columns), ColumnLabels(columns), contentWidth,
                                   root, m_Ctx.Driver.ViewWidth, m_Ctx.Session.ViewportWidth);

                if (useScroll)
                {
                    // 高度有界, 避免与外层窗口滚动视图互相抢空间; 滚动区只含数据行(headerRows=0)。
                    // 先给一个"按行数估"的预估高度(见 TableFillHeight 的兜底分支); 真正的"填满外框剩余高度"
                    // 放到布局完成后的几何回调里直接改 style.maxHeight(只触发廉价回流, 不再整树重建)。
                    // 旧逻辑用"量到→缓存→整树重绘"在每帧反复重排, 堆叠多表时串行传导成十几帧 —— 打开/切页签抖动的根因。
                    var maxHeight = TableFillHeight(target, member, list.Count, 0, table.NumberOfItemsPerPage);

                    // 视口宽仍按老办法量(滚动区已存在, 量完下一帧均分列宽); 填高改走几何回调。
                    var scrollNode = m_Builder.BeginScroll(ref scroll, maxHeight);
                    scrollNode.AfterBegin = _ =>
                    {
                        // 数据行就在这个滚动区里: 量一次视口宽, 同时喂给列宽分发(下一帧用)与诊断。
                        var viewport = m_Ctx.Driver.MeasureScrollViewportWidth();
                        MeasureTableRowWidth(target, member, viewport);
                        m_Sink.SetTableScrollWidth(viewport);

                        // 布局完成后, 用当前滚动区的实际位置直接填高, 不再触发整树重建(消除抖动)。
                        var sv = m_Ctx.Driver.ActiveScroll;
                        if (sv != null) RegisterTableFill(sv);
                    };
                    scrollNode.End = value => m_Ctx.Session.SetScroll(target, member.RawName, value);
                }

                // 注意: IsReadOnly 只隐藏增删按钮(见 showRemove), 不禁用整行 ——
                // 约定: 单元格内容(嵌套列表折叠/按钮/同步)保持可交互
                DrawTableRows(target, member, list, columns, host, depth, showIndex, showRemove, start, end);

                if (useScroll) m_Builder.EndScroll();

                // 诊断: 建树阶段量不到"盒子内宽", 用本次排版的表格宽代替(它才是决定是否溢出的那个值)
                m_Sink.TableGeometry(contentWidth);

                if (pageEnabled && pageCount > 1)
                {
                    m_Builder.BeginHorizontal();
                    m_Pager.Build(m_Builder, target, member.RawName, pageIndex, pageCount);
                    m_Builder.EndHorizontal();
                }
            }

            m_Builder.EndVertical();
        }

        /// <summary>
        /// 表格可用宽的硬上界: 永远不超过窗口宽。
        ///
        /// 正常情况下根宽就是面板视口宽(≤ 窗口宽), 这道闸不生效; 它只是兜底 ——
        /// 万一某处的定宽子控件把面板内容宽顶大, 这里把越界值夹回窗口宽。
        /// </summary>
        private float ClampTableWidth(float width)
        {
            // 宿主报的视口宽是"与内容无关"的值(见 HostState.SetViewportWidth), 比窗口宽更可信:
            // 会话缓存里可能还留着"内容把面板撑宽"那几帧的旧值, 只用窗口宽夹不住它 ——
            // 表格会一直比面板宽出一条滚动条, 表头与计数被窗口右缘裁掉。
            var viewport = m_Ctx.Session.ViewportWidth;
            if (viewport > 1f) width = Mathf.Min(width, viewport);

            var view = m_Ctx.Driver.ViewWidth;
            return view > 1f ? Mathf.Min(width, view) : width;
        }

        /// <summary>表格里可见的数据行数: 空表也留 <see cref="MinTableVisibleRows"/> 行, 并封顶到 NumberOfItemsPerPage。</summary>
        internal static int TableVisibleRows(int itemCount, int itemsPerPage)
        {
            return Mathf.Clamp(itemCount, MinTableVisibleRows, Mathf.Max(MinTableVisibleRows, itemsPerPage));
        }

        /// <summary>
        /// 表格开 DrawScrollView 时的滚动区高度上限。
        ///
        /// 优先**填满外框剩下的高度**: 数据区底边贴着表格盒的下沿, 窗口拉高(视口变高)时跟着拉长。
        /// 算法: 可用下沿 − 滚动区所在的纵坐标(现在由几何回调在布局完成后直接量、直接填, 见 <see cref="ApplyTableFill"/>)。
        ///
        /// 下沿有两个来源: 由宿主经 HostState 报(它拿不到视口矩形), 或
        /// 后端自己就在 ScrollView 里、直接量得到(见 <see cref="Driver.AvailableBottom"/>)。
        /// 两边都拿不到(检视器这类没有"内容区下沿"概念的宿主)或量不到位置时,
        /// 退回 <see cref="TableScrollHeight"/> 的"按行数估"。
        ///
        /// BeginScroll 是 MaxHeight 语义: 内容比它短时自动收缩, 所以"填满"不会给空表撑出空白;
        /// 窗口拉小时也不会把数据区压成一条缝(留 <see cref="MinTableVisibleRows"/> 行)。
        /// </summary>
        private float TableFillHeight(object target, CachedMember member, int itemCount, int headerRows,
                                      int itemsPerPage)
        {
            var lineHeight = m_Ctx.Driver.LineHeight;
            var scrollbar  = m_Ctx.Driver.ScrollbarHeight;
            var byRows     = TableScrollHeight(itemCount, headerRows, itemsPerPage, lineHeight, scrollbar);

            // 宿主报的优先(), 报不了就问后端( 直接量 ScrollView 视口)
            var bottom = m_Ctx.Session.ContentBottom;
            if (bottom <= 1f) bottom = m_Ctx.Driver.AvailableBottom;
            if (bottom <= 1f) return byRows;

            // 初值兜底: 下面取上一帧缓存的位置; 真正"填满外框剩余高度"已改由几何回调 ApplyTableFill 在布局完成后处理。
            var top = m_Ctx.Session.GetContentTop(target, member.RawName);
            if (top <= 1f) return byRows;

            // 扣掉表格盒自己的下沿留白(盒 padding): 不扣就差这几像素, 数据区顶出视口,
            // 外框被撑出竖向滚动条
            var remaining = bottom - top - Mathf.CeilToInt(m_Ctx.Driver.BoxInsets);
            var minimum   = lineHeight * MinTableVisibleRows + scrollbar;

            return Mathf.Max(remaining, minimum);
        }

        /// <summary>
        /// 把"表格滚动区填满外框剩余高度"放到布局完成后的几何回调里做: 直接改滚动区的
        /// <c>style.maxHeight</c>(只触发廉价回流, 不重建整棵树), 从而消除"量到→缓存→整树重绘"
        /// 在每帧反复重排、堆叠多表时串行传导成十几帧的打开/切页签抖动。
        ///
        /// 与旧探针同一套几何关系(下沿 − 当前位置), 但用屏幕坐标的 worldBound 折算到外层滚动区的内容坐标,
        /// 与宿主报的下沿(ContentBottom / AvailableBottom)同一坐标系, 滚动安全。
        /// </summary>
        private static readonly ConditionalWeakTable<VisualElement, EventCallback<GeometryChangedEvent>> SFillHandlers = new();
        private static readonly ConditionalWeakTable<VisualElement, FillIter> SFillIters = new();

        private sealed class FillIter
        {
            public int Value;
        }

        private void RegisterTableFill(VisualElement scrollView)
        {
            // 滚动区 VE 是池化复用的: 复用时先卸掉上一轮挂的回调, 再挂新的, 保证每轮只有一个存活。
            if (SFillHandlers.TryGetValue(scrollView, out var old))
            {
                scrollView.UnregisterCallback(old);
                SFillHandlers.Remove(scrollView);
            }

            EventCallback<GeometryChangedEvent> handler = _ => ApplyTableFill(scrollView);
            SFillHandlers.Add(scrollView, handler);
            scrollView.RegisterCallback(handler);
        }

        private void ApplyTableFill(VisualElement scrollView)
        {
            var driver = m_Ctx.Driver;

            // 屏幕坐标 top 折算到外层滚动区的内容坐标(加 scrollOffset, 因为屏幕 = 内容 − scrollOffset),
            // 与下沿(宿主报的 ContentBottom, 也是内容坐标的固定下沿)同系 —— 这样未滚动 / 已滚动都正确(原探针只在未滚动时一致)。
            var outer = FindOuterScroll(scrollView);
            if (outer == null) return;
            var topContent = scrollView.worldBound.y + outer.scrollOffset.y;
            if (topContent <= 1f) return;

            var bottom = m_Ctx.Session.ContentBottom;   // 固定内容下沿(宿主报)
            if (bottom <= 1f) bottom = driver.AvailableBottom;
            if (bottom <= 1f) return;

            var maxH = bottom - topContent - driver.BoxInsets;
            if (maxH <= 1f) return;

            var minimum = driver.LineHeight * MinTableVisibleRows + driver.ScrollbarHeight;
            maxH = Mathf.Max(maxH, minimum);

            var cur = scrollView.style.maxHeight.value.value;   // StyleLength → Length → float
            if (Mathf.Abs(maxH - cur) <= 1f)   // 1px 滞后, 吸收亚像素, 避免来回摆
            {
                SFillIters.Remove(scrollView);
                return;
            }

            // 安全闸: 同一 VE 连续改太多次(>100)就停, 防几何环把 CPU 打满(正常收敛远小于此)。
            if (SFillIters.TryGetValue(scrollView, out var iter))
            {
                if (iter.Value > 100) return;
                iter.Value++;
            }
            else
            {
                SFillIters.Add(scrollView, new FillIter { Value = 1 });
            }

            scrollView.style.maxHeight = maxH;
        }

        private static ScrollView FindOuterScroll(VisualElement element)
        {
            for (var e = element.parent; e != null; e = e.parent)
                if (e is ScrollView s) return s;

            return null;
        }

        /// <summary>
        /// 量一次"数据行真正能用的宽"(滚动区视口宽)并缓存, 变了就请求重绘(下一帧按它均分列宽)。
        ///
        /// 为什么量滚动区: 只有这一处知道"表格盒/分组盒的 padding + 竖向滚动条"实际吃掉多少。
        /// 在盒子外面量(根节点)再按层数估算, 估多几像素就是表格右侧那条空白, 估少又撑出横向滚动条。
        ///
        /// 为什么不会"越撑越宽": 滚动区的矩形由外框给定, 与里面画什么无关(表格再宽也顶不大它);
        /// 再加上 <see cref="ClampTableWidth"/> 与根宽这道上限, 不存在"内容撑宽 → 再排更宽"的反馈环。
        ///
        /// 时机同理: 只有 Repaint 事件拿得到结算后的矩形,
        /// 所以量到 → 缓存 → 请求重绘, 排版用上一帧的值。
        /// </summary>
        private void MeasureTableRowWidth(object target, CachedMember member, float width)
        {
            // 后端量不了滚动区时(或 Layout pass 还没结算)不量: 表格退回按盒子层数估内缩(见 DistributeTableColumns)
            if (width <= 1f) return;

            var root = m_Ctx.RootWidth;
            if (root > 1f) width = Mathf.Min(width, root);   // 兜一层: 视口宽不该超过面板宽

            width = ClampTableWidth(width);
            if (Mathf.Abs(width - m_Ctx.Session.GetContentWidth(target, member.RawName)) <= 0.5f) return;

            m_Ctx.Session.SetContentWidth(target, member.RawName, width);
            m_Ctx.Driver.RequestRepaint();
        }

        /// <summary>
        /// 按行数估的滚动区高度, 逐项算:
        /// (数据行 + 表头行) × 行高 + 横向滚动条 + 表头分隔线。
        /// 每一行都按 singleLineHeight 计, 而表头/单元格实际用的是更小的 mini 样式 —— 属于上界,
        /// 宁可多留几像素。宿主没报可用下沿时用它兜底。
        /// </summary>
        internal static float TableScrollHeight(int itemCount, int headerRows, int itemsPerPage,
                                               float lineHeight, float scrollbarHeight)
        {
            // 上限按"每页行数"计, 与实际数据行数无关: BeginScroll 是 MaxHeight 语义,
            // 内容少时自动收缩到内容高度, 内容多时封顶出滚动条。
            return lineHeight * (Mathf.Max(itemsPerPage, MinTableVisibleRows) + headerRows)
                   + scrollbarHeight + RDTheme.TableHeaderLineThickness;
        }

        /// <summary>
        /// 无显式 [TableColumnWidth] 的列按可用宽度均分。
        /// 每一行横排都独立分配弹性空间, "弹性列"会导致行与行、表头与单元格
        /// 各排各的(列对不齐), 内容最小宽一挤还撑出横向滚动条 —— 跨行对齐必须用固定宽,
        /// 所以"均分撑满"要在这里显式算成像素。
        ///
        /// rowWidthMeasured 表示 contentWidth 是否就是"数据行的宽"(见 <see cref="MeasureTableRowWidth"/>):
        /// 是则表格盒 padding / 竖向滚动条已经在里面, 一项都不必再扣。
        /// hasScroll 表示表格自己开了滚动区: 只有它才带竖向滚动条与 scrollView padding。
        /// </summary>
        private void DistributeTableColumns(object target, List<TableColumn> columns,
                                            bool showIndex, bool showRemove, float contentWidth,
                                            bool rowWidthMeasured, bool hasScroll)
        {
            // 用户拖过的列宽优先于声明值: 拖完它就是这一列的定宽(下一帧起不再参与均分)。
            // 按列序记键: 列定义按元素类型缓存, 同一份定义被所有同类型的表格共用,
            // "第几列"才是这张表里稳定的身份(分组的 GroupKey 对独立列永远为空, 会互相串)。
            var dragged = DraggedBuffer(columns.Count);
            for (var i = 0; i < columns.Count; i++)
            {
                dragged[i] = m_Ctx.Session.GetColumnWidth(target, columns[i].SessionKey);
                if (dragged[i] > 0f) columns[i].Width = Mathf.RoundToInt(dragged[i]);
            }

            var flexCount  = 0;
            var fixedWidth = 0f;
            for (var i = 0; i < columns.Count; i++)
            {
                // 定宽 = 声明过的 + 用户拖过的; 两者之外的才是"按剩余宽均分"
                if (columns[i].WidthIsExplicit || dragged[i] > 0f) fixedWidth += columns[i].Width;
                else flexCount++;
            }

            if (flexCount == 0) return;

            // 索引列/删除列/列间竖线是所有列宽之外真正要占位的东西, 必须从可用宽里扣掉:
            // 定宽列(声明 + 拖拽)也一样先扣, 否则每个弹性列仍按"整条可用宽"平分 →
            // 各列宽之和必然超过可用宽, 表格一直被撑出横向滚动条、右侧的列被截掉。
            var reserved = (showIndex ? TableIndexColumnWidth : 0)
                           + (showRemove ? TableRemoveColumnWidth : 0)
                           + (columns.Count - 1) * (int)TableColumnDividerWidth;

            // 只有"可用宽不是数据行的宽"时, 还要把外面那些容器的内缩估出来:
            // 表格外那几层盒子的左右 padding, 以及(表格自己开滚动区时的)滚动条/滚动区 padding。
            // 估多了就是表格右侧那条空白, 所以只要有实测值(见 rowWidthMeasured)就一项都不估。
            if (rowWidthMeasured == false)
            {
                reserved += TableHorizontalMargin
                            + Mathf.CeilToInt(m_Ctx.Driver.BoxInsets * TableBoxLevels);

                // 分页表格(没开自己的滚动区)直接铺在表格盒里, 没有竖向滚动条 ——
                // 这一条再扣就是白扣, 各列平白窄一条滚动条的宽
                if (hasScroll) reserved += Mathf.CeilToInt(m_Ctx.Driver.ScrollViewInsets);
            }

            var shared = Mathf.Max((int)(contentWidth - fixedWidth) - reserved, 0);
            var flex   = Mathf.Max(shared / flexCount, MinTableColumnWidth);

            // 均分除不尽的零头补给前几列: 否则列宽合计比可用宽少一两像素, 右侧又是一条细缝。
            // 窄窗口下 flex 被下限顶住(shared < flex × flexCount), 这时没有余数可分。
            var remainder = Mathf.Clamp(shared - flex * flexCount, 0, flexCount);

            // 列宽只在"输入变了"时打一次(而非每帧): 既能验证均分是否生效, 又不刷屏
            m_Sink.TableDistribution(contentWidth, reserved, flex, flexCount,
                                     ColumnWidths(columns), ColumnExplicitWidths(columns));

            for (var i = 0; i < columns.Count; i++)
            {
                if (columns[i].WidthIsExplicit || dragged[i] > 0f) continue;
                columns[i].Width = flex + (remainder-- > 0 ? 1 : 0);
            }
        }

        /// <summary>用户拖拽过的列宽在会话里的键: 成员名 + 列序(见 <see cref="DistributeTableColumns"/>)。</summary>
        private static string ColumnWidthKey(CachedMember member, int index)
        {
            return member.RawName + "#" + index;
        }

        /// <summary>
        /// 复制一份列定义供本次绘制使用(见 <see cref="RenderTable"/> 的说明)。
        ///
        /// 复制而不直接改缓存的原因不只是"同类型的两张表": 表格单元格里还能嵌另一张同类型的表
        /// (嵌套列表), 内层绘制完会把自己的列宽留给外层 —— 外层接着画数据行时列宽就变了。
        /// 成员清单是只读的, 可以共享; 每列顺带把会话键算好(拖拽分发与表头手柄都要用)。
        /// </summary>
        private static List<TableColumn> CloneColumns(List<TableColumn> source, CachedMember member)
        {
            var columns = new List<TableColumn>(source.Count);
            for (var i = 0; i < source.Count; i++)
            {
                var origin = source[i];
                columns.Add(new TableColumn
                {
                    Members         = origin.Members,
                    GroupKey        = origin.GroupKey,
                    GroupName       = origin.GroupName,
                    Label           = origin.Label,
                    Width           = origin.Width,
                    WidthIsExplicit = origin.WidthIsExplicit,
                    Resizable       = origin.Resizable,
                    SessionKey      = ColumnWidthKey(member, i),
                });
            }

            return columns;
        }

        /// <summary>
        /// "用户拖拽列宽"的临时缓冲: 借自本次绘制的上下文(<see cref="DrawContext.ColumnWidthScratch"/>),
        /// 一张面板里的多张表共用一块, 用完即弃。
        /// </summary>
        private float[] DraggedBuffer(int count)
        {
            return m_Ctx.ColumnWidthScratch(count);
        }

        private void DrawTableHeader(object target, List<TableColumn> columns,
                                     bool showIndex, bool showRemove)
        {
            // 单行表头: 合并列显示分组名(信息/我依赖/依赖我), 独立列显示成员名。
            // 用带框横排铺一层更亮的表头底色, 与数据行区分, 否则列名像第一行数据。
            m_Builder.BeginHorizontal(true);
            if (showIndex)
            {
                m_Builder.Label("#", DrawTextSpec.Of(DrawAlign.Center, width: TableIndexColumnWidth,
                                                     height: RDTheme.PanelHeaderHeight));
            }

            for (var i = 0; i < columns.Count; i++)
            {
                // 与数据行同样在列之间画竖线: 表头与列的边界对不上时, 网格线会错位。
                // 这里必须传定高: 表头行在  里是横排, ExpandHeight 会顶出整块空白
                if (i > 0) DrawHeaderColumnBoundary(target, columns, i);

                // 定宽盒: 列名在**列内居中**, 宽度与数据行的单元格一致 → 表头与列严格对齐。
                m_Builder.BeginVertical(false, columns[i].Width);
                m_Builder.Label(columns[i].Label, RDTheme.TableHeaderText);
                m_Builder.EndVertical();
            }

            if (showRemove) m_Builder.Space(TableRemoveColumnWidth);
            m_Builder.EndHorizontal();

            // 表头行自带 1px 下边线(见旧实现里的 HeaderRowStyle): 这里再画分隔线会变成
            // "线 + 2px 空 + 线"的双线, 与参考的一道横线不符
        }

        private void DrawTableRows(object target, CachedMember member, IList list, List<TableColumn> columns,
                                   EditorWindow host, int depth, bool showIndex, bool showRemove,
                                   int pageStart = -1, int pageEnd = -1)
        {
            if (list.Count == 0)
            {
                m_Builder.Label("(空)", DrawTextSpec.Of(mini: true));
                return;
            }

            var from = pageStart >= 0 ? pageStart : 0;
            var to   = pageEnd   >= 0 ? pageEnd   : list.Count;
            for (var i = from; i < to; i++)
            {
                // 删除动作在渲染后执行, 必须按下标捕获(for 变量是共享的)
                var index   = i;
                var element = list[index];

                m_Builder.BeginHorizontal();
                // 注意: 这里**不能**插探针 —— 数据行是横向组, ExpandWidth 的探针会抢走剩余宽度,
                // 把后面的单元格整体挤走(行宽改用"各单元格宽之和"推算)。
                m_Sink.BeginTableRow();
                if (showIndex)
                {
                    m_Builder.Label(IndexText.Of(index), DrawTextSpec.Of(mini: true, width: TableIndexColumnWidth));
                }

                if (element == null)
                {
                    // 与选择器的 "None" 对齐: 表格里的 null 行走的是行尾那枚 ×, 这里只负责把它标出来
                    m_Builder.Label("None", DrawTextSpec.Of(mini: true));
                }
                else
                {
                    for (var c = 0; c < columns.Count; c++)
                    {
                        if (c > 0) DrawColumnDivider();
                        DrawTableCell(element, columns[c], host, depth);
                    }
                }

                if (showRemove)
                {
                    m_Builder.Button("×", TableRemoveButtonSpec, () =>
                    {
                        m_Mutator.RemoveAt(target, list, index);
                        m_Ctx.Driver.RequestRepaint();
                    });
                }

                m_Builder.EndHorizontal();
                DrawHorizontalLine();
            }
        }

        private void DrawTableCell(object element, TableColumn column, EditorWindow host, int depth)
        {
            // 列宽已是定值(显式或均分结果):  每行独立分配弹性空间, 固定宽才能跨行对齐
            m_Builder.BeginVertical(false, column.Width);
            // 诊断: 单元格的宽就是列宽(建树阶段量不到"实际宽", 列宽才是本帧的排版输入)
            m_Sink.AddTableCellWidth(column.Width);

            // 单元格内(含嵌套内容, 如依赖列表的条目)里的按钮按约定撑满所在格宽。
            // 用作用域而不是手工置位/还原: 嵌套绘制时进出顺序由栈保证。
            using (m_Ctx.EnterTableCell())
            {
                // 合并列: 同一分组的成员在单元格内竖向堆叠;
                // 其中连续的同 [ButtonGroup] 按钮成员横排成一行, 平分列宽
                var i = 0;
                while (i < column.Members.Count)
                {
                    var member = column.Members[i];
                    var buttonGroup = member.Kind == MemberKind.MethodButton ? member.GetAttr<ButtonGroupAttribute>() : null;

                    if (buttonGroup == null)
                    {
                        // 独立按钮: 撑满所在列宽(如"重置")
                        DrawTableCellMember(element, member, host, depth, CellRowFullWidth);
                        i++;
                        continue;
                    }

                    // 数一下这一行有几个同组按钮, 把列宽平分成几份(6 个更新键各占 1/6)
                    // 只统计"可见"的成员: 隐藏成员既不占位置也不占份额。
                    var groupEnd = i;
                    var count    = 0;
                    for (var j = i; j < column.Members.Count; j++)
                    {
                        var next = column.Members[j];
                        var nextGroup = next.Kind == MemberKind.MethodButton ? next.GetAttr<ButtonGroupAttribute>() : null;
                        if (nextGroup == null || nextGroup.GroupID != buttonGroup.GroupID) break;
                        groupEnd = j + 1;
                        if (m_Visibility.IsVisible(next, element, m_Ctx.IsPlaying)) count++;
                    }

                    var share = count > 0
                        ? Mathf.Max((column.Width - (count - 1) * CellButtonGap) / count, 1f)
                        : 0f;

                    m_Builder.BeginHorizontal();
                    for (var k = i; k < groupEnd; k++)
                    {
                        var grouped = column.Members[k];
                        // 组内隐藏成员直接跳过: 留空位会在横向组里吃掉一份宽度
                        if (m_Visibility.IsVisible(grouped, element, m_Ctx.IsPlaying) == false) continue;
                        DrawTableCellMember(element, grouped, host, depth, share);
                    }

                    m_Builder.EndHorizontal();
                    i = groupEnd;
                }
            }

            m_Builder.EndVertical();
        }

        /// <summary>绘制单元格里的单个成员; 条件不满足时留一行空位。</summary>
        /// <param name="cellShare">单元格按钮宽度上下文, 见 <see cref="DrawContext.CellButtonShare"/>。</param>
        private void DrawTableCellMember(object element, CachedMember member, EditorWindow host, int depth, float cellShare)
        {
            if (m_Visibility.IsVisible(member, element, m_Ctx.IsPlaying) == false)
            {
                m_Builder.Label("", DrawTextSpec.Of(height: m_Ctx.Driver.LineHeight));
                return;
            }

            if (member.Kind == MemberKind.MethodButton)
            {
                // 单元格按钮上下文: 只圈住这一次按钮绘制, 嵌套内容(列表项里的按钮等)不继承
                using (m_Ctx.EnterTableCell(cellShare))
                {
                    m_Drawer.DrawMember(element, member, host, depth);
                }
            }
            else
            {
                m_Drawer.DrawValueMember(element, member, host, depth, true);
            }
        }

        private void DrawHorizontalLine()
        {
            m_Builder.Separator(RDTheme.TableHeaderLineThickness, RDTheme.TableHeaderLineColor);
        }

        /// <summary>列间竖线: 后端不提供这个可选能力时就没有网格线, 列宽照旧。</summary>
        /// <param name="height">表头传定高(行高已知); 数据行传 0, 由后端撑满行高。</param>
        private void DrawColumnDivider(float height = 0f)
        {
            m_Builder.ColumnSeparator(height);
        }

        /// <summary>
        /// 表头行的列边界: 兼作列宽拖拽手柄(<see cref="Driver.ColumnResizer"/>)。
        ///
        /// 边界归**左边那一列**: 拖右边界改左列是表格的通行做法(拖哪条线, 改哪条线左边的列)。
        /// 该列不可拖时退化成一模一样的普通分隔线 —— 视觉上两种边界没有区别,
        /// 只有鼠标移上去时的手型光标才透出"这条能拖"。
        ///
        /// 拖拽增量要等渲染后才有, 因此本帧仍按旧列宽排(表头与数据行一致), 下一帧按新列宽重排。
        /// </summary>
        private void DrawHeaderColumnBoundary(object target, List<TableColumn> columns, int index)
        {
            var left = columns[index - 1];
            if (left.Resizable == false)
            {
                DrawColumnDivider(RDTheme.PanelHeaderHeight);
                return;
            }

            m_Builder.ColumnResizer(RDTheme.PanelHeaderHeight, delta =>
            {
                if (Mathf.Abs(delta) < 0.01f) return;

                // 上界: 一列再宽也不该超过整张表的可用宽(否则拖一下就把表格顶出横向滚动条)
                var available = m_Ctx.Driver.ContentWidth;
                var limit     = available > 1f ? Mathf.Max(available, MinDragColumnWidth) : float.MaxValue;

                left.Width = Mathf.RoundToInt(Mathf.Clamp(left.Width + delta, MinDragColumnWidth, limit));
                m_Ctx.Session.SetColumnWidth(target, left.SessionKey, left.Width);
                m_Ctx.Driver.RequestRepaint();
            });
        }

        /// <summary>
        /// 把内部列定义投影成诊断通道用的原始数组。
        ///
        /// 诊断通道(<see cref="IDiagnosticSink"/>)是公开契约, 不该暴露 <see cref="TableColumn"/>,
        /// 所以在这里做一次投影 —— 每帧只在诊断输出真要发出去时才调, 不是热路径。
        /// </summary>
        private static float[] ColumnWidths(List<TableColumn> columns)
        {
            var widths = new float[columns.Count];
            for (var i = 0; i < columns.Count; i++) widths[i] = columns[i].Width;
            return widths;
        }

        private static bool[] ColumnExplicitWidths(List<TableColumn> columns)
        {
            var flags = new bool[columns.Count];
            for (var i = 0; i < columns.Count; i++) flags[i] = columns[i].WidthIsExplicit;
            return flags;
        }

        private static string[] ColumnLabels(List<TableColumn> columns)
        {
            var labels = new string[columns.Count];
            for (var i = 0; i < columns.Count; i++) labels[i] = columns[i].Label;
            return labels;
        }

    }
}
