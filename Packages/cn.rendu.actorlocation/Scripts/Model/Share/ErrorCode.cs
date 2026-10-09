namespace RD
{
	/// <summary>
	/// ActorLocation 包的错误码常量（partial）：每个常量 = <c>ERR_WithException + PackageType.ActorLocation * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class ErrorCode
	{
		/// <summary>找不到目标 Actor（路由表里查不到 instanceId）。</summary>
		public const int ERR_NotFoundActor = ERR_WithException + PackageType.ActorLocation * 1000 + 1;
		/// <summary>Actor RPC 调用失败（远端抛异常或非预期状态）。</summary>
		public const int ERR_RpcFail = ERR_WithException + PackageType.ActorLocation * 1000 + 2;
		/// <summary>Actor 消息处理超时。</summary>
		public const int ERR_MessageTimeout = ERR_WithException + PackageType.ActorLocation * 1000 + 3;
		/// <summary>ActorLocationSender 重试阶段 2 超时（Locate 后转发的第一次超时）。</summary>
		public const int ERR_ActorLocationSenderTimeout2 = ERR_WithException + PackageType.ActorLocation * 1000 + 4;
		/// <summary>ActorLocationSender 重试阶段 3 超时。</summary>
		public const int ERR_ActorLocationSenderTimeout3 = ERR_WithException + PackageType.ActorLocation * 1000 + 5;
		/// <summary>ActorLocationSender 重试阶段 4 超时（最终超时）。</summary>
		public const int ERR_ActorLocationSenderTimeout4 = ERR_WithException + PackageType.ActorLocation * 1000 + 6;
	}
}
