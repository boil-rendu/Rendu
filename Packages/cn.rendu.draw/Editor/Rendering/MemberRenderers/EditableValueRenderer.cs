using System;
using System.Reflection;
using UnityEngine;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 兜底档: 一个真正可编辑的成员 —— 禁用/颜色状态栈 + 控件 + 变更检测 + 写回 +
    /// [OnValueChanged]。
    ///
    /// 它承载"字段怎么被编辑"这条骨架, 这条骨架只服务于这条路径(其余各档都是一次性
    /// 绘制或展开, 不写值), 所以骨架归它而不是归主流程, 是准确的归属。
    /// </summary>
    internal sealed class EditableValueRenderer : MemberRendererBase
    {
        private readonly VisibilityEvaluator m_Visibility;
        private readonly ValueControlChain m_Controls;

        public EditableValueRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values,
                                     ILabelFormatter labels, ITypeShape shape,
                                     VisibilityEvaluator visibility, ValueControlChain controls)
            : base(ctx, builder, values, labels, shape)
        {
            m_Visibility = visibility;
            m_Controls   = controls;
        }

        public override bool CanDraw(in MemberDrawArgs args) => true;   // 兜底

        public override void Draw(in MemberDrawArgs args)
        {
            var target = args.Value.Target;
            var member = args.Value.Member;
            var label  = args.Value.Label;

            var readOnly = member.GetAttr<ReadOnlyAttribute>() != null;
            if (member.Kind == MemberKind.Property && (member.Info as PropertyInfo)?.SetMethod == null)
            {
                readOnly = true;
            }

            var disabled = readOnly || m_Visibility.IsDisabled(member, target);

            // 产树: 外观作用域(禁用/颜色) + 行 + 值控件 + 行内按钮。写回由 ChangeApplier 统一做。
            var color    = Labels.ResolveGUIColor(member);
            var hasColor = color.HasValue;

            Builder.PushEnabled(disabled == false);
            if (hasColor) Builder.PushColor(color.Value);

            try
            {
                Builder.BeginHorizontal();

                // 复用主流程已读到的值: 同一次绘制里再走一遍反射 getter 只是重复装箱。
                m_Controls.Resolve(new ValueDrawArgs(target, member, args.Value.Type, args.Value.Value, label));

                DrawInlineButtons(target, member);

                Builder.EndHorizontal();
            }
            catch (Exception e) when (!(e is ExitGUIException))
            {
                DrawLog.Error($"[Inspector] 建树失败 {member.RawName}\n{e}");
            }
            finally
            {
                // 建树器是"当前节点"栈: 抛异常也必须把作用域弹回去, 否则同一帧后面的节点会挂错父级。
                if (hasColor) Builder.PopScope();
                Builder.PopScope();
            }
        }

        /// <summary>
        /// [InlineButton]: 画在值框旁边的行内小按钮。
        ///
        /// 它属于"可编辑成员这一行", 所以跟可编辑档住在一起 —— 其余各档不写值, 也就没有行内按钮。
        /// </summary>
        private void DrawInlineButtons(object target, CachedMember member)
        {
            var buttons = member.GetAttrs<InlineButtonAttribute>();
            if (buttons.Length == 0) return;

            foreach (var attr in buttons)
            {
                var text = attr.ButtonLabel;
                if (string.IsNullOrEmpty(text)) text = attr.MemberMethod;

                // 按钮动作在渲染后由 ChangeApplier 按点击键匹配执行。
                Builder.Button(text, InlineButtonSpec, () =>
                {
                    var method = Values.FindMethod(target, attr.MemberMethod);
                    if (method != null)
                    {
                        Values.InvokeSafe(target, method);
                    }
                });
            }
        }

        /// <summary>[InlineButton]: 按文字宽度收缩, 不吃整行。</summary>
        private static readonly DrawButtonSpec InlineButtonSpec = DrawButtonSpec.Of(expandWidth: false);
    }
}
