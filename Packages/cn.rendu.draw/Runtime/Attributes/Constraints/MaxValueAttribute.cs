using System;

namespace RD.Draw
{
    /// <summary>限制数值最大值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class MaxValueAttribute : Attribute
    {
        public double MaxValue { get; set; }
        public string MemberName { get; set; }

        public MaxValueAttribute(double maxValue)
        {
            MaxValue = maxValue;
        }

        public MaxValueAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }
}
