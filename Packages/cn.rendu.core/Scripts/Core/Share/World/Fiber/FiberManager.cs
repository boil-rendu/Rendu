using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RD.Core
{
    public enum SchedulerType
    {
        Main,
        Thread,
        ThreadPool,
    }
    
    /// <summary>
    /// 纤程管理器（单例）：持有全部 Fiber 实例与三类调度器（Main / Thread / ThreadPool），
    /// 负责 Fiber 的创建（<see cref="Create"/>）、移除（<see cref="Remove"/>）、查找（<see cref="Get"/>），
    /// 并把主线程的每帧 Update / LateUpdate 分发给 Main 调度器以驱动所有 Fiber。
    /// </summary>
    public class FiberManager: Singleton<FiberManager>, ISingletonAwake, ISingletonReverseDispose
    {
        /// <summary>三类调度器实例，索引对应 <see cref="SchedulerType"/>。</summary>
        private readonly IScheduler[] schedulers = new IScheduler[3];
        
        // 10000000以下为保留的用于StartSceneConfig的fiber id, 1个区配置1000个纤程，可以配置10000个区
        private int idGenerator = 10000000;
        /// <summary>所有存活 Fiber 的 fiberId → Fiber 映射。</summary>
        private ConcurrentDictionary<int, Fiber> fibers = new();

        private MainThreadScheduler mainThreadScheduler;
        
        /// <summary>
        /// 初始化调度器：主线程调度器始终存在；在 Editor/WebGL 下 Thread/ThreadPool 也回落到主线程（无多线程），
        /// 其余平台分别使用独立线程与线程池调度器。
        /// </summary>
        public void Awake()
        {
            this.mainThreadScheduler = new MainThreadScheduler(this);
            this.schedulers[(int)SchedulerType.Main] = this.mainThreadScheduler;
            
#if (ENABLE_VIEW && UNITY_EDITOR) || UNITY_WEBGL
            this.schedulers[(int)SchedulerType.Thread] = this.mainThreadScheduler;
            this.schedulers[(int)SchedulerType.ThreadPool] = this.mainThreadScheduler;
#else
            this.schedulers[(int)SchedulerType.Thread] = new ThreadScheduler(this);
            this.schedulers[(int)SchedulerType.ThreadPool] = new ThreadPoolScheduler(this);
#endif
        }
        
        /// <summary>每帧更新入口：仅驱动主线程调度器（线程/线程池调度器自行在其线程上运行）。</summary>
        public void Update()
        {
            this.mainThreadScheduler.Update();
        }

        /// <summary>每帧帧末入口：仅驱动主线程调度器的 LateUpdate。</summary>
        public void LateUpdate()
        {
            this.mainThreadScheduler.LateUpdate();
        }

        /// <summary>
        /// 销毁：先释放所有调度器，再逐个释放存活 Fiber（级联销毁其 Scene/Entity），
        /// 最后清空 fibers 映射。注意应在进程退出或热重载时调用。
        /// </summary>
        protected override void Destroy()
        {
            foreach (IScheduler scheduler in this.schedulers)
            {
                scheduler.Dispose();
            }
            
            foreach (var kv in this.fibers)
            {
                kv.Value.Dispose();
            }

            this.fibers = null;
        }

        /// <summary>
        /// 以指定的 schedulerType / fiberId 创建一个纤程。
        /// <para>流程：</para>
        /// <list type="number">
        ///   <item>校验 sceneType 合法（非 0）。</item>
        ///   <item>构造 Fiber 并写入 fibers 映射（重复 id 抛错，需先 await <see cref="Remove"/>）。</item>
        ///   <item>将 fiberId 登记到对应调度器，使其进入调度循环。</item>
        ///   <item>把初始化动作 post 到纤程线程，按 sceneType 分发 <see cref="FiberInit"/> 执行（必须在纤程线程内 Init）。</item>
        ///   <item>await 一个 TaskCompletionSource，直到 Init 完成才返回 fiberId，确保调用方拿到的是已初始化好的纤程。</item>
        /// </list>
        /// </summary>
        public async Promise<int> Create(SchedulerType schedulerType, int fiberId, int zone, int sceneType, string name)
        {
            if (sceneType == 0)
            {
                throw new Exception("fiberId is 0");
            } 
            
            try
            {
                Fiber fiber = new(fiberId, zone, sceneType, name);

                if (!this.fibers.TryAdd(fiberId, fiber))
                {
                    throw new Exception($"same fiber already existed, if you remove, please await Remove then Create fiber! {fiberId}");
                }
                this.schedulers[(int) schedulerType].Add(fiberId);
                
                // 用 TCS 等待纤程线程内的 Init 完成，使 Create 返回时纤程已可用
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                // 必须在 Fiber 线程中执行 Init 分发
                fiber.ThreadSynchronizationContext.Post(() =>
                {
                    Action().NoContext();
                });

                await tcs.Task;
                return fiberId;

                async Promise Action()
                {
                    try
                    {
                        // 根据Fiber的SceneType分发Init,必须在Fiber线程中执行
                        await EventSystem.Instance.Invoke<FiberInit, Promise>(sceneType, new FiberInit() {Fiber = fiber});
                        tcs.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"init fiber fail: {sceneType} {e}");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"create fiber error: {fiberId} {sceneType}", e);
            }
        }
        
        /// <summary>
        /// 自动分配 fiberId（从 <c>idGenerator</c> 原子自增，避开 10000000 以下的保留区段）后创建纤程。
        /// </summary>
        public async Promise<int> Create(SchedulerType schedulerType, int zone, int sceneType, string name)
        {
            int fiberId = Interlocked.Increment(ref this.idGenerator);
            return await this.Create(schedulerType, fiberId, zone, sceneType, name);
        }
        
        /// <summary>
        /// 异步移除并释放纤程：把销毁动作 post 到纤程自身线程执行（避免线程竞争），
        /// 从 fibers 映射移除后调用 <see cref="Fiber.Dispose"/> 级联释放 Scene/Entity，最后唤醒等待者。
        /// </summary>
        public async Promise Remove(int id)
        {
            Fiber fiber = this.Get(id);
            TaskCompletionSource<bool> tcs = new();
            // 要扔到fiber线程执行，否则会出现线程竞争
            fiber.ThreadSynchronizationContext.Post(() =>
            {
                if (this.fibers.Remove(id, out Fiber f))
                {
                    f.Dispose();
                }
                tcs.SetResult(true);
            });
            await tcs.Task;
        }

        // 不允许外部调用，容易出现多线程问题, 只能通过消息通信，不允许直接获取其它Fiber引用
        internal Fiber Get(int id)
        {
            this.fibers.TryGetValue(id, out Fiber fiber);
            return fiber;
        }

        /// <summary>当前存活纤程数量。</summary>
        public int Count()
        {
            return this.fibers.Count;
        }
    }
}