using System;
using System.Diagnostics;

namespace RD.Core
{
    /// <summary>
    /// 日志门面。只负责级别过滤与来源选择，具体输出委托给 ILog 实现。
    /// 保留静态入口是为了兼容既有调用方式，内部逻辑全部走多态。
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 日志来源责任链，按优先级排列：纤程级 → 进程级 → 兜底。
        /// 兜底实现在构造时即绑定，故 Resolve() 恒不为空。
        /// </summary>
        [StaticField]
        private static readonly LogSourceChain Chain = new(
                FallbackLog.Instance, new FiberLogSource(), new GlobalLogSource());

        /// <summary>
        /// Options 尚未注册或已销毁时使用的日志级别。
        /// 与 Options.LogLevel 的默认值一致（0 = 最详细）：配置不可用时宁可多打也不能漏打，
        /// 日志丢失的代价远高于日志冗余。
        /// </summary>
        private const int DefaultLogLevel = 0;

        /// <summary>
        /// 是否发生过降级（锁存：一旦降级即保持为 true，不会因恢复而自动清零）。
        /// 降级意味着日志没有落到正式实现（文件 / 上报），属于必须排查的异常信号。
        /// 生产环境该值应恒为 false，可用于启动自检、运行时监控与单元测试断言。
        /// </summary>
        [StaticField]
        public static bool HasFallbackOccurred => FallbackLog.HitCount > 0;

        /// <summary>
        /// 启动自检：确认日志正式实现已注入。
        /// 日志缺失属于致命配置错误，应当在启动阶段 fail fast——
        /// 否则进程会"看起来正常"地运行，却一条日志都收不到。
        /// </summary>
        public static void EnsureConfigured()
        {
            if (Logger.Instance == null || Logger.Instance.Log == null)
            {
                throw new InvalidOperationException(
                        "[Log] 日志实现未注入：Logger.Instance.Log 为 null。" +
                        "日志缺失会导致运行期静默降级，请检查启动流程中的 Logger 注入。");
            }
        }

        [Conditional("DEBUG")]
        public static void Debug(string msg)
        {
            if (!IsEnabled(LogSeverity.Debug))
            {
                return;
            }

            GetLog().Debug(msg);
        }

        [Conditional("DEBUG")]
        public static void Trace(string msg)
        {
            if (!IsEnabled(LogSeverity.Trace))
            {
                return;
            }

            StackTrace st = new(1, true);
            GetLog().Trace($"{msg}\n{st}");
        }

        public static void Info(string msg)
        {
            if (!IsEnabled(LogSeverity.Info))
            {
                return;
            }

            GetLog().Info(msg);
        }

        public static void TraceInfo(string msg)
        {
            if (!IsEnabled(LogSeverity.Info))
            {
                return;
            }

            StackTrace st = new(1, true);
            GetLog().Trace($"{msg}\n{st}");
        }

        public static void Warning(string msg)
        {
            if (!IsEnabled(LogSeverity.Warning))
            {
                return;
            }

            GetLog().Warning(msg);
        }

        public static void Error(string msg)
        {
            StackTrace st = new(1, true);
            GetLog().Error($"{msg}\n{st}");
        }

        public static void Error(Exception e)
        {
            GetLog().Error(e.ToString());
        }

        public static void Console(string msg)
        {
            ILog log = GetLog();

            // 兜底通道的唯一出口就是控制台，此时再单独写一遍会让同一条消息输出两次。
            if (IsConsoleEnabled() && !ReferenceEquals(log, FallbackLog.Instance))
            {
                System.Console.WriteLine(msg);
            }

            log.Debug(msg);
        }

        /// <summary>
        /// 当前日志级别阈值。
        /// Options 是 Singleton，World 初始化前与 Dispose 后 Instance 均为 null，
        /// 这里必须做空判——否则崩溃会发生在比兜底逻辑更早的地方，兜底形同虚设。
        /// </summary>
        private static int GetLogLevel()
        {
            Options options = Options.Instance;
            return options == null ? DefaultLogLevel : options.LogLevel;
        }

        private static bool IsConsoleEnabled()
        {
            Options options = Options.Instance;
            return options != null && options.Console == 1;
        }

        private static bool IsEnabled(LogSeverity severity)
        {
            return GetLogLevel() <= (int)severity;
        }

        private static ILog GetLog()
        {
            return Chain.Resolve();
        }

#if DOTNET
        [Conditional("DEBUG")]
        public static void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (!IsEnabled(LogSeverity.Trace))
            {
                return;
            }
            StackTrace st = new(1, true);
            GetLog().Trace($"{message.ToStringAndClear()}\n{st.ToString()}");
        }
        [Conditional("DEBUG")]
        public static void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (!IsEnabled(LogSeverity.Warning))
            {
                return;
            }
            GetLog().Warning(ref message);
        }

        public static void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (!IsEnabled(LogSeverity.Info))
            {
                return;
            }
            GetLog().Info(ref message);
        }
        [Conditional("DEBUG")]
        public static void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            if (!IsEnabled(LogSeverity.Debug))
            {
                return;
            }
            GetLog().Debug(ref message);
        }

        public static void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message)
        {
            GetLog().Error(ref message);
        }
#endif
    }
}
