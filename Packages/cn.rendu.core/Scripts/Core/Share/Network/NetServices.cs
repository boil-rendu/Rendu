using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RD.Core
{
    /// <summary>网络协议枚举：标识 Service/Channel 的传输层类型（KCP 默认 / TCP / Websocket / UDP）。</summary>
    public enum NetworkProtocol
    {
        /// <summary>TCP 协议。</summary>
        TCP,
        /// <summary>KCP（基于 UDP 的可靠传输）协议。</summary>
        KCP,
        /// <summary>Websocket 协议。</summary>
        Websocket,
        /// <summary>裸 UDP 协议（不保证可靠/有序）。</summary>
        UDP,
    }

    /// <summary>Channel Id 分配器：分别维护「外网 connect 侧」与「内网 accept 侧」两套 Id 空间，避免互相冲突。</summary>
    public class NetServices: Singleton<NetServices>, ISingletonAwake
    {

        public void Awake()
        {
        }

        // 这个因为是NetClientComponent中使用，不会与Accept冲突
        /// <summary>为外网 connect 侧 channel 分配 Id：使用 <see cref="RandomGenerator.RandUInt32"/> 随机数，与内网 accept Id 空间隔开。</summary>
        public uint CreateConnectChannelId()
        {
            return RandomGenerator.RandUInt32();
        }

        // 防止与内网进程号的ChannelId冲突，所以设置为一个大的随机数
        private int acceptIdGenerator = int.MinValue;

        /// <summary>为内网 accept 侧 channel 分配 Id：从 <see cref="int.MinValue"/> 起 <see cref="Interlocked.Add"/> 单调递增，与 connect 空间互不重叠。</summary>
        public uint CreateAcceptChannelId()
        {
            return (uint)Interlocked.Add(ref this.acceptIdGenerator, 1);
        }

    }
}
