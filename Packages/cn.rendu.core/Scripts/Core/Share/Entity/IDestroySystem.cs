using System;

namespace RD.Core
{
    /// <summary>实体侧 Destroy 标记接口：实体实现此接口表示在销毁时需要清理，由 <see cref="EntitySystemSingleton.Destroy(Entity)"/> 派发。</summary>
    public interface IDestroy
    {
    }

    /// <summary>Destroy 的 System 处理侧接口：<see cref="Run"/> 派发入口。</summary>
    public interface IDestroySystem: ISystemType
    {
        void Run(Entity o);
    }

    /// <summary>
    /// Destroy 的 System 抽象基类：实现 <see cref="IDestroySystem.Run"/> 强类型转发到 <see cref="Destroy(T)"/>。
    /// 派生类被 <see cref="EntitySystemAttribute"/> 标记，<see cref="EntitySystemSingleton"/> 自动注册并派发。
    /// </summary>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="IDestroy"/>。</typeparam>
    [EntitySystem]
    public abstract class DestroySystem<T> : SystemObject, IDestroySystem where T: Entity, IDestroy
    {
        void IDestroySystem.Run(Entity o)
        {
            this.Destroy((T)o);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IDestroySystem);
        }

        Type ISystemType.Type()
        {
            return typeof(T);
        }

        /// <summary>派生类实现：实体销毁时的清理逻辑。</summary>
        protected abstract void Destroy(T self);
    }
}
