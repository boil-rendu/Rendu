using System;

namespace RD.Core
{
    /// <summary>
    /// <see cref="Entity"/> 的异步 Component/Child 增删扩展（partial）。
    /// 所有 <c>AddComponentAsync</c>/<c>AddChildAsync</c> 在创建后均会通过 <see cref="EntityAwakeAsyncSystem"/> 派发异步 Awake 钩子（<c>await</c>），确保 <see cref="IAwakeAsync"/> 实现类的初始化全部完成。
    /// </summary>
    public abstract partial class Entity
    {
        /// <summary>异步添加 Component（无参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<Entity> AddComponentAsync(Type type, bool isFromPool = false)
        {
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef);
            return componentRef;
        }

        /// <summary>异步添加 Component 并指定业务 id（无参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<K> AddComponentWithIdAsync<K>(long id, bool isFromPool = false) where K : Entity, IAwakeAsync, new()
        {
            var type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef);
            return (K)componentRef.Entity;
        }

        /// <summary>异步添加 Component 并指定业务 id（1 参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<K> AddComponentWithIdAsync<K, P1>(long id, P1 p1, bool isFromPool = false) where K : Entity, IAwakeAsync<P1>, new()
        {
            var type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, p1);
            return (K)componentRef.Entity;
        }

        /// <summary>异步添加 Component 并指定业务 id（2 参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<K> AddComponentWithIdAsync<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2>, new()
        {
            var type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, p1, p2);
            return (K)componentRef.Entity;
        }

        /// <summary>异步添加 Component 并指定业务 id（3 参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<K> AddComponentWithIdAsync<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2, P3>, new()
        {
            var type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, p1, p2, p3);
            return (K)componentRef.Entity;
        }

        /// <summary>异步添加 Component 并指定业务 id（4 参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<K> AddComponentWithIdAsync<K, P1, P2, P3, P4>(long id, P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2, P3, P4>, new()
        {
            var type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, p1, p2, p3, p4);
            return (K)componentRef.Entity;
        }

        /// <summary>异步添加 Component 并指定业务 id（5 参 Awake），Component 已存在则抛异常。</summary>
        public async Promise<K> AddComponentWithIdAsync<K, P1, P2, P3, P4, P5>(long id, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2, P3, P4, P5>, new()
        {
            var type = typeof(K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"实体组件已存在： {type.FullName}");
            }

            var component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, p1, p2, p3, p4, p5);
            return (K)componentRef.Entity;
        }

        /// <summary>异步添加 Component（无参 Awake），id 取自自身。</summary>
        public async Promise<K> AddComponentAsync<K>(bool isFromPool = false) where K : Entity, IAwakeAsync, new()
        {
            return await this.AddComponentWithIdAsync<K>(this.Id, isFromPool);
        }

        /// <summary>异步添加 Component（1 参 Awake），id 取自自身。</summary>
        public async Promise<K> AddComponentAsync<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwakeAsync<P1>, new()
        {
            return await this.AddComponentWithIdAsync<K, P1>(this.Id, p1, isFromPool);
        }

        /// <summary>异步添加 Component（2 参 Awake），id 取自自身。</summary>
        public async Promise<K> AddComponentAsync<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2>, new()
        {
            return await this.AddComponentWithIdAsync<K, P1, P2>(this.Id, p1, p2, isFromPool);
        }

        /// <summary>异步添加 Component（3 参 Awake），id 取自自身。</summary>
        public async Promise<K> AddComponentAsync<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2, P3>, new()
        {
            return await this.AddComponentWithIdAsync<K, P1, P2, P3>(this.Id, p1, p2, p3, isFromPool);
        }

        /// <summary>异步添加 Component（4 参 Awake），id 取自自身。</summary>
        public async Promise<K> AddComponentAsync<K, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2, P3, P4>, new()
        {
            return await this.AddComponentWithIdAsync<K, P1, P2, P3, P4>(this.Id, p1, p2, p3, p4, isFromPool);
        }

        /// <summary>异步添加 Component（5 参 Awake），id 取自自身。</summary>
        public async Promise<K> AddComponentAsync<K, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, bool isFromPool = false) where K : Entity, IAwakeAsync<P1, P2, P3, P4, P5>, new()
        {
            return await this.AddComponentWithIdAsync<K, P1, P2, P3, P4, P5>(this.Id, p1, p2, p3, p4, p5, isFromPool);
        }

        /// <summary>异步添加 Child Entity（无参 Awake），id 自动生成。</summary>
        public async Promise<Entity> AddChildAsync(Type type, bool isFromPool = false)
        {
            return await AddChildWithIdAsync(type, IdGenerator.Instance.GenerateId(), isFromPool);
        }

        /// <summary>异步添加 Child Entity（无参 Awake），id 自动生成。</summary>
        public async Promise<T> AddChildAsync<T>(bool isFromPool = false) where T : Entity, IAwakeAsync
        {
            return await AddChildWithIdAsync<T>(IdGenerator.Instance.GenerateId(), isFromPool);
        }

        /// <summary>异步添加 Child Entity（1 参 Awake），id 自动生成。</summary>
        public async Promise<T> AddChildAsync<T, A>(A a, bool isFromPool = false) where T : Entity, IAwakeAsync<A>
        {
            return await AddChildWithIdAsync<T, A>(IdGenerator.Instance.GenerateId(), a, isFromPool);
        }

        /// <summary>异步添加 Child Entity（2 参 Awake），id 自动生成。</summary>
        public async Promise<T> AddChildAsync<T, A, B>(A a, B b, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B>
        {
            return await AddChildWithIdAsync<T, A, B>(IdGenerator.Instance.GenerateId(), a, b, isFromPool);
        }

        /// <summary>异步添加 Child Entity（3 参 Awake），id 自动生成。</summary>
        public async Promise<T> AddChildAsync<T, A, B, C>(A a, B b, C c, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B, C>
        {
            return await AddChildWithIdAsync<T, A, B, C>(IdGenerator.Instance.GenerateId(), a, b, c, isFromPool);
        }

        /// <summary>异步添加 Child Entity（4 参 Awake），id 自动生成。</summary>
        public async Promise<T> AddChildAsync<T, A, B, C, D>(A a, B b, C c, D d, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B, C, D>
        {
            return await AddChildWithIdAsync<T, A, B, C, D>(IdGenerator.Instance.GenerateId(), a, b, c, d, isFromPool);
        }

        /// <summary>异步添加 Child Entity（5 参 Awake），id 自动生成。</summary>
        public async Promise<T> AddChildAsync<T, A, B, C, D, E>(A a, B b, C c, D d, E e, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B, C, D, E>
        {
            return await AddChildWithIdAsync<T, A, B, C, D, E>(IdGenerator.Instance.GenerateId(), a, b, c, d, e, isFromPool);
        }

        /// <summary>异步添加 Child Entity（无参 Awake）并指定 id。</summary>
        public async Promise<Entity> AddChildWithIdAsync(Type type, long id, bool isFromPool = false)
        {
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef);
            return componentRef.Entity;
        }

        /// <summary>异步添加 Child Entity（无参 Awake）并指定 id。</summary>
        public async Promise<T> AddChildWithIdAsync<T>(long id, bool isFromPool = false) where T : Entity, IAwakeAsync
        {
            var type = typeof(T);
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef);
            return (T)componentRef.Entity;
        }

        /// <summary>异步添加 Child Entity（1 参 Awake）并指定 id。</summary>
        public async Promise<T> AddChildWithIdAsync<T, A>(long id, A a, bool isFromPool = false) where T : Entity, IAwakeAsync<A>
        {
            var type = typeof(T);
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, a);
            return (T)componentRef.Entity;
        }

        /// <summary>异步添加 Child Entity（2 参 Awake）并指定 id。</summary>
        public async Promise<T> AddChildWithIdAsync<T, A, B>(long id, A a, B b, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B>
        {
            var type = typeof(T);
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, a, b);
            return (T)componentRef.Entity;
        }

        /// <summary>异步添加 Child Entity（3 参 Awake）并指定 id。</summary>
        public async Promise<T> AddChildWithIdAsync<T, A, B, C>(long id, A a, B b, C c, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B, C>
        {
            var type = typeof(T);
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, a, b, c);
            return (T)componentRef.Entity;
        }

        /// <summary>异步添加 Child Entity（4 参 Awake）并指定 id。</summary>
        public async Promise<T> AddChildWithIdAsync<T, A, B, C, D>(long id, A a, B b, C c, D d, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B, C, D>
        {
            var type = typeof(T);
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, a, b, c, d);
            return (T)componentRef.Entity;
        }

        /// <summary>异步添加 Child Entity（5 参 Awake）并指定 id。</summary>
        public async Promise<T> AddChildWithIdAsync<T, A, B, C, D, E>(long id, A a, B b, C c, D d, E e, bool isFromPool = false) where T : Entity, IAwakeAsync<A, B, C, D, E>
        {
            var type = typeof(T);
            var component = Create(type, isFromPool);
            component.Id = id;
            component.Parent = this;
            EntityRef<Entity> componentRef = component;
            await EntityAwakeAsyncSystem.EntityAwakeAsync(componentRef, a, b, c, d, e);
            return (T)componentRef.Entity;
        }
    }
}
