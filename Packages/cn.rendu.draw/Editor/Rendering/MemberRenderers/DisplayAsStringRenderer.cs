using System.Text.RegularExpressions;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// [DisplayAsString]: 只读文本, 不给编辑控件。
    ///
    /// 它排在进度条之前 —— 显式声明只读, 否则 [ProgressBar] 会盖掉它。
    /// </summary>
    internal sealed class DisplayAsStringRenderer : MemberRendererBase
    {
        /// <summary>剔除 Unity 富文本标记, 用于统计可见字符数或生成纯文本截断结果。</summary>
        private static readonly Regex s_RichTagRegex = new("<[^>]+>", RegexOptions.Compiled);

        public DisplayAsStringRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values,
                                       ILabelFormatter labels, ITypeShape shape)
            : base(ctx, builder, values, labels, shape)
        {
        }

        public override bool CanDraw(in MemberDrawArgs args)
        {
            return args.Value.Member.GetAttr<DisplayAsStringAttribute>() != null;
        }

        public override void Draw(in MemberDrawArgs args)
        {
            var member = args.Value.Member;
            var attr   = member.GetAttr<DisplayAsStringAttribute>();
            if (attr == null) return;

            var value = Values.Read(member, args.Value.Target);
            var text  = value?.ToString() ?? "";
            if (attr.MaxCharCount > 0)
            {
                // 富文本标记不计入截断长度, 且截断只作用于剥离标记后的正文,
                // 避免把标签拦腰砍成非法残片被当成明文显示。
                var visible = s_RichTagRegex.Replace(text, "");
                if (visible.Length > attr.MaxCharCount)
                {
                    text = visible.Substring(0, attr.MaxCharCount) + "...";
                }
            }

            var spec = AnnotationPainter.ToTextSpec(attr.Alignment, wrap: true, richText: true);

            var color    = Labels.ResolveGUIColor(member);
            var hasColor = color.HasValue;
            if (hasColor) Builder.PushColor(color.Value);

            // 标签在表格单元格里也要画: [DisplayAsString] 在表格单元格里照样显示,
            // 而 LabelForControl 在单元格内恒返回空 —— 那是给列名已由表头承担的输入框用的。
            var label = Labels.LabelOrEmpty(member);
            Builder.Label(string.IsNullOrEmpty(label) ? text : $"{label} {text}", spec);

            if (hasColor) Builder.PopScope();
        }
    }
}
