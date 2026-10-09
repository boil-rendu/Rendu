using System;

namespace RD.Core
{
    /// <summary>日志门面：定义 5 级（Trace/Warning/Info/Debug/Error）方法 + Error 重载（接受 <see cref="Exception"/>）。</summary>
    /// <remarks>
    /// <c>DOTNET</c> 编译符号下还提供「<c>ref DefaultInterpolatedStringHandler</c>」重载，避免 <c>$\"...{x}...\"</c> 在不需要时仍格式化字符串。
    /// 由 <see cref="Logger"/> 持有实例，调用方用 <c>Logger.Instance.Log.Info(...)</c>，便于在测试或 AOT 限制场景替换实现。
    /// </remarks>
    public interface ILog
    {
        void Trace(string message);
        void Warning(string message);
        void Info(string message);
        void Debug(string message);
        void Error(string message);
        void Error(Exception e);

#if DOTNET
        public void Trace(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Warning(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Info(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Debug(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
        public void Error(ref System.Runtime.CompilerServices.DefaultInterpolatedStringHandler message);
#endif
    }
}
