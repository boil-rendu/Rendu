using System;

namespace RD.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class InvokeAttribute : BaseAttribute
    {
        public long Type { get; private set; }
        public string InvokeType { get; private set; }

        // 使用 long type 参数
        public InvokeAttribute(long type = 0)
        {
            this.Type = type;
        }

        // 使用 string invokeType 参数
        public InvokeAttribute(string invokeType)
        {
            this.InvokeType = invokeType;
        }

        // 无参数，读取方法名
        public InvokeAttribute()
        {
        }
    }

}