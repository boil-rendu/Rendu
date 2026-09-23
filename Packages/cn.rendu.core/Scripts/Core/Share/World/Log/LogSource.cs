namespace RD.Core
{
    /// <summary>
    /// 日志实现的来源（责任链上的一个环节）。
    /// 用多态取代调用处的 if 链：新增来源只需实现本接口并挂到链上。
    /// </summary>
    internal interface ILogSource
    {
        bool TryGetLog(out ILog log);
    }

    /// <summary>纤程级日志来源，优先级最高。</summary>
    internal sealed class FiberLogSource: ILogSource
    {
        public bool TryGetLog(out ILog log)
        {
            log = Fiber.Instance?.Log;
            return log != null;
        }
    }

    /// <summary>进程级日志来源（Logger 单例），纤程不可用时兜住。</summary>
    internal sealed class GlobalLogSource: ILogSource
    {
        public bool TryGetLog(out ILog log)
        {
            log = Logger.Instance?.Log;
            return log != null;
        }
    }

    /// <summary>
    /// 日志来源责任链（组合模式）。
    /// 构造时即绑定一个永不为空的兜底实现，因此 Resolve() 恒不返回 null，
    /// 调用侧无需判空，也不必写不可达的兜底分支。
    /// </summary>
    internal sealed class LogSourceChain
    {
        private readonly ILogSource[] sources;
        private readonly ILog fallback;

        public LogSourceChain(ILog fallback, params ILogSource[] sources)
        {
            this.fallback = fallback;
            this.sources = sources;
        }

        public ILog Resolve()
        {
            foreach (ILogSource source in this.sources)
            {
                if (source.TryGetLog(out ILog log))
                {
                    return log;
                }
            }

            return this.fallback;
        }
    }
}
