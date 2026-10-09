using System;

namespace RD.Core
{
    /// <summary>框架每帧 Update 派发事件体（marker struct，无数据载荷）。</summary>
    public struct UpdateEvent
    {
    }

    /// <summary>实体侧「每帧 Update」标记接口：实体实现此接口表示愿意接收 <see cref="UpdateEvent"/> 派发，由 <c>EntitySystem.Publish&lt;UpdateEvent&gt;</c> 触发。</summary>
    public interface IUpdate: IEvent<UpdateEvent>
    {
    }

    /// <summary>
    /// Update 的 System 抽象基类：实现 <see cref="EventSystem{E, T}.Run"/> 强类型转发到 <see cref="Update(T)"/>。
    /// 派生类被 <see cref="EntitySystemAttribute"/> 标记，<see cref="EntitySystemSingleton"/> 自动注册并由 <c>EntitySystem.Publish&lt;UpdateEvent&gt;</c> 每帧派发。
    /// </summary>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="IUpdate"/>。</typeparam>
    [EntitySystem]
    public abstract class UpdateSystem<T> : EventSystem<T, UpdateEvent> where T: Entity, IUpdate
    {
        protected override void Handle(Entity e, UpdateEvent t)
        {
            this.Update((T)e);
        }

        /// <summary>派生类实现：每帧回调。</summary>
        protected abstract void Update(T self);
    }
}
