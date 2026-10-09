namespace RD
{
	/// <summary>
	/// Login 包的 Timer Invoke 常量（partial）：每个常量 = <c>PackageType.Login * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class TimerInvokeType
	{
		/// <summary>Session 闲置检测器定时器（超时未收到心跳则关闭 session）。</summary>
		public const int SessionIdleChecker = PackageType.Login * 1000 + 1;
		/// <summary>Session 接受连接后等待握手的超时定时器。</summary>
		public const int SessionAcceptTimeout = PackageType.Login * 1000 + 2;
	}
}
