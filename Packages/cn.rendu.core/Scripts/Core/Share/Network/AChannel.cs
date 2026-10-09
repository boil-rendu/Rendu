using System;
using System.IO;
using System.Net;

namespace RD.Core
{
    /// <summary>通道类型：<c>Connect</c> = 主动连接方、<c>Accept</c> = 被动接受方（影响 <see cref="IKcpTransport.Send"/> 的隐式 connect 行为）。</summary>
    public enum ChannelType
    {
        /// <summary>主动 connect 侧。</summary>
        Connect,
        /// <summary>被动 accept 侧。</summary>
        Accept,
    }

    /// <summary>网络包头结构：Opcode（2 字节）+ ActorId（内网 16 字节，外网无）+ MessageStream。</summary>
    public struct Packet
    {
        /// <summary>最小包大小：至少要能装下 Opcode（2 字节）。</summary>
        public const int MinPacketSize = 2;
        /// <summary>Opcode 字段长度（2 字节 ushort）。</summary>
        public const int OpcodeLength = 2;
        /// <summary>ActorId 字段在内网包中的起始偏移（0）。</summary>
        public const int ActorIdIndex = 0;
        /// <summary>ActorId 字段长度（Process 4 + Fiber 4 + InstanceId 8 = 16 字节）。</summary>
        public const int ActorIdLength = 16;

        /// <summary>消息操作码。</summary>
        public ushort Opcode;
        /// <summary>目标 ActorId（内网包，外网包不使用）。</summary>
        public long ActorId;
        /// <summary>序列化后的消息体流。</summary>
        public MemoryStream MemoryStream;
    }

    /// <summary>网络 channel 抽象基类：保存 Id + 远端地址 + 错误码 + 释放钩子，派生：<see cref="KChannel"/>（KCP）、<c>TChannel</c>（TCP）。</summary>
    public abstract class AChannel: IDisposable
    {
        /// <summary>channel Id（0 表示已释放）。</summary>
        public long Id;

        /// <summary>channel 类型（主动 connect / 被动 accept）。</summary>
        public ChannelType ChannelType { get; protected set; }

        /// <summary>错误码（0 = 正常，非 0 = 断开原因）。</summary>
        public int Error { get; set; }

        private IPEndPoint remoteAddress;

        /// <summary>远端地址（KCP accept 后由 <c>KChannel</c> 填，TCP 由 <c>TChannel</c> 填）。</summary>
        public IPEndPoint RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
            set
            {
                this.remoteAddress = value;
            }
        }

        /// <summary>是否已释放（<see cref="Id"/> == 0）。</summary>
        public bool IsDisposed
        {
            get
            {
                return this.Id == 0;
            }
        }

        /// <summary>释放 channel：把 <see cref="Id"/> 置 0 + 关闭底层 socket/KCP 资源（由派生类实现）。</summary>
        public abstract void Dispose();
    }
}
