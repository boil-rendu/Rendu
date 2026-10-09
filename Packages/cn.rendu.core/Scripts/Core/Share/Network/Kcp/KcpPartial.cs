namespace RD.Core
{
    /// <summary>KCP 相关常量集合（partial class，KCP 核心算法实现在 <c>Kcp.cs</c>）。</summary>
    public partial class Kcp
    {
        /// <summary>1MB 字节数（KCP 缓冲区/MTU 换算用）。</summary>
        public const int OneM = 1024 * 1024;
        /// <summary>Inner 服务单 channel 最大等待缓冲（1MB）。</summary>
        public const int InnerMaxWaitSize = 1024 * 1024;
        /// <summary>Outer 服务单 channel 最大等待缓冲（1MB）。</summary>
        public const int OuterMaxWaitSize = 1024 * 1024;
    }


}
