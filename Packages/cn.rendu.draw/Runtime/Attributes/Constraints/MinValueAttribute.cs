using System;

namespace RD.Draw
{
    /// <summary>限制数值最小值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class MinValueAttribute : Attribute
    {
        public double MinValue { get; set; }
        public string MemberName { get; set; }

        public MinValueAttribute(double minValue)
        {
            MinValue = minValue;
        }

        public MinValueAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
