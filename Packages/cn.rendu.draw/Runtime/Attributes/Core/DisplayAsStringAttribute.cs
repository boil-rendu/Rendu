using System;
using UnityEngine;

namespace RD.Draw
{
    /// <summary>以字符串形式显示成员值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class DisplayAsStringAttribute : Attribute
    {
        public bool Overflow { get; set; }

        /// <summary>最大显示字符数(0 表示不限制)。</summary>
        public int MaxCharCount { get; set; }

        /// <summary>文本对齐方式。</summary>
        public TextAlignment Alignment { get; set; }

        /// <summary>对齐是否作用于整个属性标签/布局。</summary>
        public bool AlignLabel { get; set; }

        public DisplayAsStringAttribute(bool overflow = true)
        {
            Overflow = overflow;
        }

        public DisplayAsStringAttribute(bool overflow, int maxCharCount, TextAlignment alignment, bool alignLabel)
        {
            Overflow = overflow;
            MaxCharCount = maxCharCount;
            Alignment = alignment;
            AlignLabel = alignLabel;
        }
    }
}
