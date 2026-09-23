using System.Threading;

namespace RD.Core
{
    /// <summary>
    /// 兜底日志的输出目标（策略接口）。
    /// 平台差异收敛到各自实现里，避免在调用处用条件编译和级别数值比较做分支。
    /// </summary>
    internal interface ILogSink
    {
        void Write(LogSeverity severity, string message);
    }

#if UNITY
    /// <summary>
    /// Unity 兜底输出：按级别映射到 UnityEngine.Debug 的 Log / LogWarning / LogError。
    /// </summary>
    internal sealed class UnityLogSink: ILogSink
    {
        [StaticField]
        public static readonly UnityLogSink Instance = new();

        public void Write(LogSeverity severity, string message)
        {
            switch (severity)
            {
                case LogSeverity.Error:
                    global::UnityEngine.Debug.LogError(message);
                    break;
                case LogSeverity.Warning:
                    global::UnityEngine.Debug.LogWarning(message);
                    break;
                default:
                    global::UnityEngine.Debug.Log(message);
                    break;
            }
        }
    }
#else
    /// <summary>
    /// 服务端兜底输出：标准输出 / 标准错误。
    /// Console.Out 是带进程级全局锁的同步 IO，无节制地输出会把一次偶发的配置疏漏
    /// 放大成性能事故，因此非 Error 日志限量输出，并定期汇报丢弃量。
    /// </summary>
    internal sealed class ConsoleLogSink: ILogSink
    {
        /// <summary>兜底通道允许输出的非 Error 日志总条数。</summary>
        private const int MaxNonErrorWrites = 1000;

        /// <summary>丢弃日志的汇报间隔，保证运维仍能感知真实日志量。</summary>
        private const int DroppedReportInterval = 1000;

        [StaticField]
        public static readonly ConsoleLogSink Instance = new();

        // 实例字段：限流是单个 sink 自己的状态，不需要进静态字段表。
        private int nonErrorWriteCount;
        private int droppedCount;

        public void Write(LogSeverity severity, string message)
        {
            if (severity == LogSeverity.Error)
            {
                // Error 无条件输出：兜底通道存在的意义就是不让致命信息丢失。
                System.Console.Error.WriteLine(message);
                return;
            }

            if (Interlocked.Increment(ref this.nonErrorWriteCount) <= MaxNonErrorWrites)
            {
                System.Console.WriteLine(message);
                return;
            }

            // 已超限：回写封顶值，防止长期运行后 int 溢出。
            Interlocked.Exchange(ref this.nonErrorWriteCount, MaxNonErrorWrites);

            if (Interlocked.Increment(ref this.droppedCount) % DroppedReportInterval == 0)
            {
                System.Console.Error.WriteLine(
                        $"[FallbackLog] 兜底日志已超限，累计丢弃 {Volatile.Read(ref this.droppedCount)} 条非 Error 日志，请检查 Logger 配置。");
            }
        }
    }
#endif
}
