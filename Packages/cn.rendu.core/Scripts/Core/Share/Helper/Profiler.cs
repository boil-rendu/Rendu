using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RD.Core
{
    /// <summary>
    /// 运行时耗时统计：先测量，再优化。
    /// - 一次性事件（启动注册、热更）：using var _ = Profiler.Once("key")，结束时立即输出耗时日志。
    /// - 热路径（每帧分发）：using var _ = Profiler.Section("key")，累加次数与耗时，由 Dump() 输出。
    /// Scope 为 struct，无堆分配；由 using 保证成对收口（含提前 return 的情况）。
    /// </summary>
    public static class Profiler
    {
        /// <summary>热路径统计开关（不影响 Once 的一次性日志）。</summary>
        [StaticField]
        public static bool Enabled { get; set; }

        [StaticField]
        private static readonly double TickToMs = 1000.0 / Stopwatch.Frequency;

        [StaticField]
        private static readonly object Gate = new();

        [StaticField]
        private static readonly Dictionary<string, Sample> samples = new();

        /// <summary>一次性事件计时：Dispose 时立即输出耗时日志。</summary>
        public static Scope Once(string key)
        {
            return new Scope(key, logOnce: true);
        }

        /// <summary>热路径计时：Dispose 时累加。Enabled 为 false 时不统计，零开销。</summary>
        public static Scope Section(string key)
        {
            return new Scope(key, logOnce: false);
        }

        /// <summary>
        /// 向标准输出打印。
        /// 服务端 Log.Info 不写控制台、Log.Console 又受 Options.Console(默认 0) 门控，
        /// 且启动期注册发生在 Options 创建之前，两条路都不通，故测量期直接写标准输出；
        /// Unity 侧仍走 Log.Info 进入 Console 窗口。
        /// </summary>
        public static void Print(string message)
        {
#if UNITY
            Log.Info(message);
#else
            System.Console.WriteLine(message);
#endif
        }

        /// <summary>输出累计统计（调用次数 / 总耗时 / 平均耗时）。</summary>
        public static void Dump()
        {
            lock (Gate)
            {
                if (samples.Count == 0)
                {
                    Print("[Profiler] no data (is Enabled false?)");
                    return;
                }

                StringBuilder sb = new();
                sb.AppendLine($"[Profiler] window | calls | total ms | avg us");

                foreach (KeyValuePair<string, Sample> pair in samples)
                {
                    Sample sample = pair.Value;
                    double totalMs = sample.Ticks * TickToMs;
                    double avgUs = sample.Count == 0 ? 0 : totalMs * 1000.0 / sample.Count;
                    sb.AppendLine($"  {pair.Key} | {sample.Count} | {totalMs:F3} | {avgUs:F3}");
                }

                Print(sb.ToString());
            }
        }

        /// <summary>清空累计数据。</summary>
        public static void Reset()
        {
            lock (Gate)
            {
                samples.Clear();
            }
        }

        private class Sample
        {
            public long Count;
            public long Ticks;
        }

        /// <summary>using 作用域计时器。struct 实现，无堆分配。</summary>
        public readonly struct Scope: IDisposable
        {
            private readonly string key;
            private readonly long startTicks;
            private readonly bool logOnce;

            internal Scope(string key, bool logOnce)
            {
                // 一次性事件始终计时；热路径受 Enabled 控制
                this.key = key;
                this.logOnce = logOnce;
                this.startTicks = (logOnce || Enabled) ? Stopwatch.GetTimestamp() : 0;
            }

            public void Dispose()
            {
                if (!this.logOnce && !Enabled)
                {
                    return;
                }

                long cost = Stopwatch.GetTimestamp() - this.startTicks;

                if (this.logOnce)
                {
                    Print($"[Profiler] {this.key} = {cost * TickToMs:F3} ms");
                    return;
                }

                lock (Gate)
                {
                    if (!samples.TryGetValue(this.key, out Sample sample))
                    {
                        sample = new Sample();
                        samples.Add(this.key, sample);
                    }

                    sample.Count++;
                    sample.Ticks += cost;
                }
            }
        }
    }
}
