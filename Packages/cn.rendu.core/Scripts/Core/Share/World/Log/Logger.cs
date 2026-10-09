namespace RD.Core
{
    /// <summary>日志实现持有者：单例，内部存一份 <see cref="ILog"/> 实例，业务通过 <see cref="Log"/> 替换底层（Unity / Console / 文件 / Test 桩等）。</summary>
    public class Logger: Singleton<Logger>, ISingletonAwake
    {
        private ILog log;

        /// <summary>当前日志实现：<c>set</c> 在启动早期由具体平台（Editor LogHandler/Server Console/FileLogger）赋值，<c>get</c> 供全局 <c>Log.Info(...)</c> 静态门面取用。</summary>
        public ILog Log
        {
            set
            {
                this.log = value;
            }
            get
            {
                return this.log;
            }
        }

        public void Awake()
        {
        }
    }
}
