namespace RD
{
	/// <summary>
	/// Router 包的错误码常量（partial）：每个常量 = <c>ERR_WithException + PackageType.Router * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class ErrorCode
	{
		/// <summary>KCP Router 连接失败（无法建立到目标 Router 的连接）。</summary>
		public const int ERR_KcpRouterConnectFail = ERR_WithException + PackageType.Router * 1000 + 1;
		/// <summary>KCP Router 通信超时。</summary>
		public const int ERR_KcpRouterTimeout = ERR_WithException + PackageType.Router * 1000 + 2;
		/// <summary>重复注册同一个 Router（已有相同 key 的 Router）。</summary>
		public const int ERR_KcpRouterSame = ERR_WithException + PackageType.Router * 1000 + 3;
		/// <summary>Router 内部同步次数过多（路由回环或路由表震荡）。</summary>
		public const int ERR_KcpRouterRouterSyncCountTooMuchTimes = ERR_WithException + PackageType.Router * 1000 + 4;
		/// <summary>Router 收到过多包（流控触发）。</summary>
		public const int ERR_KcpRouterTooManyPackets = ERR_WithException + PackageType.Router * 1000 + 5;
		/// <summary>Router 同步次数过多（最终失败）。</summary>
		public const int ERR_KcpRouterSyncCountTooMuchTimes = ERR_WithException + PackageType.Router * 1000 + 6;
	}
}
