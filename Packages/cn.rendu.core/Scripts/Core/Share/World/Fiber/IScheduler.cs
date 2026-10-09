using System;

namespace RD.Core
{
    /// <summary>Fiber 调度器接口：把 fiber Id 投入调度循环；<see cref="IDisposable.Dispose"/> 负责停止所有 worker 线程。</summary>
    /// <remarks>实现：<see cref="MainThreadScheduler"/>（主线程 Unity 驱动）、<see cref="ThreadScheduler"/>（每 fiber 一线程）、<see cref="ThreadPoolScheduler"/>（线程池共享）。</remarks>
    public interface IScheduler: IDisposable
    {
        /// <summary>把指定 <paramref name="fiberId"/> 投入调度。</summary>
        /// <param name="fiberId"><see cref="FiberManager"/> 中的 fiber Id。</param>
        void Add(int fiberId);
    }
}
