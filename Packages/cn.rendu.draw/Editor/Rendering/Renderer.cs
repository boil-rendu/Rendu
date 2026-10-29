using RD.Draw.Editor.Rendering;

namespace RD.Draw.Editor
{
    /// <summary>
    /// Widget 树 → Driver 原语 的解释器(具体类, 不再有 IRenderer / WidgetTreeRenderer 抽象层)。
    ///
    /// <see cref="Render"/> 按节点类型直接调用 <see cref="Driver"/> 上的原语, 控件节点交给
    /// 注入的 <see cref="IControlRenderer"/>。引擎保证每一分支 toolkit 都配平(Begin/End 成对),
    /// 这里只负责"翻译", 不持有任何状态。
    /// </summary>
    public sealed class Renderer
    {
        private readonly IControlRenderer m_ControlRenderer = new ControlRenderer();

        /// <summary>遍历整棵 Widget 树, 把每个节点翻译成 Driver 原语。</summary>
        internal void Render(Widget root, RenderContext ctx)
        {
            Walk(root, ctx);
        }

        private void Walk(Widget node, RenderContext ctx)
        {
            var drv = ctx.Ctx.Driver;

            switch (node)
            {
                case RootWidget:
                    WalkChildren(node, ctx);
                    return;

                case ColumnWidget col:
                    drv.BeginVertical(col.Boxed, col.Width, col.ExpandHeight, col.ExpandWidth);
                    WalkChildren(col, ctx);
                    drv.EndVertical();
                    return;

                case RowWidget row:
                    drv.BeginHorizontal(row.Boxed);
                    WalkChildren(row, ctx);
                    drv.EndHorizontal();
                    return;

                case IndentWidget indent:
                    drv.BeginIndent();
                    WalkChildren(indent, ctx);
                    drv.EndIndent();
                    return;

                case ScrollWidget scroll:
                    scroll.BeforeBegin?.Invoke(ctx);
                    drv.BeginScroll(ref scroll.Scroll, scroll.MaxHeight);
                    scroll.AfterBegin?.Invoke(ctx);
                    WalkChildren(scroll, ctx);
                    drv.EndScroll();
                    scroll.End?.Invoke(scroll.Scroll);
                    return;

                case SpaceWidget space:
                    drv.Space(space.Pixels);
                    return;

                case FlexibleSpaceWidget:
                    drv.FlexibleSpace();
                    return;

                case SeparatorWidget separator:
                    drv.Separator(separator.Thickness, separator.Color);
                    return;

                case LabelWidget label:
                    drv.Label(label.Text, label.Spec);
                    return;

                case KeyValueLabelWidget kv:
                    drv.KeyValueLabel(kv.Label, kv.Value);
                    return;

                case HelpBoxWidget help:
                    drv.HelpBox(help.Message, help.Kind);
                    return;

                case ProgressBarWidget progress:
                    drv.ProgressBar(progress.Value, progress.Title, progress.Height);
                    return;

                case PanelHeaderWidget header:
                    drv.PanelHeader(header.Text, header.Align, header.Bold);
                    return;

                case ColumnSeparatorWidget columnSep:
                    drv.ColumnSeparator(columnSep.Height);
                    return;

                case ColumnResizerWidget resizer:
                    resizer.Delta = drv.ColumnResizer(resizer.Height);
                    return;

                case CardWidget card:
                    drv.BeginCard();
                    WalkChildren(card, ctx);
                    drv.EndCard();
                    return;

                case ReorderWidget reorder:
                    drv.BeginReorder(reorder.ListId);
                    WalkChildren(reorder, ctx);
                    reorder.Result = drv.PollReorder(reorder.ListId);
                    return;

                case ReorderItemWidget item:
                    // 条目内容挂在本节点的子级里, 必须先走完: 最后一步 EndVertical 会把条目容器
                    // 关进 Tree.LastPopped, 紧接着的 EndReorderItem 就是拿它登记拖拽区间的。
                    // 少了这一句, 条目的整棵子树都不会落屏 —— 可拖拽列表的列表体永远是空的。
                    WalkChildren(item, ctx);
                    drv.EndReorderItem(item.ListId, item.Index);
                    return;

                case ReorderHandleWidget handle:
                    drv.ReorderHandle(handle.ListId, handle.Index, handle.Width, handle.Ghost);
                    return;

                case StyledScopeWidget scope:
                    if (scope.Color.HasValue) drv.PushColor(scope.Color.Value);
                    if (scope.Enabled.HasValue) drv.PushEnabled(scope.Enabled.Value);
                    if (scope.LabelWidth.HasValue) drv.PushLabelWidth(scope.LabelWidth.Value);
                    WalkChildren(scope, ctx);
                    if (scope.LabelWidth.HasValue) drv.PopLabelWidth();
                    if (scope.Enabled.HasValue) drv.PopEnabled();
                    if (scope.Color.HasValue) drv.PopColor();
                    return;

                case ControlWidget control:
                    m_ControlRenderer.Render(control, ctx);
                    return;

                default:
                    WalkChildren(node, ctx);
                    return;
            }
        }

        private void WalkChildren(Widget node, RenderContext ctx)
        {
            var children = node.ChildrenOrNull;
            if (children == null) return;
            for (var i = 0; i < children.Count; i++)
            {
                Walk(children[i], ctx);
            }
        }
    }
}
