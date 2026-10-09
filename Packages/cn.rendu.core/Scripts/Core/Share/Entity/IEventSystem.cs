using System;

namespace RD.Core
{
    /// <summary>
    /// 实体侧事件标记接口：声明某类 Entity 愿意接收并处理结构体事件 <typeparamref name="T"/>。
    /// 由 <c>EntitySystem.Publish&lt;T&gt;</c> 在派发阶段用 <c>is IEvent&lt;T&gt;</c> 过滤目标实体。
    /// 与 Scene 级非泛型 <c>IEvent</c>（事件 handler 端接口）通过泛型 arity 区分。
    /// </summary>
    /// <typeparam name="T">事件数据结构（struct 以避免 GC）。</typeparam>
    public interface IEvent<T>
    {
    }

    /// <summary>
    /// 实体侧事件系统标记接口：所有实体事件系统（<see cref="AEventSystem{T}"/>/<see cref="EventSystem{E, T}"/>）的根接口。
    /// 用于在 <c>EntitySystemSingleton</c> 中以统一基类型登记与判别。
    /// </summary>
    public interface IEventSystem
    {
    }

    /// <summary>
    /// 实体事件系统的处理侧抽象：定义 <c>Run</c> 派发入口，由 <c>EntitySystem.Publish&lt;T&gt;</c> 调用。
    /// <c>in T</c> 逆变使得 <c>AEventSystem&lt;子类&gt;</c> 可安全处理 <c>子类</c> 事件。
    /// </summary>
    /// <typeparam name="T">事件数据结构（struct）。</typeparam>
    public interface AEventSystem<in T>: ISystemType, IEventSystem where T: struct
    {
        void Run(Entity e, T t);
    }

    /// <summary>
    /// 实体事件系统基类：实现 <see cref="AEventSystem{T}.Run"/> 派发到强类型 <see cref="Handle"/>，
    /// 派生类只需重写 <c>Handle(E, T)</c> 编写具体业务处理。
    /// 通过 <c>TypeSystems</c> 按实体类型注册；调用方用 <c>EntitySystem.Publish&lt;T&gt;(message)</c> 触发，
    /// 所有已注册且 <c>is IEvent&lt;T&gt;</c> 的实体都会收到事件。
    /// </summary>
    /// <typeparam name="E">实体类型，需实现 <see cref="IEvent{T}"/>。</typeparam>
    /// <typeparam name="T">事件数据结构（struct）。</typeparam>
    public abstract class EventSystem<E, T> : SystemObject, AEventSystem<T> where E: Entity, IEvent<T> where T: struct
    {
        public void Run(Entity e, T t)
        {
            this.Handle((E)e, t);
        }

        public Type SystemType()
        {
            return typeof(AEventSystem<T>);
        }

        public Type Type()
        {
            return typeof(E);
        }

        /// <summary>派生类实现：接收到事件 <paramref name="t"/> 时的具体业务处理（强类型 <typeparamref name="E"/> 实体）。</summary>
        /// <param name="e">被派发的实体实例。</param>
        /// <param name="t">事件数据。</param>
        protected abstract void Handle(Entity e, T t);
    }
}
