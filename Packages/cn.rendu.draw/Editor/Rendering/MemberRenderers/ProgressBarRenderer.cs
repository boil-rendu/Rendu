using System;
using UnityEngine;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// [ProgressBar]: 数值成员按进度条显示, 不画输入框。
    ///
    /// 表格单元格里不画 —— 进度条会把行撑高, 破坏表格的等高行。
    /// </summary>
    internal sealed class ProgressBarRenderer : MemberRendererBase
    {
        public ProgressBarRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values,
                                   ILabelFormatter labels, ITypeShape shape)
            : base(ctx, builder, values, labels, shape)
        {
        }

        public override bool CanDraw(in MemberDrawArgs args)
        {
            return args.Value.Member.GetAttr<ProgressBarAttribute>() != null
                && args.InTableCell == false;
        }

        public override void Draw(in MemberDrawArgs args)
        {
            var member = args.Value.Member;
            var attr   = member.GetAttr<ProgressBarAttribute>();
            var value  = Values.Read(member, args.Value.Target);

            float ratio;
            try
            {
                ratio = Convert.ToSingle(value);
            }
            catch (Exception)
            {
                return;   // 不是数值: 特性用错了, 不画(也不该把绘制炸掉)
            }

            ratio = Mathf.Clamp01(ratio);

            var title = attr.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = Labels.GetLabel(member);
            }

            if (string.IsNullOrEmpty(title))
            {
                title = $"{Mathf.RoundToInt(ratio * 100f)}%";
            }

            Builder.ProgressBar(ratio, title, attr.Height);
        }
    }
}
