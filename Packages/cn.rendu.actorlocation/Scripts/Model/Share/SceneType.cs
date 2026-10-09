namespace RD
{
	/// <summary>
	/// ActorLocation 包的 SceneType 常量（partial）：每个常量 = <c>PackageType.ActorLocation * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class SceneType
	{
		/// <summary>ActorLocation Location Server：维护 actor id → process info 路由表。</summary>
		public const int Location = PackageType.ActorLocation * 1000 + 1;
	}
}
