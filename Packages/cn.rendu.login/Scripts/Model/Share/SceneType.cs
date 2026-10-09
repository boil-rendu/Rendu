namespace RD
{
	/// <summary>
	/// Login 包的 SceneType 常量（partial）：每个常量 = <c>PackageType.Login * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class SceneType
	{
		/// <summary>Realm Server：账号注册/登录/选区（玩家接入层）。</summary>
		public const int Realm = PackageType.Login * 1000 + 1;
		/// <summary>Gate Server：网关（玩家长连接、消息转发、加密）。</summary>
		public const int Gate = PackageType.Login * 1000 + 2;
		/// <summary>Router Server：HTTP 路由（外部 HTTP 请求转发到内部 Scene）。</summary>
		public const int Router = PackageType.Login * 1000 + 3;
		/// <summary>Router Manager：管理多个 Router 实例。</summary>
		public const int RouterManager = PackageType.Login * 1000 + 4;
	}
}
