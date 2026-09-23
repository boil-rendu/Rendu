namespace RD.Core
{
    public class UIInvokeSystemAttribute : BaseAttribute
    {
        public string InvokeType { get; }

        public UIInvokeSystemAttribute(string invokeType)
        {
            this.InvokeType = invokeType;
        }
    }
}