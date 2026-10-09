namespace RD.Core
{

    /// <summary>日志上下文：随每条日志一起记录的进程/Fiber/场景标识，便于多进程聚合日志时按 Server/Scene 归类。</summary>
    public struct LogInvoker
    {
        /// <summary>场景名（<see cref="Options.SceneName"/>）。</summary>
        public string SceneName;
        /// <summary>进程号（<see cref="Options.Process"/>）。</summary>
        public int Process;
        /// <summary>当前 <see cref="Fiber"/> Id。</summary>
        public int Fiber;
    }
}
