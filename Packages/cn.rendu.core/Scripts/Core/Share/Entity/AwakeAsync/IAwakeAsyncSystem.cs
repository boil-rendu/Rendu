using System;

namespace RD.Core
{
    /// <summary>实体侧「无参异步 Awake」标记接口：实现此接口的实体在 <c>AddComponentAsync</c>/<c>AddChildAsync</c> 时会 <c>await</c> 其 <see cref="AwakeAsyncSystem{T}.AwakeAsync(T)"/> 钩子。</summary>
    public interface IAwakeAsync
    {
    }

    /// <summary>实体侧「1 参异步 Awake」标记接口。</summary>
    public interface IAwakeAsync<A>
    {
    }

    /// <summary>实体侧「2 参异步 Awake」标记接口。</summary>
    public interface IAwakeAsync<A, B>
    {
    }

    /// <summary>实体侧「3 参异步 Awake」标记接口。</summary>
    public interface IAwakeAsync<A, B, C>
    {
    }

    /// <summary>实体侧「4 参异步 Awake」标记接口。</summary>
    public interface IAwakeAsync<A, B, C, D>
    {
    }

    /// <summary>实体侧「5 参异步 Awake」标记接口。</summary>
    public interface IAwakeAsync<A, B, C, D, E>
    {
    }

    /// <summary>无参异步 Awake 的 System 处理侧接口：<see cref="Run"/> 派发入口。</summary>
    public interface IAwakeAsyncSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    /// <summary>1 参异步 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeAsyncSystem<A> : ISystemType
    {
        Promise Run(Entity o, A a);
    }

    /// <summary>2 参异步 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeAsyncSystem<A, B> : ISystemType
    {
        Promise Run(Entity o, A a, B b);
    }

    /// <summary>3 参异步 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeAsyncSystem<A, B, C> : ISystemType
    {
        Promise Run(Entity o, A a, B b, C c);
    }

    /// <summary>4 参异步 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeAsyncSystem<A, B, C, D> : ISystemType
    {
        Promise Run(Entity o, A a, B b, C c, D d);
    }

    /// <summary>5 参异步 Awake 的 System 处理侧接口。</summary>
    public interface IAwakeAsyncSystem<A, B, C, D, E> : ISystemType
    {
        Promise Run(Entity o, A a, B b, C c, D d, E e);
    }

    /// <summary>
    /// 无参异步 Awake 的 System 抽象基类：<see cref="IAwakeAsyncSystem.Run"/> 转发到 <see cref="AwakeAsync(T)"/>。
    /// </summary>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="IAwakeAsync"/>。</typeparam>
    [EntitySystem]
    public abstract class AwakeAsyncSystem<T> : SystemObject, IAwakeAsyncSystem where T : Entity, IAwakeAsync
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeAsyncSystem);
        }

        async Promise IAwakeAsyncSystem.Run(Entity o)
        {
            await this.AwakeAsync((T)o);
        }

        /// <summary>派生类实现：异步初始化（无参）。</summary>
        protected abstract Promise AwakeAsync(T self);
    }

    /// <summary>1 参异步 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeAsyncSystem<T, A> : SystemObject, IAwakeAsyncSystem<A> where T : Entity, IAwakeAsync<A>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeAsyncSystem<A>);
        }

        async Promise IAwakeAsyncSystem<A>.Run(Entity o, A a)
        {
            await this.AwakeAsync((T)o, a);
        }

        /// <summary>派生类实现：异步初始化（1 参）。</summary>
        protected abstract Promise AwakeAsync(T self, A a);
    }

    /// <summary>2 参异步 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeAsyncSystem<T, A, B> : SystemObject, IAwakeAsyncSystem<A, B> where T : Entity, IAwakeAsync<A, B>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeAsyncSystem<A, B>);
        }

        async Promise IAwakeAsyncSystem<A, B>.Run(Entity o, A a, B b)
        {
            await this.AwakeAsync((T)o, a, b);
        }

        /// <summary>派生类实现：异步初始化（2 参）。</summary>
        protected abstract Promise AwakeAsync(T self, A a, B b);
    }

    /// <summary>3 参异步 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeAsyncSystem<T, A, B, C> : SystemObject, IAwakeAsyncSystem<A, B, C> where T : Entity, IAwakeAsync<A, B, C>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeAsyncSystem<A, B, C>);
        }

        async Promise IAwakeAsyncSystem<A, B, C>.Run(Entity o, A a, B b, C c)
        {
            await this.AwakeAsync((T)o, a, b, c);
        }

        /// <summary>派生类实现：异步初始化（3 参）。</summary>
        protected abstract Promise AwakeAsync(T self, A a, B b, C c);
    }

    /// <summary>4 参异步 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeAsyncSystem<T, A, B, C, D> : SystemObject, IAwakeAsyncSystem<A, B, C, D> where T : Entity, IAwakeAsync<A, B, C, D>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeAsyncSystem<A, B, C, D>);
        }

        async Promise IAwakeAsyncSystem<A, B, C, D>.Run(Entity o, A a, B b, C c, D d)
        {
            await this.AwakeAsync((T)o, a, b, c, d);
        }

        /// <summary>派生类实现：异步初始化（4 参）。</summary>
        protected abstract Promise AwakeAsync(T self, A a, B b, C c, D d);
    }

    /// <summary>5 参异步 Awake 的 System 抽象基类。</summary>
    [EntitySystem]
    public abstract class AwakeAsyncSystem<T, A, B, C, D, E> : SystemObject, IAwakeAsyncSystem<A, B, C, D, E> where T : Entity, IAwakeAsync<A, B, C, D, E>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IAwakeAsyncSystem<A, B, C, D, E>);
        }

        async Promise IAwakeAsyncSystem<A, B, C, D, E>.Run(Entity o, A a, B b, C c, D d, E e)
        {
            await this.AwakeAsync((T)o, a, b, c, d, e);
        }

        /// <summary>派生类实现：异步初始化（5 参）。</summary>
        protected abstract Promise AwakeAsync(T self, A a, B b, C c, D d, E e);
    }
}
