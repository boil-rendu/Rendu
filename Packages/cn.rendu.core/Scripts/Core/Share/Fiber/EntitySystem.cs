using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 实体事件派发器（<see cref="EntitySystemSingleton"/> 的运行时搭档）：为每个事件 <c>SystemType</c> 维护一个
    /// 实体弱引用队列，<see cref="Publish{T}"/> 派发时按队列顺序逐个调用匹配的 <see cref="AEventSystem{T}"/>。
    /// 队列按 <c>EventSystem</c> 注册到该事件的所有实体排列，循环轮转实现「本帧内每个实体都收到一次事件」的语义。
    /// 持有的是 <see cref="EntityRef{T}"/>（强类型句柄）而非 <see cref="Entity"/> 直接引用，可感知 <c>InstanceId</c> 变化并跳过已回收的实体。
    /// </summary>
    public class EntitySystem
    {
        private readonly Dictionary<Type, Queue<EntityRef<Entity>>> queues = new();

        /// <summary>按 <paramref name="type"/>（事件 <c>SystemType</c>）取出或新建一个实体队列。</summary>
        /// <param name="type">事件 <c>SystemType</c>，如 <c>typeof(AEventSystem&lt;UpdateEvent&gt;)</c>。</param>
        public Queue<EntityRef<Entity>> GetQueue(Type type)
        {
            if (!this.queues.TryGetValue(type, out var queue))
            {
                queue = new Queue<EntityRef<Entity>>();
                this.queues.Add(type, queue);
            }

            return queue;
        }

        /// <summary>把实体注册到它实现的所有 <c>IEvent&lt;T&gt;</c> 对应的队列中（由 <see cref="Entity.RegisterSystem"/> 在注册时调用）。</summary>
        /// <remarks>只会注册到该实体类型在 <see cref="EntitySystemSingleton.TypeSystems"/> 的 <c>ClassType</c> 集合里的事件类型；尚未被任何 <see cref="EntitySystem"/> 关注的实体不会被入队。</remarks>
        public virtual void RegisterSystem(Entity component)
        {
            Type type = component.GetType();

            TypeSystems.OneTypeSystems oneTypeSystems = EntitySystemSingleton.Instance.TypeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }

            foreach (Type queueType in oneTypeSystems.ClassType)
            {
                var queue = this.GetQueue(queueType);
                queue.Enqueue(component);
            }
        }

        /// <summary>派发事件 <typeparamref name="T"/>：从队列头依次取出订阅了该事件的实体并执行其 <see cref="AEventSystem{T}.Run"/>，处理完重新入队形成轮转。</summary>
        /// <remarks>
        /// 每轮从队列中取一个实体，处理后立即重新入队，确保本帧仍订阅该事件的所有实体都收到一次。
        /// 跳过已 <c>null</c>、已 <c>IsDisposed</c>、或已不再实现 <see cref="IEvent{T}"/> 的实体。
        /// </remarks>
        public void Publish<T>(T t) where T: struct
        {
            using var _ = Profiler.Section("EntitySystem.Publish");
            Type systemType = typeof(AEventSystem<T>);

            Queue<EntityRef<Entity>> queue = this.GetQueue(systemType);
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }
                if (component.IsDisposed)
                {
                    continue;
                }
                if (component is not IEvent<T>)
                {
                    continue;
                }
                try
                {
                    Type componentType = component.GetType();

                    List<SystemObject> systems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(componentType, systemType);

                    if (systems == null)
                    {
                        continue;
                    }

                    queue.Enqueue(component);

                    foreach (AEventSystem<T> eventSystem in systems)
                    {
                        try
                        {
                            eventSystem.Run(component, t);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"entity system update fail: {component.GetType().FullName}", e);
                }

            }
        }
    }
}
