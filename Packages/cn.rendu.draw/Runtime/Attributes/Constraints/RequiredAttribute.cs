using System;

namespace RD.Draw
{
    /// <summary>字段值校验：为空/默认值时在 Inspector 中报错。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RequiredAttribute : Attribute
    {
        public string ErrorMessage { get; set; }
        public InfoMessageType MessageType { get; set; }

        public RequiredAttribute(string errorMessage = null, InfoMessageType infoMessageType = InfoMessageType.Error)
        {
            ErrorMessage = errorMessage;
            MessageType = infoMessageType;
        }
    }
}
