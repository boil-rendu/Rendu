using System;

namespace RD.Draw
{
    /// <summary>在成员上方绘制一个标题。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TitleAttribute : Attribute
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public TitleAlignment Alignment { get; set; }
        public bool HorizontalLine { get; set; }
        public bool Bold { get; set; }

        public TitleAttribute(string title, string subtitle = null,
                              TitleAlignment alignment = TitleAlignment.Left,
                              bool horizontalLine = true, bool bold = true)
        {
            Title          = title;
            Subtitle       = subtitle;
            Alignment      = alignment;
            HorizontalLine = horizontalLine;
            Bold           = bold;
        }
    }
}
