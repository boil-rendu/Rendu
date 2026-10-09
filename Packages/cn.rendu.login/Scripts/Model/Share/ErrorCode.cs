namespace RD
{
	/// <summary>
	/// Login 包的错误码常量（partial）：每个常量 = <c>ERR_WithException + PackageType.Login * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class ErrorCode
	{
		/// <summary>连接 Gate 的 key 校验失败（防重放/防伪造）。</summary>
		public const int ERR_ConnectGateKeyError = ERR_WithException + PackageType.Login * 1000 + 1;
	}
}
