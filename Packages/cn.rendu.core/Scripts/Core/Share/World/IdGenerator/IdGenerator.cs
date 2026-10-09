using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RD.Core
{
    /// <summary>全局唯一 Id 的位布局：<c>Process(14) | Time-since-2022(30) | Value(20)</c> = 64 位可逆编码。</summary>
    /// <remarks>
    /// 与 <see cref="InstanceIdStruct"/> 的 32+32 不同：本结构在分布式场景下可解码出 Process/Time/Value 三段，常用于 Entity 跨进程寻址。
    /// 位宽与 <see cref="IdGenerator.Mask14bit"/>/<see cref="IdGenerator.Mask30bit"/>/<see cref="IdGenerator.Mask20bit"/> 一一对应。
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IdStruct
    {
        /// <summary>进程号，14 位（最多 16384）。</summary>
        public short Process;  // 14bit
        /// <summary>自 2022-01-01 起的秒数，30 位（约 34 年）。</summary>
        public uint Time;    // 30bit
        /// <summary>进程内单调递增序号，20 位（每进程内每秒最多 1M Id）。</summary>
        public uint Value;   // 20bit

        /// <summary>把 3 段按 14+30+20 位压缩到 64 位 long。</summary>
        public long ToLong()
        {
            ulong result = 0;
            result |= (ushort) this.Process;
            result <<= 30;
            result |= this.Time;
            result <<= 20;
            result |= this.Value;
            return (long) result;
        }

        public IdStruct(uint time, short process, uint value)
        {
            this.Process = process;
            this.Time = time;
            this.Value = value;
        }

        /// <summary>从 64 位 long 反向解码出 (process, time, value)，用于网络包解析/调试。</summary>
        public IdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (uint) (result & IdGenerator.Mask20bit);
            result >>= 20;
            this.Time = (uint) result & IdGenerator.Mask30bit;
            result >>= 30;
            this.Process = (short) (result & IdGenerator.Mask14bit);
        }

        public override string ToString()
        {
            return $"process: {this.Process}, time: {this.Time}, value: {this.Value}";
        }
    }

    /// <summary>进程内 Entity 实例 Id：<c>Time(32) | Value(32)</c> = 64 位，可解码时间戳与序号。</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InstanceIdStruct
    {
        /// <summary>自 2022-01-01 起的秒数，32 位（约 136 年）。</summary>
        public uint Time;  // 32bit
        /// <summary>进程内单调递增序号，32 位（每秒最多 4G）。</summary>
        public uint Value; // 32bit

        /// <summary>把 (time, value) 按 32+32 位压缩到 64 位 long。</summary>
        public long ToLong()
        {
            ulong result = 0;
            result |= this.Time;
            result <<= 32;
            result |= this.Value;
            return (long) result;
        }

        public InstanceIdStruct(uint time, uint value)
        {
            this.Time = time;
            this.Value = value;
        }

        /// <summary>从 64 位 long 反向解码出 (time, value)。</summary>
        public InstanceIdStruct(long id)
        {
            ulong result = (ulong) id;
            this.Value = (uint)(result & uint.MaxValue);
            result >>= 32;
            this.Time = (uint)(result & uint.MaxValue);
        }

        public override string ToString()
        {
            return $"time: {this.Time}, value: {this.Value}";
        }
    }

    /// <summary>全局 Id 生成器：按位编码产出 64 位 <see cref="IdStruct"/>（含进程号）/ <see cref="InstanceIdStruct"/>（不含进程号），base epoch = 2022-01-01 UTC。</summary>
    public class IdGenerator: Singleton<IdGenerator>, ISingletonAwake
    {
        /// <summary>进程号最大分区数（2^14 = 16384）。</summary>
        public const int MaxZone = 1024;

        /// <summary>14 位掩码（<see cref="IdStruct.Process"/>）。</summary>
        public const int Mask14bit = 0x3fff;
        /// <summary>30 位掩码（<see cref="IdStruct.Time"/>）。</summary>
        public const int Mask30bit = 0x3fffffff;
        /// <summary>20 位掩码（<see cref="IdStruct.Value"/>）。</summary>
        public const int Mask20bit = 0xfffff;

        private long epoch2022;

        private int value;
        private int instanceIdValue;

        public void Awake()
        {
            long epoch1970tick = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
            this.epoch2022 = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - epoch1970tick;
        }

        /// <summary>当前 <see cref="TimeInfo.FrameTime"/> 距离 epoch2022 的秒数，作为 <see cref="IdStruct.Time"/>。</summary>
        private uint TimeSince2022()
        {
            uint a = (uint)((TimeInfo.Instance.FrameTime - this.epoch2022) / 1000);
            return a;
        }

        /// <summary>产出一个全局唯一 Id（<see cref="IdStruct"/> → long），进程内加锁推进 <c>value</c>，到 20 位上限时回卷。</summary>
        public long GenerateId()
        {
            uint time = TimeSince2022();
            int v = 0;
            // 这里必须加锁
            lock (this)
            {
                if (++this.value > Mask20bit - 1)
                {
                    this.value = 0;
                }
                v = this.value;
            }

            IdStruct idStruct = new(time, (short)Options.Instance.Process, (uint)v);
            return idStruct.ToLong();
        }

        /// <summary>产出一个进程内 Entity 实例 Id（<see cref="InstanceIdStruct"/> → long），用 <see cref="Interlocked.Add"/> 推进 <c>instanceIdValue</c>。</summary>
        public long GenerateInstanceId()
        {
            uint time = this.TimeSince2022();
            uint v = (uint)Interlocked.Add(ref this.instanceIdValue, 1);
            InstanceIdStruct instanceIdStruct = new(time, v);
            return instanceIdStruct.ToLong();
        }
    }
}
