using System;

namespace RD.Core
{
    /// <summary>框架每帧 LateUpdate 派发事件体（marker struct，无数据载荷）。</summary>
    public struct LateUpdateEvent
    {
    }

    /// <summary>实体侧「每帧 LateUpdate」标记接口：实体实现此接口表示愿意接收 <see cref="LateUpdateEvent"/> 派发，由 <c>EntitySystem.Publish&lt;LateUpdateEvent&gt;</c> 在所有 Update 之后触发。</summary>
    public interface ILateUpdate: IEvent<LateUpdateEvent>
    {
    }

    /// <summary>
    /// LateUpdate 的 System 抽象基类：实现 <see cref="EventSystem{E, T}.Run"/> 强类型转发到 <see cref="LateUpdate(T)"/>。
    /// 派生类被 <see cref="EntitySystemAttribute"/> 标记，由 <c>EntitySystem.Publish&lt;LateUpdateEvent&gt;</c> 每帧派发（晚于 <see cref="UpdateSystem{T}"/>）。
    /// </summary>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="ILateUpdate"/>。</typeparam>
    [EntitySystem]
    public abstract class LateUpdateSystem<T> : EventSystem<T, LateUpdateEvent> where T: Entity, ILateUpdate
    {
        protected override void Handle(Entity e, LateUpdateEvent t)
        {
            this.LateUpdate((T)e);
        }

        /// <summary>派生类实现：每帧 LateUpdate 回调。</summary>
        protected abstract void LateUpdate(T self);
    }
}
