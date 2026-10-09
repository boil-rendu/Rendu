namespace RD
{
	/// <summary>
	/// 进程内共享错误码常量集合（partial）：每个常量 = <c>ERR_WithException + PackageType.Core * 1000 + 业务子码</c>。
	/// 其它子 dll（actorlocation/login/...）以 partial 形式追加自己的错误码。
	/// </summary>
	public static partial class ErrorCode
	{
		/// <summary>Session 收发超时（区分 <c>ERR_WithException</c>，让网络层识别这是错误而非业务正常返回）。</summary>
		public const int ERR_SessionSendOrRecvTimeout = ERR_WithException + PackageType.Core * 1000 + 1;
	}
}
