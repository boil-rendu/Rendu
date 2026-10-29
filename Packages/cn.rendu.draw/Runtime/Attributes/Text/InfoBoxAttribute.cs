using System;

namespace RD.Draw
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
}
