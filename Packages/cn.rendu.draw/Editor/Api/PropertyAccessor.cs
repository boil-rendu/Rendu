using System;

namespace RD.Draw.Editor
{
    public sealed class PropertyAccessor
    {
        internal ExpressionContext Context;

        public PropertyState State => Context?.State;

        public object Value
        {
            get => Context?.Value;
            set
            {
                if (Context != null) Context.Value = value;
            }
        }
    }
}
