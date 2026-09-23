using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace RD.Core
{
    public static class KcpProtocalType
    {
        public const byte SYN = 1;
        public const byte ACK = 2;
        public const byte FIN = 3;
        public const byte MSG = 4;
        public const byte RouterReconnectSYN = 5;
        public const byte RouterReconnectACK = 6;
        public const byte RouterSYN = 7;
        public const byte RouterACK = 8;
    }

    public enum ServiceType
    {
        Outer,
        Inner,
    }

    /// <summary>
    /// 基于 KCP 的传输服务（Service），管理一组 <see cref="KChannel"/> 的生命周期与收发。
    /// <para>已按职责拆分为多个 <c>partial</c> 文件以降低单文件复杂度（原单文件约 600 行）：</para>
    /// <list type="bullet">
    ///   <item><b>连接表</b>（KChannelTable + KService.Channels.cs）：活跃与等待接受通道的增删查，提供 Get/Create/Remove。</item>
    ///   <item><b>超时调度</b>（KTimeoutScheduler + KService.Timeout.cs）：timeId(MultiMap) + minTime 驱动 KCP 的 Update/超时检测。</item>
    ///   <item><b>路由回调</b>（KService.Router.cs）：routerAckCallback 记录 RouterSYN/ACK 的异步回调。</item>
    ///   <item><b>收发</b>（KService.Recv.cs / KService.Send.cs）：从 IKcpTransport 收取 UDP 包并分派到对应 KChannel，承载握手(SYN/ACK/FIN)与 MSG 数据帧。</item>
    /// </list>
    /// <para>连接表与超时调度已抽取为独立类型 <see cref="KChannelTable"/> / <see cref="KTimeoutScheduler"/>（均有单元测试守护）。本类仍耦合 KCP 协议状态机，进一步改动需在可编译环境下进行。</para>
    /// </summary>
    public sealed partial class KService: AService
    {
        /// <summary>连接 establishment 超时阈值（毫秒）。</summary>
        public const int ConnectTimeoutTime = 20 * 1000;

        // KService创建的时间（基准时间戳）
        private readonly long startTime;

        /// <summary>
        /// 自 KService 创建起的相对时间（毫秒，uint 循环计数），线程安全。
        /// 各 KChannel 的 KCP 超时/重传计时都以此为基准，避免依赖绝对时钟。
        /// </summary>
        public uint TimeNow
        {
            get
            {
                return (uint)(TimeInfo.Instance.ClientFrameTime() - this.startTime);
            }
        }

        /// <summary>底层 UDP 传输实现（IKcpTransport），负责真实收发包。</summary>
        public IKcpTransport Transport;
        
        /// <summary>网络协议类型（Outer / Inner）。</summary>
        public NetworkProtocol Protocol { get; set; }

        public KService(IKcpTransport kcpTransport, ServiceType serviceType)
        {
            this.ServiceType = serviceType;
            this.startTime = TimeInfo.Instance.ClientFrameTime();
            this.Transport = kcpTransport;
        }

        /// <summary>
        /// 释放服务：先释放所有活跃通道（级联关闭 KChannel），再释放底层 Transport。
        /// 注意先 ToArray 拷贝 key 集合，避免遍历中修改字典导致异常。
        /// </summary>
        public override void Dispose()
        {
            if (this.IsDisposed())
            {
                return;
            }
            
            base.Dispose();
            
            foreach (long channelId in this.channels.GetActiveKeys().ToArray())
            {
                this.Remove(channelId);
            }

            this.Transport.Dispose();
            this.Transport = null;
        }

        /// <summary>
        /// 获取指定通道的本地/远端连接号（conn），用于 KCP 握手报文的地址回填。
        /// 通道不存在时抛异常——调用方应保证 channelId 有效。
        /// </summary>
        public override (uint, uint) GetChannelConn(long channelId)
        {
            KChannel kChannel = this.Get(channelId);
            if (kChannel == null)
            {
                throw new Exception($"GetChannelConn conn not found KChannel! {channelId}");
            }
            return (kChannel.LocalConn, kChannel.RemoteConn);
        }
        
        /// <summary>
        /// 更新通道的远端地址（IPEndPoint），用于断线重连/路由切换时把通道指向新的对端。
        /// 通道不存在则静默忽略。
        /// </summary>
        public override void ChangeAddress(long channelId, IPEndPoint newIPEndPoint)
        {
            KChannel kChannel = this.Get(channelId);
            if (kChannel == null)
            {
                return;
            }
            kChannel.RemoteAddress = newIPEndPoint;
        }

        /// <summary>
        /// 每帧服务驱动，按固定顺序推进 KCP 网络栈：
        /// <list type="number">
        ///   <item><see cref="TimerOut"/>：处理超时与 KCP 重传/探测（基于 <see cref="TimeNow"/>）。</item>
        ///   <item><see cref="CheckWaitAcceptChannel"/>：检查处于等待接受状态的通道是否完成握手。</item>
        ///   <item><see cref="Recv"/>：从 Transport 收取 UDP 包并分派到对应 KChannel（含 SYN/ACK/FIN 握手与 MSG 数据帧）。</item>
        ///   <item><see cref="UpdateChannel"/>：驱动各 KChannel 的 KCP Update，推进发送/重传/接收窗口。</item>
        ///   <item><see cref="IKcpTransport.Update"/>：刷新底层传输（如 socket 收发缓冲）。</item>
        /// </list>
        /// </summary>
        public override void Update()
        {
            uint timeNow = this.TimeNow;

            this.TimerOut(timeNow);

            this.CheckWaitAcceptChannel(timeNow);
            
            this.Recv();

            this.UpdateChannel(timeNow);
            
            this.Transport.Update();
        }
    }
}
