using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 值成员的渲染骨架: 标签宽作用域 + 批注 + [Required] 校验 + 交给成员渲染链。
    ///
    /// "这个成员整体由谁画"是 <see cref="MemberRenderChain"/> 的事;
    /// 本类只负责**画它之前要做的事**: 压标签宽、画标题与提示、校验必填。
    ///
    /// 它是"值成员"这条路径的入口, 表格单元格也走这里(只是标签宽压成 0)。
    ///
    /// ── 渲染层的命名约定 ─────────────────────────────────────────────
    /// 这一层有三族同名后缀的类, 职责不同, 靠**动词**区分, 不可互换:
    ///   - <c>*Renderer</c> 渲染一段结构, 方法一律 <c>Render*</c>:
    ///     本类、<see cref="GroupRenderer"/>、<see cref="CollectionRenderer"/>、<see cref="TableRenderer"/>。
    ///     它们的依赖各不相同(分组要 StateUpdater、表格要 TableSchema 与诊断 sink),
    ///     **刻意不抽公共基类** —— 唯一共同点只有 DrawContext, 抽出来只会得到一个
    ///     "持有 ctx 的空壳", 反而让人误以为它们能互换。
    ///   - <c>*Renderer</c> 且实现 <see cref="IMemberRenderer"/>: 成员渲染链上的一档
    ///     (见 <c>MemberRenderers/</c>), 共享 <see cref="MemberRendererBase"/> 的四个依赖 ——
    ///     这一族才是真同族, 基类成立。
    ///   - <c>*Painter</c> 被注入的一项绘制能力, 方法一律 <c>Paint*</c>:
    ///     <see cref="AnnotationPainter"/>、<see cref="ButtonPainter"/>。它们不"渲染一段结构",
    ///     因此不叫 Renderer。
    /// </───────────────────────────────────────────────────────────────
    /// </summary>
    internal sealed class ValueMemberRenderer
    {
        private readonly DrawContext m_Ctx;
        private readonly WidgetBuilder m_Builder;
        private readonly ILabelFormatter m_Labels;
        private readonly VisibilityEvaluator m_Visibility;
        private readonly ValueAccessor m_Values;
        private readonly AnnotationPainter m_AnnotationPainter;
        private readonly MemberRenderChain m_MemberRenderers;

        public ValueMemberRenderer(DrawContext ctx, WidgetBuilder builder, ILabelFormatter labels,
                                   VisibilityEvaluator visibility, ValueAccessor values,
                                   AnnotationPainter annotationPainter, MemberRenderChain memberRenderers)
        {
            m_Ctx               = ctx;
            m_Builder           = builder;
            m_Labels            = labels;
            m_Visibility        = visibility;
            m_Values            = values;
            m_AnnotationPainter = annotationPainter;
            m_MemberRenderers   = memberRenderers;
        }

        /// <summary>
        /// 绘制字段/属性。LabelWidth: 绘制期间覆盖 EditorGUIUtility.labelWidth。
        /// 表格单元格(inTableCell)内标签由表头承担, 因此强制 labelWidth = 0。
        /// </summary>
        public void Render(object target, CachedMember member, EditorWindow host, int depth, bool inTableCell)
        {
            if (inTableCell)
            {
                // 单元格里真正让标签消失的是 LabelForControl 返回的空标签; 这个只是给
                // 带自有标签的嵌套控件(向量分量之类)少留点位置。
                m_Builder.PushLabelWidth(0f);
            }
            else
            {
                // 没有 [LabelWidth] 时传 null: 保持外层宽度, 别把上层设好的值顶掉
                var labelWidth = member.GetAttr<LabelWidthAttribute>();
                m_Builder.PushLabelWidth(labelWidth != null ? labelWidth.Width : (float?)null);
            }

            try
            {
                RenderBody(target, member, host, depth, inTableCell);
            }
            finally
            {
                m_Builder.PopScope();
            }
        }

        private void RenderBody(object target, CachedMember member, EditorWindow host, int depth, bool inTableCell)
        {
            var type = member.ValueType;
            if (type == null) return;

            m_AnnotationPainter.PaintTitlePrelude(target, member);

            // 值只读一次, 同时交给校验与渲染链, 三处看到的一定是同一个值。
            var value = m_Values.Read(member, target);

            // Required 校验: 值缺失时给出提示(表格单元格内不画, 避免撑高行)
            if (inTableCell == false) DrawRequired(target, member, value);

            // "谁画这个成员"的判断整条交给渲染链(见 MemberRenderChain 的优先级说明)。
            // 这里只负责准备上下文: 标签怎么取取决于是否在单元格里。
            m_MemberRenderers.Resolve(new MemberDrawArgs(
                new ValueDrawArgs(target, member, type, value,
                                  m_Labels.LabelForControl(member, inTableCell)),
                host, depth, inTableCell));
        }

        /// <summary>值缺失(空/默认值)时按 [Required] 的提示等级给出提示。</summary>
        /// <param name="value">已经读到的值 —— 不再重复读一次反射 getter。</param>
        private void DrawRequired(object target, CachedMember member, object value)
        {
            var attr = member.GetAttr<RequiredAttribute>();
            if (attr == null) return;
            if (RequiredIsValid(value, member.ValueType)) return;

            var message = string.IsNullOrEmpty(attr.ErrorMessage)
                ? $"{m_Labels.GetLabel(member)} 是必填项"
                : attr.ErrorMessage;
            m_Builder.HelpBox(message, AnnotationPainter.ToMessageKind(attr.MessageType));
        }

        /// <summary>null、空字符串、空 Unity 对象、空集合、值类型的默认值都视为"未填写"。</summary>
        private static bool RequiredIsValid(object value, Type type)
        {
            if (value == null) return false;
            if (value is string s) return string.IsNullOrWhiteSpace(s) == false;
            if (value is UnityEngine.Object uo) return uo != null;
            if (value is ICollection c) return c.Count > 0;

            if (type != null && type.IsValueType)
            {
                try
                {
                    // 默认值只跟类型有关, 缓存一份即可, 结果永不改变。
                    if (Equals(value, DefaultOf(type))) return false;
                }
                catch
                {
                    // 无法构造的类型不做默认值判断
                }
            }

            return true;
        }

        /// <summary>值类型的默认值(装箱后缓存); 构造不了的类型不缓存, 交给调用方的 catch。</summary>
        private static object DefaultOf(Type type)
        {
            if (s_Defaults.TryGetValue(type, out var cached)) return cached;

            var value = Activator.CreateInstance(type);
            s_Defaults[type] = value;
            return value;
        }

        private static readonly Dictionary<Type, object> s_Defaults = new();
    }
}
