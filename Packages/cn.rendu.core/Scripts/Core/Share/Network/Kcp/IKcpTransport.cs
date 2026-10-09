using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace RD.Core
{
    /// <summary>KCP 传输层抽象：把 KCP 的 Send/Recv 适配到不同的 Socket 类型（<see cref="UdpTransport"/> 直 UDP / <see cref="TcpTransport"/> TCP 模拟）。</summary>
    public interface IKcpTransport: IDisposable
    {
        /// <summary>发送一段字节到 <paramref name="endPoint"/>。</summary>
        /// <param name="channelType">连接类型（<c>Connect</c> 主动连接，<c>Accept</c> 被动接受），影响 <see cref="TcpTransport"/> 的隐式 connect 行为。</param>
        void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType);
        /// <summary>接收一段字节到 <paramref name="buffer"/>，<paramref name="endPoint"/> 输出远端地址。</summary>
        int Recv(byte[] buffer, ref EndPoint endPoint);
        /// <summary>可读字节数（无锁，参考 <see cref="Socket.Available"/> 语义）。</summary>
        int Available();
        /// <summary>每帧驱动（<see cref="TcpTransport"/> 借此做读/写超时检查）。</summary>
        void Update();
        /// <summary>底层错误回调（仅 <see cref="TcpTransport"/> 用）。</summary>
        void OnError(long id, int error);
    }

    /// <summary>UDP 传输：直接用 <see cref="Socket"/> SendTo/ReceiveFrom，macOS 不调整 socket buffer（避免内核压力），其它平台扩到 64MB。</summary>
    public class UdpTransport: IKcpTransport
    {
        private readonly Socket socket;

        public UdpTransport(AddressFamily addressFamily)
        {
            this.socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
            NetworkHelper.SetSioUdpConnReset(this.socket);
        }

        public UdpTransport(IPEndPoint ipEndPoint)
        {
            this.socket = new Socket(ipEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                this.socket.SendBufferSize = Kcp.OneM * 64;
                this.socket.ReceiveBufferSize = Kcp.OneM * 64;
            }

            try
            {
                this.socket.Bind(ipEndPoint);
            }
            catch (Exception e)
            {
                throw new Exception($"bind error: {ipEndPoint}", e);
            }

            NetworkHelper.SetSioUdpConnReset(this.socket);
        }

        public void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType)
        {
            this.socket.SendTo(bytes, index, length, SocketFlags.None, endPoint);
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return this.socket.ReceiveFrom(buffer, ref endPoint);
        }

        public int Available()
        {
            return this.socket.Available;
        }

        public void Update()
        {
        }

        public void OnError(long id, int error)
        {
        }

        public void Dispose()
        {
            this.socket?.Dispose();
        }
    }

    /// <summary>TCP 模拟 KCP：用内部 <c>TService</c> 跑 TCP socket，把 KCP 协议的 Send/Recv 透传到 TCP 流，外部使用方式与 <see cref="UdpTransport"/> 相同。</summary>
    public class TcpTransport: IKcpTransport
    {
        private readonly TService tService;

        private readonly DoubleMap<long, EndPoint> idEndpoints = new();

        private readonly Queue<(EndPoint, MemoryBuffer)> channelRecvDatas = new();

        private readonly Dictionary<long, long> readWriteTime = new();

        private readonly Queue<long> channelIds = new();

        public TcpTransport(AddressFamily addressFamily)
        {
            this.tService = new TService(addressFamily, ServiceType.Outer);
            this.tService.ErrorCallback = this.OnError;
            this.tService.ReadCallback = this.OnRead;
        }

        public TcpTransport(IPEndPoint ipEndPoint)
        {
            this.tService = new TService(ipEndPoint, ServiceType.Outer);
            this.tService.AcceptCallback = this.OnAccept;
            this.tService.ErrorCallback = this.OnError;
            this.tService.ReadCallback = this.OnRead;
        }

        private void OnAccept(long id, IPEndPoint ipEndPoint)
        {
            TChannel channel = this.tService.Get(id);
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            this.readWriteTime[id] = timeNow;
            this.channelIds.Enqueue(id);
            this.idEndpoints.Add(id, channel.RemoteAddress);
        }

        /// <summary>TCP 通道断开回调：清理 endpoint/时间戳映射。</summary>
        public void OnError(long id, int error)
        {
            Log.Warning($"IKcpTransport tcp error: {id} {error}");
            this.tService.Remove(id, error);
            this.idEndpoints.RemoveByKey(id);
            this.readWriteTime.Remove(id);
        }

        private void OnRead(long id, MemoryBuffer memoryBuffer)
        {
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            this.readWriteTime[id] = timeNow;
            TChannel channel = this.tService.Get(id);
            channelRecvDatas.Enqueue((channel.RemoteAddress, memoryBuffer));
        }

        public void Send(byte[] bytes, int index, int length, EndPoint endPoint, ChannelType channelType)
        {
            long id = this.idEndpoints.GetKeyByValue(endPoint);
            if (id == 0)
            {
                if (channelType != ChannelType.Connect)
                {
                    return;
                }

                id = IdGenerator.Instance.GenerateInstanceId();
                this.tService.Create(id, (IPEndPoint)endPoint);
                this.idEndpoints.Add(id, endPoint);
                this.channelIds.Enqueue(id);
            }
            MemoryBuffer memoryBuffer = this.tService.Fetch();
            memoryBuffer.Write(bytes, index, length);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            this.tService.Send(id, memoryBuffer);

            long timeNow = TimeInfo.Instance.ClientFrameTime();
            this.readWriteTime[id] = timeNow;
        }

        public int Recv(byte[] buffer, ref EndPoint endPoint)
        {
            return RecvNonAlloc(buffer, ref endPoint);
        }

        /// <summary>无锁取一条已收数据：<c>(endPoint, memoryBuffer)</c> 队列出队，复用 memoryBuffer 到 <c>tService</c> 池。</summary>
        public int RecvNonAlloc(byte[] buffer, ref EndPoint endPoint)
        {
            (EndPoint e, MemoryBuffer memoryBuffer) = this.channelRecvDatas.Dequeue();
            endPoint = e;
            int count = memoryBuffer.Read(buffer);
            this.tService.Recycle(memoryBuffer);
            return count;
        }

        public int Available()
        {
            return this.channelRecvDatas.Count;
        }

        /// <summary>每帧检查读/写空闲超 30 秒的 TCP channel（一次检查 10 个），并驱动内部 <c>tService.Update()</c>。</summary>
        public void Update()
        {
            // 检查长时间不读写的TChannel, 超时断开, 一次update检查10个
            long timeNow = TimeInfo.Instance.ClientFrameTime();
            const int MaxCheckNum = 10;
            int n = this.channelIds.Count < MaxCheckNum? this.channelIds.Count : MaxCheckNum;
            for (int i = 0; i < n; ++i)
            {
                long id = this.channelIds.Dequeue();
                if (!this.readWriteTime.TryGetValue(id, out long rwTime))
                {
                    continue;
                }
                if (timeNow - rwTime > 30 * 1000)
                {
                    this.OnError(id, ErrorCore.ERR_KcpReadWriteTimeout);
                    continue;
                }
                this.channelIds.Enqueue(id);
            }

            this.tService.Update();
        }

        public void Dispose()
        {
            this.tService?.Dispose();
        }
    }
}
