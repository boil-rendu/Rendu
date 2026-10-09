using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RD.Core
{
    /// <summary>线程池 <see cref="IScheduler"/>：<c>ProcessorCount</c> 个 worker 线程共享一个 fiber Id 队列，按负载分配，适合轻量短任务 fiber。</summary>
    /// <remarks>每个 worker 自适应「连续处理 fiber 数量」以减少 <c>Thread.Sleep(1)</c> 次数；不持有 fiber → 多个 fiber 自动在 worker 间轮转。</remarks>
    internal class ThreadPoolScheduler: IScheduler
    {
        private readonly List<Thread> threads;

        private readonly ConcurrentQueue<int> idQueue = new();

        private readonly FiberManager fiberManager;

        public ThreadPoolScheduler(FiberManager fiberManager)
        {
            this.fiberManager = fiberManager;
            int threadCount = Environment.ProcessorCount;
            this.threads = new List<Thread>(threadCount);
            for (int i = 0; i < threadCount; ++i)
            {
                Thread thread = new(this.Loop);
                this.threads.Add(thread);
                thread.Start();
            }
        }

        /// <summary>worker 工作循环：取 fiber Id → 切上下文 → <c>Update</c>+<c>LateUpdate</c> → 入队轮转；连续处理 <c>idQueue.Count / threadCount + 1</c> 次后 sleep 1ms 让其他 worker 有机会抢。</summary>
        private void Loop()
        {
            int count = 0;
            while (true)
            {
                if (count <= 0)
                {
                    Thread.Sleep(1);

                    // count最小为1
                    count = this.idQueue.Count / this.threads.Count + 1;
                }

                --count;

                if (this.fiberManager.IsDisposed())
                {
                    return;
                }

                if (!this.idQueue.TryDequeue(out int id))
                {
                    Thread.Sleep(1);
                    continue;
                }

                Fiber fiber = this.fiberManager.Get(id);
                if (fiber == null)
                {
                    continue;
                }

                if (fiber.IsDisposed)
                {
                    continue;
                }

                Fiber.Instance = fiber;
                SynchronizationContext.SetSynchronizationContext(fiber.ThreadSynchronizationContext);
                fiber.Update();
                fiber.LateUpdate();
                SynchronizationContext.SetSynchronizationContext(null);
                Fiber.Instance = null;

                this.idQueue.Enqueue(id);
            }
        }

        /// <summary>等所有 worker <c>Join</c> 后退出。</summary>
        public void Dispose()
        {
            foreach (Thread thread in this.threads)
            {
                thread.Join();
            }
        }

        /// <summary>把 <paramref name="fiberId"/> 加入共享队列（无锁，<see cref="ConcurrentQueue{T}"/>）。</summary>
        public void Add(int fiberId)
        {
            this.idQueue.Enqueue(fiberId);
        }
    }
}
