using System;

namespace RD.Inspector
{
    /// <summary>在 Inspector 中绘制一条提示信息框。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class InfoBoxAttribute : Attribute
    {
        public string Message { get; set; }
        public InfoMessageType InfoMessageType { get; set; }
        public string VisibleIfMemberName { get; set; }

        public InfoBoxAttribute(string message, InfoMessageType infoMessageType = InfoMessageType.None)
        {
            Message = message;
            InfoMessageType = infoMessageType;
        }

        public InfoBoxAttribute(string message, string visibleIfMemberName)
        {
            Message = message;
            VisibleIfMemberName = visibleIfMemberName;
        }

        public InfoBoxAttribute(string message, InfoMessageType infoMessageType, string visibleIfMemberName)
        {
            Message = message;
            InfoMessageType = infoMessageType;
            VisibleIfMemberName = visibleIfMemberName;
        }
    }

    /// <summary>带可折叠详细说明的信息框。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class DetailedInfoBoxAttribute : Attribute
    {
        public string Message { get; set; }
        public string Details { get; set; }
        public InfoMessageType InfoMessageType { get; set; }
        public string VisibleIfMemberName { get; set; }

        public DetailedInfoBoxAttribute(string message, string details,
                                        InfoMessageType infoMessageType = InfoMessageType.None,
                                        string visibleIfMemberName = null)
        {
            Message = message;
            Details = details;
            InfoMessageType = infoMessageType;
            VisibleIfMemberName = visibleIfMemberName;
        }
    }

    /// <summary>在成员上方绘制一个标题。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TitleAttribute : Attribute
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public TitleAlignments Alignment { get; set; }
        public TitleAlignments TitleAlignment { get; set; }
        public bool HorizontalLine { get; set; }
        public bool Bold { get; set; }

        public TitleAttribute(string title, string subtitle = null,
                              TitleAlignments alignment = TitleAlignments.Left,
                              bool horizontalLine = true, bool bold = true)
        {
            Title          = title;
            Subtitle       = subtitle;
            Alignment      = alignment;
            TitleAlignment = alignment;
            HorizontalLine = horizontalLine;
            Bold           = bold;
        }
    }
}
