using System;

namespace RD.Draw
{
    /// <summary>标题组（带横线的大标题分组）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TitleGroupAttribute : PropertyGroupAttribute
    {
        private readonly TitleAlignment m_Alignment;
        private readonly bool m_HorizontalLine;
        private readonly bool m_BoldTitle;

        /// <summary>子级缩进(引擎当前未读取, 保留默认 false 行为)。</summary>
        public bool IndentChildren { get; set; }

        public override GroupKind Kind => GroupKind.Title;
        public override TitleAlignment Alignment => m_Alignment;
        public override bool HorizontalLine => m_HorizontalLine;
        public override bool BoldTitle => m_BoldTitle;

        public TitleGroupAttribute(string groupName, TitleAlignment alignment = TitleAlignment.Left,
                                   bool horizontalLine = true, bool boldTitle = true, bool indentChildren = false)
            : base(groupName)
        {
            m_Alignment      = alignment;
            m_HorizontalLine = horizontalLine;
            m_BoldTitle      = boldTitle;
            IndentChildren   = indentChildren;
        }
    }
}
