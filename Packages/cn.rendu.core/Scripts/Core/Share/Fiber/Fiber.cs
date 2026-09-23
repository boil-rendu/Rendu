using System;
using System.Collections.Generic;
using System.Threading;

namespace RD.Core
{
    /// <summary>
    /// 纤程相关的扩展与辅助方法。
    /// </summary>
    public static class FiberHelper
    {
        /// <summary>
        /// 由实体推导其全局唯一 <see cref="ActorId"/>：进程号 + 所属纤程 id + 实体 InstanceId，
        /// 供跨纤程消息寻址使用（目标纤程凭 fiber id 路由，InstanceId 定位具体实体）。
        /// </summary>
        public static ActorId GetActorId(this Entity self)
        {
            Fiber root = self.Fiber();
            return new ActorId(root.Process, root.Id, self.InstanceId);
        }
    }
    
    /// <summary>
    /// 纤程（Fiber）：一个独立的 Actor 执行单元，绑定到单一线程运行。
    /// 每个 Fiber 拥有自己的根 Scene、EntitySystem、Mailbox 集合、线程同步上下文与日志器；
    /// 纤程内所有代码都在其所属线程上执行，跨纤程通信只能经由消息（以 ActorId 寻址），禁止直接持有其它 Fiber 引用。
    /// </summary>
    public class Fiber: IDisposable
    {
        // 该字段只能框架使用，绝对不能改成public，改了后果自负
        [StaticField]
        [ThreadStatic]
        public static Fiber Instance;
        
        /// <summary>是否已释放。</summary>
        public bool IsDisposed;
        
        /// <summary>纤程唯一标识（fiber id）。</summary>
        public int Id;

        /// <summary>区服号（Zone）。</summary>
        public int Zone;

        /// <summary>根 Scene，本纤程内所有 Entity 的祖先容器。</summary>
        public Scene Root { get; }

        /// <summary>本纤程的地址（进程号 + fiber id），用于跨纤程寻址。</summary>
        public Address Address
        {
            get
            {
                return new Address(this.Process, this.Id);
            }
        }

        /// <summary>所属进程号（全局唯一，取自启动配置）。</summary>
        public int Process
        {
            get
            {
                return Options.Instance.Process;
            }
        }

        /// <summary>实体系统：负责 Entity 的 Awake/Update/Destroy 等生命周期事件分发。</summary>
        public EntitySystem EntitySystem { get; }
        /// <summary>邮箱集合：本纤程对外暴露的可被消息寻址的 Mailbox。</summary>
        public Mailboxes Mailboxes { get; private set; }
        /// <summary>线程同步上下文：将跨线程回调 post 回本纤程所属线程执行，避免线程竞争。</summary>
        public ThreadSynchronizationContext ThreadSynchronizationContext { get; }
        /// <summary>本纤程的日志器（带 fiber/process/scene 上下文）。</summary>
        public ILog Log { get; }

        /// <summary>帧末（LateUpdate）阶段需要被唤醒的 Promise 队列，由 <see cref="WaitFrameFinish"/> 入队。</summary>
        private readonly Queue<Promise> frameFinishTasks = new();
        
        /// <summary>
        /// 构造一个纤程并初始化其运行时资源（EntitySystem / Mailboxes / 同步上下文 / 日志器），最后创建根 Scene。
        /// </summary>
        internal Fiber(int id, int zone, int sceneType, string name)
        {
            this.Id = id;
            this.Zone = zone;
            this.EntitySystem = new EntitySystem();
            this.Mailboxes = new Mailboxes();
            this.ThreadSynchronizationContext = new ThreadSynchronizationContext();

            LogInvoker logInvoker = new()
                    { Fiber = this.Id, Process = this.Process, SceneName = SceneTypeSingleton.Instance.GetSceneName(sceneType) };
            this.Log = EventSystem.Instance.Invoke<LogInvoker, ILog>(logInvoker);
            
            this.Root = new Scene(this, id, 1, sceneType, name);
        }

        /// <summary>
        /// 帧更新入口：向所有 Entity 广播 <see cref="UpdateEvent"/>，驱动业务逻辑每帧更新。
        /// 异常被捕获并记录到本纤程日志，避免单点错误中断整个帧循环。
        /// </summary>
        internal void Update()
        {
            try
            {
                this.EntitySystem.Publish(new UpdateEvent());
            }
            catch (Exception e)
            {
                this.Log.Error(e);
            }
        }
        
        /// <summary>
        /// 帧末更新入口，按固定顺序执行三件事：
        /// <list type="number">
        ///   <item>广播 <see cref="LateUpdateEvent"/>，供需要在 Update 之后、帧末之前执行的逻辑（如渲染同步）。</item>
        ///   <item><see cref="FrameFinishUpdate"/>：唤醒本帧内通过 <see cref="WaitFrameFinish"/> 等待的 Promise，形成跨帧同步点。</item>
        ///   <item>执行线程同步上下文中的回帖（post 回来的跨线程回调），确保它们在纤程线程上运行。</item>
        /// </list>
        /// </summary>
        internal void LateUpdate()
        {
            try
            {
                this.EntitySystem.Publish(new LateUpdateEvent());
                FrameFinishUpdate();
                
                this.ThreadSynchronizationContext.Update();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        /// <summary>
        /// 注册一个"等待本帧帧末"的 Promise。调用方 await 后会在 <see cref="LateUpdate"/> 的 <see cref="FrameFinishUpdate"/> 阶段被唤醒，
        /// 常用于需要在同一帧的 Update 全部完成后再继续的跨帧同步场景。
        /// </summary>
        public async Promise WaitFrameFinish()
        {
            Promise task = Promise.Create(true);
            this.frameFinishTasks.Enqueue(task);
            await task;
        }

        /// <summary>
        /// 帧末批量唤醒：将 <see cref="frameFinishTasks"/> 中所有等待的 Promise 出队并置为完成，
        /// 使 <see cref="WaitFrameFinish"/> 的调用方在本帧 LateUpdate 末尾恢复执行。
        /// </summary>
        private void FrameFinishUpdate()
        {
            while (this.frameFinishTasks.Count > 0)
            {
                Promise task = this.frameFinishTasks.Dequeue();
                task.SetResult();
            }
        }

        /// <summary>
        /// 释放纤程：先标记已释放（防止重入），再释放根 Scene 以级联销毁其下所有 Entity。
        /// 注意：EntitySystem / Mailboxes 等随 Scene 树与纤程生命周期自然结束，不在此单独释放。
        /// </summary>
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.IsDisposed = true;
            
            this.Root.Dispose();
        }
    }
}