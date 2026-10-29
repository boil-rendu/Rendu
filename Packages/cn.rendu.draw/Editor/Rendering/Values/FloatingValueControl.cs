using System;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>浮点值控件: 有 [MinValue]+[MaxValue] 画滑杆, 否则画浮点输入框。</summary>
    internal sealed class FloatingValueControl : ValueControlBase
    {
        public FloatingValueControl(WidgetBuilder builder) : base(builder)
        {
        }

        public override bool CanHandle(in ValueDrawArgs args)
        {
            switch (Type.GetTypeCode(args.Type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        public override void Build(in ValueDrawArgs args)
        {
            if (TrySliderRange(args.Member, out var min, out var max))
            {
                Builder.Slider(args.Target, args.Member, args.Label, NumberOrZero(args.Value), min, max, 0f);
                return;
            }

            Builder.FloatField(args.Target, args.Member, args.Label, NumberOrZero(args.Value), 0f);
        }
    }
}