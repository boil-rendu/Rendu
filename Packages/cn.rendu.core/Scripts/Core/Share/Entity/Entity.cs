using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace RD.Core
{
    /// <summary>实体运行时状态位标记（<see cref="FlagsAttribute"/>）。</summary>
    [Flags]
    public enum EntityStatus: byte
    {
        /// <summary>无状态。</summary>
        None = 0,
        /// <summary>实例来自对象池（需 <see cref="ObjectPool.Recycle{T}"/> 归还）。</summary>
        IsFromPool = 1,
        /// <summary>已注册到所属 Scene 的 <see cref="EntitySystem"/>，可被 <c>Publish</c>/<c>Awake</c>/<c>Destroy</c> 等系统派发器发现。</summary>
        IsRegister = 1 << 1,
        /// <summary>此 Entity 作为另一个 Entity 的 Component 存在（关系为 Owner-Component）。</summary>
        IsComponent = 1 << 2,
        /// <summary>新创建的实例（用于反序列化等场景的差异化处理）。</summary>
        IsNew = 1 << 3,
        /// <summary>序列化时跟随父 Entity 一起写入（默认行为）。</summary>
        IsSerilizeWithParent = 1 << 4,
    }

    /// <summary>
    /// 框架实体基类，所有 ECS Entity / Component / Scene 对象的根。
    /// 支持对象池复用（<see cref="IPool"/>）、MemoryPack/Bson 序列化、Component 挂载、<see cref="EntitySystem"/> 自动注册。
    /// 由源生成器扫描并注册带有 <see cref="EntitySystemAttribute"/> 的 System；本身不直接构造，业务通过 <c>EntityFactory.Create&lt;T&gt;</c> 等方式实例化。
    /// </summary>
    [MemoryPackable(GenerateType.NoGenerate)]
    public abstract partial class Entity: DisposeObject, IPool
    {
        /// <summary>从对象池获取（或新建）一个 <typeparamref name="T"/> 实体，给源生成器调用。</summary>
        // 给source generater调用的
        public static T Fetch<T>() where T : Entity
        {
            return ObjectPool.Fetch<T>();
        }

        /// <summary>实体类型的稳定 hash（基于 <see cref="RuntimeTypeHandle"/>），用于跨进程/会话比较实体类别。</summary>
        public virtual long GetLongHashCode()
        {
            return this.GetType().TypeHandle.Value.ToInt64();
        }

        /// <summary>组件类型的稳定 hash（基于 <see cref="RuntimeTypeHandle"/>），用于组件类型比较。</summary>
        public virtual long GetComponentLongHashCode(Type type)
        {
            return type.TypeHandle.Value.ToInt64();
        }

#if ENABLE_VIEW && UNITY_EDITOR
        /// <summary>Editor 视图对应的 GameObject（仅在 <c>ENABLE_VIEW</c> + Editor 下可见/构建）。</summary>
        [BsonIgnore]
        [UnityEngine.HideInInspector]
        [MemoryPackIgnore]
        public UnityEngine.GameObject ViewGO;
#endif

        /// <summary>实例唯一 ID（运行时分配，跨进程/网络传输需使用 <c>LongHash</c>）。</summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public long InstanceId { get; protected set; }

        /// <summary>状态位（参见 <see cref="EntityStatus"/>）。</summary>
        [BsonIgnore]
        private EntityStatus status = EntityStatus.None;

        /// <summary>默认构造由源生成器/池调用，业务请用 <c>EntityFactory.Create&lt;T&gt;</c>。</summary>
        protected Entity()
        {
        }

        /// <summary>实例是否来自对象池（参见 <see cref="EntityStatus.IsFromPool"/>）。</summary>
        [MemoryPackIgnore]
        [BsonIgnore]
        public bool IsFromPool
        {
            get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this.status &= ~EntityStatus.IsFromPool;
                }
            }
        }

        /// <summary>是否已注册到所属 Scene 的 <see cref="EntitySystem"/>。置 true 触发 <see cref="RegisterSystem"/> 与视图创建；置 false 撤销注册并销毁视图。</summary>
        [BsonIgnore]
        protected bool IsRegister
        {
            get => (this.status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
            set
            {
                if (this.IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    this.status |= EntityStatus.IsRegister;
                }
                else
                {
                    this.status &= ~EntityStatus.IsRegister;
                }

                if (value)
                {
                    this.RegisterSystem();
                }

#if ENABLE_VIEW && UNITY_EDITOR
                if (value)
                {
                    this.ViewGO = new UnityEngine.GameObject(this.ViewName);
                    this.ViewGO.AddComponent<ComponentView>().Component = this;
                    this.ViewGO.transform.SetParent(this.Parent == null?
                            UnityEngine.GameObject.Find("Global/ScenesRoot").transform : this.Parent.ViewGO.transform);
                }
                else
                {
                    UnityEngine.Object.Destroy(this.ViewGO);
                }
#endif
            }
        }

        /// <summary>注册到所属 <see cref="EntitySystem"/>，由 <see cref="IsRegister"/> setter 在置 true 时调用。子类可重写以注册额外的 System 钩子。</summary>
        protected virtual void RegisterSystem()
        {
            this.iScene.Fiber.EntitySystem.RegisterSystem(this);
        }

        /// <summary>Editor 视图对象名（默认 <see cref="Type.FullName"/>），子类可重写为更友好的名字。</summary>
        protected virtual string ViewName
        {
            get
            {
                return this.GetType().FullName;
            }
        }

        /// <summary>此 Entity 是否为另一 Entity 的 Component。</summary>
        [BsonIgnore]
        protected bool IsComponent
        {
            get => (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsComponent;
                }
                else
                {
                    this.status &= ~EntityStatus.IsComponent;
                }
            }
        }

        /// <summary>是否本次新创建的实例（用于序列化/反序列化的差异处理）。</summary>
        [BsonIgnore]
        protected bool IsNew
        {
            get => (this.status & EntityStatus.IsNew) == EntityStatus.IsNew;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsNew;
                }
                else
                {
                    this.status &= ~EntityStatus.IsNew;
                }
            }
        }

        /// <summary>序列化时是否跟随父 Entity 一起写入（默认 true）。</summary>
        [BsonIgnore]
        public bool IsSerilizeWithParent
        {
            get => (this.status & EntityStatus.IsSerilizeWithParent) == EntityStatus.IsSerilizeWithParent;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsSerilizeWithParent;
                }
                else
                {
                    this.status &= ~EntityStatus.IsSerilizeWithParent;
                }
            }
        }
    }
}
