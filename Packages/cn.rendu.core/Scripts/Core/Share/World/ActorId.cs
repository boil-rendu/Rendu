using System;
using System.Runtime.InteropServices;
using MemoryPack;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
    /// <summary>进程 + Fiber 二元组定位：用于网络/分布式寻址，配合 <see cref="ActorId"/> 进一步定位到具体 Entity。</summary>
    /// <remarks>64 位布局：低 14 位 Process、中 30 位 Fiber、高 20 位预留（与 <see cref="IdStruct"/> 编码方式不同，这里只用到 Process/Fiber）。</remarks>
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct Address
    {
        /// <summary>进程号。</summary>
        [MemoryPackOrder(0)]
        public int Process;
        /// <summary>Fiber 号。</summary>
        [MemoryPackOrder(1)]
        public int Fiber;

        public bool Equals(Address other)
        {
            return this.Process == other.Process && this.Fiber == other.Fiber;
        }

        public override bool Equals(object obj)
        {
            return obj is Address other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Process, this.Fiber);
        }

        public Address(int process, int fiber)
        {
            this.Process = process;
            this.Fiber = fiber;
        }

        public static bool operator ==(Address left, Address right)
        {
            return left.Process == right.Process && left.Fiber == right.Fiber;
        }

        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{this.Process}:{this.Fiber}";
        }
    }

    /// <summary>Entity 在分布式系统中的全局唯一 Id：<see cref="Address"/> + Entity <c>InstanceId</c> 复合键，可序列化（Mongo / MemoryPack）。</summary>
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public partial struct ActorId
    {
        public bool Equals(ActorId other)
        {
            return this.Address == other.Address && this.InstanceId == other.InstanceId;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Address, this.InstanceId);
        }

        /// <summary>进程/路由地址。</summary>
        [MemoryPackOrder(0)]
        public Address Address;
        /// <summary>Entity 实例 Id（由 <see cref="IdGenerator.GenerateInstanceId"/> 产生，进程内单调递增）。</summary>
        [MemoryPackOrder(1)]
        public long InstanceId;

        /// <summary><see cref="ActorId.Address"/>.<see cref="Address.Process"/> 快捷访问（Mongo 序列化时跳过）。</summary>
        [BsonIgnore]
        public int Process
        {
            get
            {
                return this.Address.Process;
            }
            set
            {
                this.Address.Process = value;
            }
        }

        /// <summary><see cref="ActorId.Address"/>.<see cref="Address.Fiber"/> 快捷访问（Mongo 序列化时跳过）。</summary>
        [BsonIgnore]
        public int Fiber
        {
            get
            {
                return this.Address.Fiber;
            }
            set
            {
                this.Address.Fiber = value;
            }
        }

        public ActorId(int process, int fiber)
        {
            this.Address = new Address(process, fiber);
            this.InstanceId = 1;
        }

        public ActorId(int process, int fiber, long instanceId)
        {
            this.Address = new Address(process, fiber);
            this.InstanceId = instanceId;
        }

        public ActorId(Address address): this(address, 1)
        {
        }

        public ActorId(Address address, long instanceId)
        {
            this.Address = address;
            this.InstanceId = instanceId;
        }

        public static bool operator ==(ActorId left, ActorId right)
        {
            return left.InstanceId == right.InstanceId && left.Address == right.Address;
        }

        public static bool operator !=(ActorId left, ActorId right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{this.Process}:{this.Fiber}:{this.InstanceId}";
        }
    }
}
