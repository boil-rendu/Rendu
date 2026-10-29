using UnityEditor;
using UnityEngine;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 分组渲染: 遍历分组树, 画分组头/外框, 分发组内成员。
    ///
    /// 树的结构知识(项怎么混排、先后顺序)不在这里 —— 见 <see cref="GroupTraverse"/> 与
    /// <see cref="GroupEntry"/>。本类只做"画"。
    ///
    /// 它是"组里那些成员由谁画"的调度方: 成员本身交给 <see cref="IMemberDrawHost"/>,
    /// 因此本类不必知道成员怎么画, 只知道"这个成员现在该不该画"。
    /// </summary>
    internal sealed class GroupRenderer
    {
        private readonly DrawContext m_Ctx;
        private readonly WidgetBuilder m_Builder;
        private readonly VisibilityEvaluator m_Visibility;
        private readonly StateUpdater m_States;
        private readonly IMemberDrawHost m_Drawer;

        public GroupRenderer(DrawContext ctx, WidgetBuilder builder, VisibilityEvaluator visibility,
                             StateUpdater states, IMemberDrawHost drawer)
        {
            m_Ctx        = ctx;
            m_Builder    = builder;
            m_Visibility = visibility;
            m_States     = states;
            m_Drawer     = drawer;
        }

        /// <summary>渲染一个分组节点(根节点代表"未分组"区域)。</summary>
        public void Render(GroupNode node, object target, EditorWindow host, int depth, bool isRoot)
        {
            // 组自身的 VisibleIf 与 HideWhenChildrenAreInvisible(默认 true) 都可能导致整组不画。
            if (isRoot == false && m_Visibility.IsGroupVisible(node, target, m_Ctx.IsPlaying) == false)
            {
                return;
            }

            var bodyOpened = true;
            if (isRoot == false)
            {
                bodyOpened = BeginGroup(node, target);
            }

            if (bodyOpened)
            {
                var entries = GroupTraverse.Entries(node);
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    if (entry.IsGroup)
                    {
                        Render(entry.Group, target, host, depth + 1, false);
                        continue;
                    }

                    var m = entry.Member;
                    if (m == null) continue;
                    if (m_Visibility.IsVisible(m, target, m_Ctx.IsPlaying) == false) continue;

                    // 建树可能开了一半容器就抛: 记下挂载点, 异常时把栈弹回去, 免得后面的
                    // 成员全挂进那个半开的容器里(整块面板缩进错位)。
                    var mark = m_Builder.Mark();
                    try
                    {
                        m_States.Run(target, m, m_Ctx.Session);
                        m_Drawer.DrawMember(target, m, host, depth);
                    }
                    catch (System.Exception e) when (!(e is ExitGUIException))
                    {
                        m_Builder.Restore(mark);
                        DrawLog.Error($"[Draw] 绘制成员失败 {m.RawName}\n{e}");
                    }
                }
            }

            if (isRoot == false && bodyOpened)
            {
                EndGroup(node);
            }
        }

        /// <summary>
        /// 绘制分组头部/外框。返回 true 表示已开启一个需由 EndGroup 收尾的 GUI 作用域。
        /// target 用于取该宿主对象自己的分组展开状态。
        /// </summary>
        private bool BeginGroup(GroupNode node, object target)
        {
            var name = node.GroupName;

            switch (node.Kind)
            {
                case GroupKind.Box:
                    // 分节标题: 一条占满整行的表头横条(与内容盒分开), 而不是与正文同底色的一行字 ——
                    // 面板一长, 没有这条底色就分不清"信息""筛选包数据"从哪开始。
                    if (node.ShowLabel && string.IsNullOrEmpty(name) == false)
                    {
                        DrawPanelHeader(name, node.CenterLabel ? DrawAlign.Center : DrawAlign.Left);
                    }

                    m_Builder.BeginVertical(true, node.Width);
                    return true;

                case GroupKind.Vertical:
                    m_Builder.BeginVertical(false, node.Width);
                    // 约定: VerticalGroup 顶部画组名标头(加粗左对齐); "_" 开头的内部/默认组不画
                    if (string.IsNullOrEmpty(name) == false && name.StartsWith("_") == false)
                    {
                        m_Builder.Label(name, DrawTextSpec.Of(DrawAlign.Left, bold: true));
                    }

                    return true;

                case GroupKind.Horizontal:
                case GroupKind.ButtonGroup:
                    m_Builder.BeginHorizontal();
                    return true;

                case GroupKind.Title:
                    m_Builder.BeginVertical(true, node.Width);
                    if (string.IsNullOrEmpty(name) == false)
                    {
                        // 分组没有副标题, Split 退化为 Left
                        m_Builder.Label(name, DrawTextSpec.Of(DrawAlign.Left, node.BoldTitle));
                    }

                    if (node.HorizontalLine)
                    {
                        m_Builder.Separator(RDTheme.TitleGroupLineThickness, RDTheme.TitleGroupLineColor);
                    }

                    m_Builder.Space(4);
                    return true;

                case GroupKind.Foldout:
                {
                    m_Builder.BeginVertical(true, node.Width);
                    var expanded = GetGroupExpanded(target, node);
                    // 折叠态写回放进回调, 由 ChangeApplier 检测到变化后触发(见 CollapseState.Build)。
                    m_Builder.Foldout(expanded, string.IsNullOrEmpty(name) ? node.GroupId : name,
                                      next => SetGroupExpanded(target, node, next));
                    if (expanded == false)
                    {
                        m_Builder.EndVertical();
                        return false;
                    }

                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// 画一条分节表头。后端实现了面板装饰( <see cref="Driver.PanelHeader"/> )时是带底色的横条;
        /// 没实现时退回一行加粗文本 —— 可选能力缺失只影响观感, 不影响内容。
        /// </summary>
        private void DrawPanelHeader(string text, DrawAlign align)
        {
            // 后端支持 PanelHeader 能力时是带底色的横条, 否则退回加粗文本 —— 降级在解释器
            // (旧  解释器) 里做, 产树层只管声明"要一条表头"。
            m_Builder.PanelHeader(text, align, bold: true);
        }

        private void EndGroup(GroupNode node)
        {
            switch (node.Kind)
            {
                case GroupKind.Horizontal:
                case GroupKind.ButtonGroup:
                    m_Builder.EndHorizontal();
                    break;
                default:
                    m_Builder.EndVertical();
                    break;
            }
        }

        /// <summary>
        /// 读取分组展开状态; 没有记录时返回分组属性声明的默认值(见 FoldoutGroupAttribute.Expanded)。
        /// 这里必须带上宿主对象: 分组路径在同类型的所有实例上是同一个字符串,
        /// 若只按路径存取, 同窗口里的两个同类型对象会共用一次折叠。
        /// </summary>
        private bool GetGroupExpanded(object target, GroupNode node)
        {
            return m_Ctx.Session.GetGroupToggle(target, node.GroupId, node.Expanded);
        }

        private void SetGroupExpanded(object target, GroupNode node, bool value)
        {
            m_Ctx.Session.SetGroupToggle(target, node.GroupId, value);
        }
    }
}
