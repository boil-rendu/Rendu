using System;

namespace RD.Draw
{
    /// <summary>将多个按钮排在同一行（同组名的按钮一行）。</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ButtonGroupAttribute : PropertyGroupAttribute
    {
        public override GroupKind Kind => GroupKind.ButtonGroup;

        public ButtonGroupAttribute(string name = "_DefaultGroup", int order = 0)
            : base(name, order)
        {
        }

        public ButtonGroupAttribute(int order)
            : base("_DefaultGroup", order)
        {
        }
    }
}
