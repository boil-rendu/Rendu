namespace RD.Core
{
    /// <summary>
    /// 日志严重级别。数值越大越严重。
    /// 过滤语义与 Options.LogLevel 一致：阈值越小，输出的级别越多。
    /// </summary>
    internal enum LogSeverity
    {
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warning = 4,
        Error = 5,
    }
}
