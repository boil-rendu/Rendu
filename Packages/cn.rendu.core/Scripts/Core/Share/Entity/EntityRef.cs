using System;

namespace RD.Core
{
    /// <summary>
    /// 实体的强类型句柄（struct，避免分配）。
    /// 内部以 <see cref="Entity.InstanceId"/> + 实体引用共同维护：<c>Entity</c> 字段在 <c>InstanceId</c> 变化（实体被回收/复用）时自动置 null 以让 GC 回收。
    /// </summary>
    /// <typeparam name="T">目标实体类型。</typeparam>
    public struct EntityRef<T>: IEquatable<EntityRef<T>> where T: Entity
    {
        private readonly long instanceId;
        private T entity;

        /// <summary>基于 <see cref="instanceId"/> 的 hash。</summary>
        public override int GetHashCode()
        {
            return this.instanceId.GetHashCode();
        }

        /// <summary>按 <see cref="instanceId"/> 比较相等性。</summary>
        public bool Equals(EntityRef<T> obj)
        {
            return this.instanceId == obj.instanceId;
        }

        /// <summary>按 <see cref="instanceId"/> 比较相等性（非泛型 object 重载）。</summary>
        public override bool Equals(object obj)
        {
            if (obj is not EntityRef<T> entityRef)
            {
                return false;
            }

            return this.instanceId == entityRef.instanceId;
        }

        /// <summary><c>==</c> 等价于 <see cref="Equals(EntityRef{T})"/>。</summary>
        public static bool operator ==(EntityRef<T> left, EntityRef<T> right)
        {
            return left.Equals(right);
        }

        /// <summary><c>!=</c> 等价于 <c>!Equals</c>。</summary>
        public static bool operator !=(EntityRef<T> left, EntityRef<T> right)
        {
            return !left.Equals(right);
        }

        private EntityRef(T t)
        {
            if (t == null)
            {
                throw new Exception("cant convert to entityref, entity is null");
            }

            if (t.InstanceId == 0)
            {
                throw new Exception("cant convert to entityref, entity instanceid == 0!");
            }
            this.instanceId = t.InstanceId;
            this.entity = t;
        }

        /// <summary>解引用为实体；若 <c>InstanceId</c> 已变化（实体被回收/复用），返回 null 并清空内部引用以便 GC。</summary>
        public T Entity
        {
            get
            {
                if (this.entity == null)
                {
                    return null;
                }
                if (this.entity.InstanceId != this.instanceId)
                {
                    // 这里instanceId变化了，设置为null，解除引用，好让runtime去gc
                    this.entity = null;
                }
                return this.entity;
            }
        }

        /// <summary>隐式转换：实体 → <see cref="EntityRef{T}"/>。</summary>
        public static implicit operator EntityRef<T>(T t)
        {
            return new EntityRef<T>(t);
        }

        /// <summary>隐式转换：<see cref="EntityRef{T}"/> → 实体（解引用，可能返回 null）。</summary>
        public static implicit operator T(EntityRef<T> v)
        {
            return v.Entity;
        }
    }


    /// <summary>
    /// 实体的弱类型句柄（struct），内部使用 <see cref="WeakReference{T}"/> 防止实体被回收后仍持有强引用导致无法 GC。
    /// 注意：<c>WeakReference</c> 的性能未经过压测。
    /// </summary>
    /// <typeparam name="T">目标实体类型。</typeparam>
    public struct EntityWeakRef<T>: IEquatable<EntityWeakRef<T>> where T: Entity
    {
        private long instanceId;
        // 使用WeakReference，这样不会导致entity dispose了却无法gc的问题
        // 不过暂时没有测试WeakReference的性能
        private readonly WeakReference<T> weakRef;

        private EntityWeakRef(T t)
        {
            if (t == null)
            {
                throw new Exception("cant convert to entityref, entity is null");
            }

            if (t.InstanceId == 0)
            {
                throw new Exception("cant convert to entityref, entity instanceid == 0!");
            }
            this.instanceId = t.InstanceId;
            this.weakRef = new WeakReference<T>(t);
        }

        /// <summary>解引用为实体；若弱引用已失效或 <c>InstanceId</c> 已变化，返回 null。</summary>
        public T Entity
        {
            get
            {
                if (this.instanceId == 0)
                {
                    return null;
                }

                if (!this.weakRef.TryGetTarget(out T entity))
                {
                    this.instanceId = 0;
                    return null;
                }

                if (entity.InstanceId != this.instanceId)
                {
                    this.instanceId = 0;
                    return null;
                }
                return entity;
            }
        }

        /// <summary>隐式转换：实体 → <see cref="EntityWeakRef{T}"/>。</summary>
        public static implicit operator EntityWeakRef<T>(T t)
        {
            return new EntityWeakRef<T>(t);
        }

        /// <summary>隐式转换：<see cref="EntityWeakRef{T}"/> → 实体（解引用，可能返回 null）。</summary>
        public static implicit operator T(EntityWeakRef<T> v)
        {
            return v.Entity;
        }

        /// <summary>基于 <see cref="instanceId"/> 的 hash。</summary>
        public override int GetHashCode()
        {
            return this.instanceId.GetHashCode();
        }

        /// <summary>按 <see cref="instanceId"/> 比较相等性。</summary>
        public bool Equals(EntityWeakRef<T> obj)
        {
            return this.instanceId == obj.instanceId;
        }

        /// <summary>按 <see cref="instanceId"/> 比较相等性（非泛型 object 重载）。</summary>
        public override bool Equals(object obj)
        {
            if (obj is not EntityWeakRef<T> entityRef)
            {
                return false;
            }

            return this.instanceId == entityRef.instanceId;
        }

        /// <summary><c>==</c> 等价于 <see cref="Equals(EntityWeakRef{T})"/>。</summary>
        public static bool operator ==(EntityWeakRef<T> left, EntityWeakRef<T> right)
        {
            return left.Equals(right);
        }

        /// <summary><c>!=</c> 等价于 <c>!Equals</c>。</summary>
        public static bool operator !=(EntityWeakRef<T> left, EntityWeakRef<T> right)
        {
            return !left.Equals(right);
        }
    }
}
