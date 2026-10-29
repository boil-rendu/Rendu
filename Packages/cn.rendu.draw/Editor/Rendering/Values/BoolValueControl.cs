using System;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>布尔值: 复选框。</summary>
    internal sealed class BoolValueControl : ValueControlBase
    {
        public BoolValueControl(WidgetBuilder builder) : base(builder)
        {
        }

        public override bool CanHandle(in ValueDrawArgs args) => args.Type == typeof(bool);

        public override void Build(in ValueDrawArgs args)
        {
            Builder.ToggleField(args.Target, args.Member, args.Label, Convert.ToBoolean(args.Value), 0f);
        }
    }
}