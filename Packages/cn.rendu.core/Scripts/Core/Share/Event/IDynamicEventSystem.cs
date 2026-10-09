using System;

namespace RD.Core
{
    /// <summary>
    /// 实体侧的「动态事件」标记接口：声明某类 Entity 愿意接收并处理结构体事件 <typeparamref name="A"/>。
    /// 与 <see cref="IEvent{A}"/> 配合使用（继承自它），但与框架内置的 <c>IUpdate</c>/<c>ILateUpdate</c> 不同，
    /// 该事件没有专门的 <c>Run</c> 钩子，由业务通过 <c>EntitySystem.Publish</c> 在需要时主动派发，
    /// 派生类通过 <see cref="DynamicEventSystem{T, A}"/> 实现具体的 <c>DynamicEvent</c> 处理逻辑。
    /// </summary>
    /// <typeparam name="A">事件数据结构（struct 以避免 GC）。</typeparam>
    public interface IDynamicEvent<A> : IEvent<A> where A : struct
    {
    }

    /// <summary>
    /// 实体动态事件系统的处理侧接口：由 <see cref="DynamicEventSystem{T, A}"/> 实现并被
    /// <c>EntitySystemSingleton</c> 注册到 <c>TypeSystems</c>，<c>EntitySystem_PublishDynamic</c>
    /// 派发时按此 <c>SystemType</c> 索引到对应处理函数并调用 <see cref="Run"/>。
    /// </summary>
    /// <typeparam name="A">事件数据结构（struct）。</typeparam>
    public interface IDynamicEventSystem<A> : ISystemType where A : struct
    {
        Promise Run(Entity o, A message);
    }

    /// <summary>
    /// 实体动态事件系统基类：实现 <see cref="IDynamicEventSystem{A}"/> 的 <c>Run</c> 派发，
    /// 派生类只需重写 <see cref="DynamicEvent"/> 编写具体业务处理。
    /// 通过 <c>[EntitySystem]</c> 特性在源生成阶段被自动注册到 <c>EntitySystemSingleton</c>，
    /// 调用方用 <c>EntitySystem.Publish&lt;A&gt;(entity, message)</c> 触发该实体的对应处理。
    /// </summary>
    /// <typeparam name="T">实体类型，需实现 <see cref="IDynamicEvent{A}"/>。</typeparam>
    /// <typeparam name="A">事件数据结构（struct）。</typeparam>
    [EntitySystem]
    public abstract class DynamicEventSystem<T, A> : EventSystem<T, A>, IDynamicEventSystem<A>
            where T : Entity, IDynamicEvent<A> where A : struct
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IDynamicEventSystem<A>);
        }

        public new async Promise Run(Entity o, A message)
        {
            await DynamicEvent((T)o, message);
        }

        protected override void Handle(Entity e, A t)
        {
            throw new NotImplementedException();
        }

        /// <summary>派生类实现：接收到事件 <paramref name="message"/> 时的具体业务处理。</summary>
        /// <param name="self">被派发的实体实例。</param>
        /// <param name="message">事件数据。</param>
        protected abstract Promise DynamicEvent(T self, A message);
    }
}
