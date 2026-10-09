using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace RD.Core
{
    /// <summary>主线程 <see cref="IScheduler"/>：所有 <see cref="Fiber"/> 在主线程上串行轮转，由 <c>Engine.Update</c>/<c>Engine.LateUpdate</c> 每帧驱动一次。</summary>
    /// <remarks>
    /// 与 <see cref="ThreadScheduler"/>/<see cref="ThreadPoolScheduler"/> 的关键区别：<c>Main</c> 不会在每帧之间 sleep，由调用方（Unity MainLoop）控制节奏；
    /// 同时负责在派发前后切换 <see cref="SynchronizationContext"/>，让协程 <c>await</c> 回来的回调仍然回到主线程。
    /// </remarks>
    internal class MainThreadScheduler: IScheduler
    {
        private readonly ConcurrentQueue<int> idQueue = new();
        private readonly ConcurrentQueue<int> addIds = new();
        private readonly FiberManager fiberManager;
        private readonly ThreadSynchronizationContext threadSynchronizationContext = new();
        private Fiber firstFiber;

        public MainThreadScheduler(FiberManager fiberManager)
        {
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
            this.fiberManager = fiberManager;
        }

        public void Dispose()
        {
            this.addIds.Clear();
            this.idQueue.Clear();
        }

        /// <summary>本帧 <c>Update</c>：取队列中所有 fiber，依次切上下文后调 <c>fiber.Update()</c>，再入队（轮转）。最后还原上下文避免污染外部 Unity 回调。</summary>
        public void Update()
        {
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
            this.threadSynchronizationContext.Update();

            int count = this.idQueue.Count;
            while (count-- > 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
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
                this.idQueue.Enqueue(id);
            }

            Fiber.Instance = this.firstFiber;
            // Fiber调度完成，要还原成默认的上下文，否则unity的回调会找不到正确的上下文
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }

        /// <summary>本帧 <c>LateUpdate</c>：与 <see cref="Update"/> 类似但晚一帧；同时把 <see cref="addIds"/> 中待加入的 fiber 真正入到 <see cref="idQueue"/>。</summary>
        public void LateUpdate()
        {
            int count = this.idQueue.Count;
            while (count-- > 0)
            {
                if (!this.idQueue.TryDequeue(out int id))
                {
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
                fiber.LateUpdate();
                this.idQueue.Enqueue(id);
            }

            while (this.addIds.Count > 0)
            {
                this.addIds.TryDequeue(out int result);
                this.idQueue.Enqueue(result);
            }

            Fiber.Instance = this.firstFiber;
            // Fiber调度完成，要还原成默认的上下文，否则unity的回调会找不到正确的上下文
            SynchronizationContext.SetSynchronizationContext(this.threadSynchronizationContext);
        }

        /// <summary>把 fiber 暂存到 <see cref="addIds"/>，在下一个 <see cref="LateUpdate"/> 才真正加入调度队列（避免在迭代中修改集合）。首次加入的 fiber 会被记为 <see cref="firstFiber"/>。</summary>
        /// <param name="fiberId">要调度的 fiber Id，默认 0（主 fiber）。</param>
        public void Add(int fiberId = 0)
        {
            this.addIds.Enqueue(fiberId);
            this.firstFiber ??= this.fiberManager.Get(fiberId);
        }
    }
}
