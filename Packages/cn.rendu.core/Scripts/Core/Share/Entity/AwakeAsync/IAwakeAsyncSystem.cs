using System;

namespace RD.Core
{
    public interface IAwakeAsync
    {
    }

    public interface IAwakeAsync<A>
    {
    }

    public interface IAwakeAsync<A, B>
    {
    }

    public interface IAwakeAsync<A, B, C>
    {
    }

    public interface IAwakeAsync<A, B, C, D>
    {
    }

    public interface IAwakeAsync<A, B, C, D, E>
    {
    }

    public interface IAwakeAsyncSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    public interface IAwakeAsyncSystem<A> : ISystemType
    {
        Promise Run(Entity o, A a);
    }

    public interface IAwakeAsyncSystem<A, B> : ISystemType
    {
        Promise Run(Entity o, A a, B b);
    }

    public interface IAwakeAsyncSystem<A, B, C> : ISystemType
    {
        Promise Run(Entity o, A a, B b, C c);
    }

    public interface IAwakeAsyncSystem<A, B, C, D> : ISystemType
    {
        Promise Run(Entity o, A a, B b, C c, D d);
    }

    public interface IAwakeAsyncSystem<A, B, C, D, E> : ISystemType
    {
        Promise Run(Entity o, A a, B b, C c, D d, E e);
    }

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

        protected abstract Promise AwakeAsync(T self);
    }

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

        protected abstract Promise AwakeAsync(T self, A a);
    }

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

        protected abstract Promise AwakeAsync(T self, A a, B b);
    }

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

        protected abstract Promise AwakeAsync(T self, A a, B b, C c);
    }

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

        protected abstract Promise AwakeAsync(T self, A a, B b, C c, D d);
    }

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

        protected abstract Promise AwakeAsync(T self, A a, B b, C c, D d, E e);
    }
}