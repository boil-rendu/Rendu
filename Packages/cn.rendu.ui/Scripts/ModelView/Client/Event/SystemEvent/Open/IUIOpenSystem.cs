using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>打开参数标记接口（各 IUIOpen<...> 都继承它）</summary>
    public interface IUIOpenParam
    {
    }

    /// <summary>标记接口：实现它的 UI 实体可被打开（无参）</summary>
    public interface IUIOpen
    {
    }

    /// <summary>标记接口：可被打开（1 参）</summary>
    public interface IUIOpen<A> : IUIOpenParam
    {
    }

    /// <summary>标记接口：可被打开（2 参）</summary>
    public interface IUIOpen<A, B> : IUIOpenParam
    {
    }

    /// <summary>标记接口：可被打开（3 参）</summary>
    public interface IUIOpen<A, B, C> : IUIOpenParam
    {
    }

    /// <summary>标记接口：可被打开（4 参）</summary>
    public interface IUIOpen<A, B, C, D> : IUIOpenParam
    {
    }

    /// <summary>标记接口：可被打开（5 参）</summary>
    public interface IUIOpen<A, B, C, D, E> : IUIOpenParam
    {
    }

    /// <summary>打开系统契约（0 参），Run 返回是否允许打开</summary>
    public interface IUIOpenSystem : ISystemType
    {
        Promise<bool> Run(Entity o);
    }

    /// <summary>打开系统契约（1 参），Run 返回是否允许打开</summary>
    public interface IUIOpenSystem<A> : ISystemType
    {
        Promise<bool> Run(Entity o, A a);
    }

    /// <summary>打开系统契约（2 参），Run 返回是否允许打开</summary>
    public interface IUIOpenSystem<A, B> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b);
    }

    /// <summary>打开系统契约（3 参），Run 返回是否允许打开</summary>
    public interface IUIOpenSystem<A, B, C> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b, C c);
    }

    /// <summary>打开系统契约（4 参），Run 返回是否允许打开</summary>
    public interface IUIOpenSystem<A, B, C, D> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b, C c, D d);
    }

    /// <summary>打开系统契约（5 参），Run 返回是否允许打开</summary>
    public interface IUIOpenSystem<A, B, C, D, E> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b, C c, D d, E e);
    }

    [EntitySystem]
    /// <summary>打开系统抽象基类（0 参；EntitySystem 自动注册，子类实现 UIOpen）</summary>
    public abstract class UIOpenSystem<T> : SystemObject, IUIOpenSystem where T : Entity, IUIOpen
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenSystem);
        }

        async Promise<bool> IUIOpenSystem.Run(Entity o)
        {
            return await this.UIOpen((T)o);
        }

        protected abstract Promise<bool> UIOpen(T self);
    }

    [EntitySystem]
    /// <summary>打开系统抽象基类（1 参；EntitySystem 自动注册，子类实现 UIOpen）</summary>
    public abstract class UIOpenSystem<T, A> : SystemObject, IUIOpenSystem<A> where T : Entity, IUIOpen<A>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenSystem<A>);
        }

        async Promise<bool> IUIOpenSystem<A>.Run(Entity o, A a)
        {
            return await this.UIOpen((T)o, a);
        }

        protected abstract Promise<bool> UIOpen(T self, A a);
    }

    [EntitySystem]
    /// <summary>打开系统抽象基类（2 参；EntitySystem 自动注册，子类实现 UIOpen）</summary>
    public abstract class UIOpenSystem<T, A, B> : SystemObject, IUIOpenSystem<A, B> where T : Entity, IUIOpen<A, B>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenSystem<A, B>);
        }

        async Promise<bool> IUIOpenSystem<A, B>.Run(Entity o, A a, B b)
        {
            return await this.UIOpen((T)o, a, b);
        }

        protected abstract Promise<bool> UIOpen(T self, A a, B b);
    }

    [EntitySystem]
    /// <summary>打开系统抽象基类（3 参；EntitySystem 自动注册，子类实现 UIOpen）</summary>
    public abstract class UIOpenSystem<T, A, B, C> : SystemObject, IUIOpenSystem<A, B, C> where T : Entity, IUIOpen<A, B, C>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenSystem<A, B, C>);
        }

        async Promise<bool> IUIOpenSystem<A, B, C>.Run(Entity o, A a, B b, C c)
        {
            return await this.UIOpen((T)o, a, b, c);
        }

        protected abstract Promise<bool> UIOpen(T self, A a, B b, C c);
    }

    [EntitySystem]
    /// <summary>打开系统抽象基类（4 参；EntitySystem 自动注册，子类实现 UIOpen）</summary>
    public abstract class UIOpenSystem<T, A, B, C, D> : SystemObject, IUIOpenSystem<A, B, C, D> where T : Entity, IUIOpen<A, B, C, D>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenSystem<A, B, C, D>);
        }

        async Promise<bool> IUIOpenSystem<A, B, C, D>.Run(Entity o, A a, B b, C c, D d)
        {
            return await this.UIOpen((T)o, a, b, c, d);
        }

        protected abstract Promise<bool> UIOpen(T self, A a, B b, C c, D d);
    }

    [EntitySystem]
    /// <summary>打开系统抽象基类（5 参；EntitySystem 自动注册，子类实现 UIOpen）</summary>
    public abstract class UIOpenSystem<T, A, B, C, D, E> : SystemObject, IUIOpenSystem<A, B, C, D, E> where T : Entity, IUIOpen<A, B, C, D, E>
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenSystem<A, B, C, D, E>);
        }

        async Promise<bool> IUIOpenSystem<A, B, C, D, E>.Run(Entity o, A a, B b, C c, D d, E e)
        {
            return await this.UIOpen((T)o, a, b, c, d, e);
        }

        protected abstract Promise<bool> UIOpen(T self, A a, B b, C c, D d, E e);
    }
}