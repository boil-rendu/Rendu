using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RD.Core
{
    // 一个Fiber一个固定的线程
    /// <summary>每 fiber 独占一线程的 <see cref="IScheduler"/>：适合 CPU 密集、长任务 fiber，规避线程切换但占用线程数 = fiber 数。</summary>
    internal class ThreadScheduler: IScheduler
    {
        private readonly ConcurrentDictionary<int, Thread> dictionary = new();

        private readonly FiberManager fiberManager;

        public ThreadScheduler(FiberManager fiberManager)
        {
            this.fiberManager = fiberManager;
        }

        /// <summary>目标 fiber 的工作循环：<c>Update</c> → <c>LateUpdate</c> → <c>Thread.Sleep(1)</c>，持续到 fiber 释放或 <see cref="FiberManager"/> 整体 dispose。</summary>
        private void Loop(int fiberId)
        {
            Fiber fiber = fiberManager.Get(fiberId);
            Fiber.Instance = fiber;
            SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);

            while (true)
            {
                if (this.fiberManager.IsDisposed())
                {
                    return;
                }

                fiber = fiberManager.Get(fiberId);
                if (fiber == null)
                {
                    this.dictionary.Remove(fiberId, out _);
                    return;
                }

                if (fiber.IsDisposed)
                {
                    this.dictionary.Remove(fiberId, out _);
                    return;
                }

                fiber.Update();
                fiber.LateUpdate();

                Thread.Sleep(1);
            }
        }

        /// <summary>等所有 worker 线程 <c>Join</c> 后退出，用于进程级关闭时优雅收尾。</summary>
        public void Dispose()
        {
            foreach (var kv in this.dictionary.ToArray())
            {
                kv.Value.Join();
            }
        }

        /// <summary>为指定 <paramref name="fiberId"/> 起一条专属 worker 线程。</summary>
        public void Add(int fiberId)
        {
            Thread thread = new(() => this.Loop(fiberId));
            this.dictionary.TryAdd(fiberId, thread);
            thread.Start();
        }
    }
}
