using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace RD.Core
{
    /// <summary>
    /// 网络服务抽象基类：聚合 channel 管理 + 收包回调 + 内存池。派生：<see cref="KService"/>（KCP/UDP）、<c>TService</c>（TCP）。
    /// <see cref="Update"/> 由引擎每帧驱动（处理 accept/recv/timeout/send 缓冲），业务通过 <see cref="AcceptCallback"/>/<see cref="ReadCallback"/>/<see cref="ErrorCallback"/> 三个回调接收事件。
    /// </summary>
    public abstract class AService: IDisposable
    {
        /// <summary>新连接建立回调：<c>(channelId, remoteEndPoint)</c>。</summary>
        public Action<long, IPEndPoint> AcceptCallback;
        /// <summary>收到一个完整包回调：<c>(channelId, memoryBuffer)</c>（包头已剥，承载 <c>Opcode + Body</c>）。</summary>
        public Action<long, MemoryBuffer> ReadCallback;
        /// <summary>错误回调：<c>(channelId, errorCode)</c>。</summary>
        public Action<long, int> ErrorCallback;

        /// <summary>Service 自身 Id（由 <see cref="IdGenerator.GenerateId"/> 生成，dispose 后置 0）。</summary>
        public long Id { get; set; } = IdGenerator.Instance.GenerateId();

        /// <summary>服务类型（Inner 走内网协议、Outer 走外网协议），由派生类在构造时设置。</summary>
        public ServiceType ServiceType { get; protected set; }

        private const int MaxMemoryBufferSize = 1024;

        private readonly Queue<MemoryBuffer> pool = new();

        /// <summary>从池中取出或新建 <see cref="MemoryBuffer"/>：默认 1KB，<paramref name="size"/> 大于 1KB 立即按需分配不放入池。</summary>
        /// <param name="size">期望承载的数据量（≤ 1KB 走池化，否则按需分配）。</param>
        public MemoryBuffer Fetch(int size = 0)
        {
            if (size > MaxMemoryBufferSize)
            {
                return new MemoryBuffer(size);
            }

            if (size < MaxMemoryBufferSize)
            {
                size = MaxMemoryBufferSize;
            }

            if (this.pool.Count == 0)
            {
                return new MemoryBuffer(size);
            }

            return pool.Dequeue();
        }

        /// <summary>回收 <see cref="MemoryBuffer"/> 到池：仅容量 ≤ 1KB 的入池，且池上限 10 个（避免占内存）；按需分配的不回收。</summary>
        public void Recycle(MemoryBuffer memoryBuffer)
        {
            if (memoryBuffer.Capacity > 1024)
            {
                return;
            }

            if (this.pool.Count > 10) // 这里不需要太大，其实Kcp跟Tcp,这里1就足够了
            {
                return;
            }

            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);

            this.pool.Enqueue(memoryBuffer);
        }


        /// <summary>虚释放：默认把 <see cref="Id"/> 置 0，派生类可 override 释放 socket/线程等资源。</summary>
        public virtual void Dispose()
        {
            this.Id = 0;
        }

        /// <summary>每帧驱动：accept 新连接 + 处理已有连接的收包/发送/超时（实现侧必须 <c>Thread.Sleep</c> 自适应节奏）。</summary>
        public abstract void Update();

        /// <summary>强制断开指定 channel（<paramref name="error"/> 非 0 触发 <see cref="ErrorCallback"/>）。</summary>
        public abstract void Remove(long id, int error = 0);

        /// <summary>Service 是否已释放（Id == 0 即已 Dispose）。</summary>
        public bool IsDisposed()
        {
            return this.Id == 0;
        }

        /// <summary>新建一个 channel（Connect 侧主动 connect；Accept 侧由 <c>Create</c> 注册到连接表）。</summary>
        public abstract void Create(long id, IPEndPoint ipEndPoint);

        /// <summary>通过 <paramref name="channelId"/> 发送已包好的 <see cref="MemoryBuffer"/>（包头已由调用方写好）。</summary>
        public abstract void Send(long channelId, MemoryBuffer memoryBuffer);

        /// <summary>取 KCP 连接的 (localConn, remoteConn) 对（仅 KCP 有效，TCP 实现会抛异常）。</summary>
        public virtual (uint, uint) GetChannelConn(long channelId)
        {
            throw new Exception($"default conn throw Exception! {channelId}");
        }

        /// <summary>把 channel 切到新地址（占位，默认空实现；KCP Router 场景会用到）。</summary>
        public virtual void ChangeAddress(long channelId, IPEndPoint ipEndPoint)
        {
        }
    }
}
