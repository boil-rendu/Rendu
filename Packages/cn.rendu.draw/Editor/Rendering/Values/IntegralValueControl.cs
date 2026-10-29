using System;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>整数值控件: 有 [MinValue]+[MaxValue] 画滑杆, 否则画整数输入框。</summary>
    internal sealed class IntegralValueControl : ValueControlBase
    {
        public IntegralValueControl(WidgetBuilder builder) : base(builder)
        {
        }

        public override bool CanHandle(in ValueDrawArgs args)
        {
            switch (Type.GetTypeCode(args.Type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
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

            Builder.IntegerField(args.Target, args.Member, args.Label, IntegerOrZero(args.Value), 0f);
        }
    }
}