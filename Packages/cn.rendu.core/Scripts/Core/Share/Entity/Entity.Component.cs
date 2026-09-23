using System;
using System.Collections.Generic;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
    // 组件（Component）：挂在 Entity 上的子实体，与 Parent 共享 Id，由 ComponentsCollection 存储
    public abstract partial class Entity
    {
        [MemoryPackInclude]
        [BsonElement]
        [BsonIgnoreIfNull]
        protected ComponentsCollection components;

        /// <summary>
        /// 当前实体挂载的组件集合。组件（Component）是与父实体<b>共享同一 Id</b> 的子实体，
        /// 通过 <see cref="AddComponent"/> 挂载后由 ComponentsCollection 按类型哈希索引。
        /// 该属性惰性分配：首次访问时才从对象池取一个集合，因此未挂载任何组件时读数会按需创建。
        /// </summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public ComponentsCollection Components
        {
            get
            {
                return this.components ??= ObjectPool.Fetch<ComponentsCollection>();
            }
        }

        public int ComponentsCount()
        {
            if (this.components == null)
            {
                return 0;
            }
            return this.components.Count;
        }

        private void AddToComponents(Entity component)
        {
            this.Components.Add(component.GetLongHashCode(), component);
        }

        private void RemoveComponentNoDispose(Entity component)
        {
            if (this.components == null)
            {
                return;
            }

            if (!this.components.Remove(component.GetLongHashCode()))
            {
                return;
            }

            if (this.components.Count == 0)
            {
                this.components.Dispose();
                this.components = null;
            }
        }

        public K GetComponent<K>() where K : Entity
        {
            if (this.components == null)
            {
                return null;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            if (this is IGetComponent)
            {
                EntitySystemSingleton.Instance.RunGetComponentSystems(this, typeof(K));
            }
            
            Entity component;
            if (!this.components.TryGetValue(this.GetComponentLongHashCode(typeof (K)), out component))
            {
                return default;
            }

            return (K) component;
        }

        public Entity GetComponent(Type type)
        {
            if (this.components == null)
            {
                return null;
            }

            // 如果有IGetComponent接口，则触发GetComponentSystem
            // 这个要在tryget之前调用，因为有可能components没有，但是执行GetComponentSystem后又有了
            if (this is IGetComponent)
            {
                EntitySystemSingleton.Instance.RunGetComponentSystems(this, type);
            }
            
            Entity component;
            if (!this.components.TryGetValue(this.GetComponentLongHashCode(type), out component))
            {
                return null;
            }

            return component;
        }

        private static Entity Create(Type type, bool isFromPool)
        {
            Entity component = (Entity) ObjectPool.Fetch(type, isFromPool);

            component.IsFromPool = isFromPool;
            component.IsNew = true;
            component.Id = 0;
            return component;
        }

        public Entity AddComponent(Entity component)
        {
            Type type = component.GetType();
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            return component;
        }

        public Entity AddComponent(Type type, bool isFromPool = false)
        {
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = this.Id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component);

            return component;
        }

        public K AddComponentWithId<K>(long id, bool isFromPool = false) where K : Entity, IAwake, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component);

            return component as K;
        }

        public K AddComponentWithId<K, P1>(long id, P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2>(long id, P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1, p2);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2, P3>(long id, P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton entitySystemSingleton = EntitySystemSingleton.Instance;
            EntitySystemSingleton.Instance.Awake(component, p1, p2, p3);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2, P3, P4>(long id, P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1, p2, p3, p4);

            return component as K;
        }

        public K AddComponentWithId<K, P1, P2, P3, P4, P5>(long id, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4, P5>, new()
        {
            Type type = typeof (K);
            if (this.components != null && this.components.ContainsKey(this.GetComponentLongHashCode(type)))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = Create(type, isFromPool);
            component.Id = id;
            component.ComponentParent = this;
            EntitySystemSingleton.Instance.Awake(component, p1, p2, p3, p4, p5);

            return component as K;
        }

        public K AddComponent<K>(bool isFromPool = false) where K : Entity, IAwake, new()
        {
            return this.AddComponentWithId<K>(this.Id, isFromPool);
        }

        public K AddComponent<K, P1>(P1 p1, bool isFromPool = false) where K : Entity, IAwake<P1>, new()
        {
            return this.AddComponentWithId<K, P1>(this.Id, p1, isFromPool);
        }

        public K AddComponent<K, P1, P2>(P1 p1, P2 p2, bool isFromPool = false) where K : Entity, IAwake<P1, P2>, new()
        {
            return this.AddComponentWithId<K, P1, P2>(this.Id, p1, p2, isFromPool);
        }

        public K AddComponent<K, P1, P2, P3>(P1 p1, P2 p2, P3 p3, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3>, new()
        {
            return this.AddComponentWithId<K, P1, P2, P3>(this.Id, p1, p2, p3, isFromPool);
        }

        public K AddComponent<K, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4>, new()
        {
            return this.AddComponentWithId<K, P1, P2, P3, P4>(this.Id, p1, p2, p3, p4, isFromPool);
        }

        public K AddComponent<K, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5, bool isFromPool = false) where K : Entity, IAwake<P1, P2, P3, P4, P5>, new()
        {
            return this.AddComponentWithId<K, P1, P2, P3, P4, P5>(this.Id, p1, p2, p3, p4, p5, isFromPool);
        }

        public void RemoveComponent<K>() where K : Entity
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.components == null)
            {
                return;
            }

            Type type = typeof (K);

            if (this.components.TryGetValue(this.GetComponentLongHashCode(type), out Entity c))
            {
                this.components.Remove(this.GetComponentLongHashCode(type));
                c.Dispose();
            }
        }

        public void RemoveComponent(Type type)
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            if (this.components == null)
            {
                return;
            }

            if (this.components.TryGetValue(this.GetComponentLongHashCode(type), out Entity c))
            {
                this.components.Remove(this.GetComponentLongHashCode(type));
                c.Dispose();
            }
        }
    }
}
