using CommandLine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 自启动特性
    /// </summary>
    public class UISingletonAttribute : RD.Core.BaseAttribute
    {
        /// <summary>
        /// 启动优先级
        /// 数字越小，优先级越高
        /// </summary>
        public int Order { get; set; }

        public UISingletonAttribute(int order = 0)
        {
            Order = order;
        }
    }
}
