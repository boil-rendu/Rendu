using System;

namespace RD.Core
{
    /// <summary>实体侧「无参 Awake」标记接口：实体实现此接口表示愿意在初始化时接收无参回调，由 <see cref="EntitySystemSingleton.Awake(Entity)"/> 派发。</summary>
    public interface IAwake
    {
    }

    /// <summary>实体侧「1 参 Awake」标记接口，参数 <typeparamref name="A"/> 携带业务上下文。</summary>
    public interface IAwake<A>
    {
    }

    /// <summary>实体侧「2 参 Awake」标记接口。</summary>
    public interface IAwake<A, B>
    {
    }

    /// <summary>实体侧「3 参 Awake」标记接口。</summary>
    public interface IAwake<A, B, C>
    {
    }

    /// <summary>实体侧「4 参 Awake」标记接口。</summary>
    public interface IAwake<A, B, C, D>
    {
    }

    /// <summary>实体侧「5 参 Awake」标记接口。</summary>
    public interface IAwake<A, B, C, D, E>
    {
    }

    /// <summary>无参 Awake 的 System 处理侧接口：<see cref="Run"/> 派发入口。</summary>
    public interface IAwakeSystem: ISystemType
    {
        void Run(Entity o);
    }

    /// <summary>1 参 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeSystem<A>: ISystemType
    {
        void Run(Entity o, A a);
    }

    /// <summary>2 参 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeSystem<A, B>: ISystemType
    {
        void Run(Entity o, A a, B b);
    }

    /// <summary>3 参 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeSystem<A, B, C>: ISystemType
    {
        void Run(Entity o, A a, B b, C c);
    }

    /// <summary>4 参 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeSystem<A, B, C, D>: ISystemType
    {
        void Run(Entity o, A a, B b, C c, D d);
    }

    /// <summary>5 参 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeSystem<A, B, C, D, E>: ISystemType
    {
        void Run(Entity o, A a, B b, C c, D d, E e);
    }

    /// <summary>
    /// 无参 Awake 的 System 抽象基类：实现 <see cref="IAwakeSystem.Run"/> 强类型转发到 <see cref="Awake(T)"/>。
    /// 派生类被 <see cref="EntitySystemAttribute"/> 标记，<see cref="EntitySystemSingleton"/> 自动注册并派发。
    /// </summary>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="IAwake"/>。</typeparam>
    [EntitySystem]
    public abstract class AwakeSystem<T> : SystemObject, IAwakeSystem where T: Entity, IAwake
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem);
        }

        void IAwakeSystem.Run(Entity o)
        {
            this.Awake((T)o);
        }

        /// <summary>派生类实现：实体初始化回调（无参）。</summary>
        protected abstract void Awake(T self);
    }

    /// <summary>1 参 Awake 的 System 抽象基类。</summary>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="IAwake{A}"/>。</typeparam>
    /// <typeparam name="A">参数类型。</typeparam>
    [EntitySystem]
    public abstract class AwakeSystem<T, A> : SystemObject, IAwakeSystem<A> where T: Entity, IAwake<A>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A>);
        }

        void IAwakeSystem<A>.Run(Entity o, A a)
        {
            this.Awake((T)o, a);
        }

        /// <summary>派生类实现：实体初始化回调（1 参）。</summary>
        protected abstract void Awake(T self, A a);
    }

    /// <summary>2 参 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeSystem<T, A, B> : SystemObject, IAwakeSystem<A, B> where T: Entity, IAwake<A, B>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B>);
        }

        void IAwakeSystem<A, B>.Run(Entity o, A a, B b)
        {
            this.Awake((T)o, a, b);
        }

        /// <summary>派生类实现：实体初始化回调（2 参）。</summary>
        protected abstract void Awake(T self, A a, B b);
    }

    /// <summary>3 参 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeSystem<T, A, B, C> : SystemObject, IAwakeSystem<A, B, C> where T: Entity, IAwake<A, B, C>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B, C>);
        }

        void IAwakeSystem<A, B, C>.Run(Entity o, A a, B b, C c)
        {
            this.Awake((T)o, a, b, c);
        }

        /// <summary>派生类实现：实体初始化回调（3 参）。</summary>
        protected abstract void Awake(T self, A a, B b, C c);
    }

    /// <summary>4 参 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeSystem<T, A, B, C, D> : SystemObject, IAwakeSystem<A, B, C, D> where T: Entity, IAwake<A, B, C, D>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B, C, D>);
        }

        void IAwakeSystem<A, B, C, D>.Run(Entity o, A a, B b, C c, D d)
        {
            this.Awake((T)o, a, b, c, d);
        }

        /// <summary>派生类实现：实体初始化回调（4 参）。</summary>
        protected abstract void Awake(T self, A a, B b, C c, D d);
    }

    /// <summary>5 参 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeSystem<T, A, B, C, D, E> : SystemObject, IAwakeSystem<A, B, C, D, E> where T: Entity, IAwake<A, B, C, D, E>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeSystem<A, B, C, D, E>);
        }

        void IAwakeSystem<A, B, C, D, E>.Run(Entity o, A a, B b, C c, D d, E e)
        {
            this.Awake((T)o, a, b, c, d, e);
        }

        /// <summary>派生类实现：实体初始化回调（5 参）。</summary>
        protected abstract void Awake(T self, A a, B b, C c, D d, E e);
    }
}
