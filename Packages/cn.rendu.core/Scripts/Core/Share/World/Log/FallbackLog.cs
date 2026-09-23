using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RD.Core
{
    /// <summary>
    /// 兜底日志实现（Null Object）。
    /// 当 Fiber 与 Logger 都没能提供日志实现时使用，保证日志调用永不因空引用而崩溃；
    /// 同时不会静默——首次降级会明确告警，便于在启动阶段暴露配置错误。
    /// 输出目标由 ILogSink 决定，本类不关心具体平台。
    /// </summary>
    internal sealed class FallbackLog: ILog
    {
        [StaticField]
        private static readonly ILogSink Sink = CreateSink();

        [StaticField]
        public static readonly FallbackLog Instance = new();

        [StaticField]
        private static int hitCount;

        /// <summary>降级次数，首次命中时告警一次，同时供 Log.HasFallbackOccurred 观测。</summary>
        [StaticField]
        public static int HitCount => Volatile.Read(ref hitCount);

        public void Trace(string message) => Write(LogSeverity.Trace, message);
        public void Debug(string message) => Write(LogSeverity.Debug, message);
        public void Info(string message) => Write(LogSeverity.Info, message);
        public void Warning(string message) => Write(LogSeverity.Warning, message);
        public void Error(string message) => Write(LogSeverity.Error, message);
        public void Error(Exception e) => Write(LogSeverity.Error, e.ToString());

#if DOTNET
        public void Trace(ref DefaultInterpolatedStringHandler message) => Trace(message.ToStringAndClear());
        public void Debug(ref DefaultInterpolatedStringHandler message) => Debug(message.ToStringAndClear());
        public void Info(ref DefaultInterpolatedStringHandler message) => Info(message.ToStringAndClear());
        public void Warning(ref DefaultInterpolatedStringHandler message) => Warning(message.ToStringAndClear());
        public void Error(ref DefaultInterpolatedStringHandler message) => Error(message.ToStringAndClear());
#endif

        private static void Write(LogSeverity severity, string message)
        {
            ReportFallbackOnce();
            Sink.Write(severity, message);
        }

        private static void ReportFallbackOnce()
        {
            if (Interlocked.Increment(ref hitCount) != 1)
            {
                return;
            }

            // 只在首次降级时告警，后续每条都告警只会放大问题。
            // 注意：这里必须直接走 Sink，绝不可走 Log 门面，否则递归。
            Sink.Write(LogSeverity.Error,
                    "[FallbackLog] 日志实现缺失（Fiber.Instance.Log 与 Logger.Instance.Log 均为 null），" +
                    "已降级到兜底通道——日志不会落盘 / 上报，请检查启动流程中的 Logger 注入。");
        }

        private static ILogSink CreateSink()
        {
#if UNITY
            return UnityLogSink.Instance;
#else
            return ConsoleLogSink.Instance;
#endif
        }
    }
}
