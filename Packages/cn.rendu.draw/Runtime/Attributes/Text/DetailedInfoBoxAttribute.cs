using System;

namespace RD.Draw
{
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
}
