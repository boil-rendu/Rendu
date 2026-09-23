using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIOpenParam
    {
    }

    public interface IUIOpen
    {
    }

    public interface IUIOpen<A> : IUIOpenParam
    {
    }

    public interface IUIOpen<A, B> : IUIOpenParam
    {
    }

    public interface IUIOpen<A, B, C> : IUIOpenParam
    {
    }

    public interface IUIOpen<A, B, C, D> : IUIOpenParam
    {
    }

    public interface IUIOpen<A, B, C, D, E> : IUIOpenParam
    {
    }

    public interface IUIOpenSystem : ISystemType
    {
        Promise<bool> Run(Entity o);
    }

    public interface IUIOpenSystem<A> : ISystemType
    {
        Promise<bool> Run(Entity o, A a);
    }

    public interface IUIOpenSystem<A, B> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b);
    }

    public interface IUIOpenSystem<A, B, C> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b, C c);
    }

    public interface IUIOpenSystem<A, B, C, D> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b, C c, D d);
    }

    public interface IUIOpenSystem<A, B, C, D, E> : ISystemType
    {
        Promise<bool> Run(Entity o, A a, B b, C c, D d, E e);
    }

    [EntitySystem]
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