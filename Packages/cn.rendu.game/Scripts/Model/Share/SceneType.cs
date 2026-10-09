using RD.Core;
namespace RD
{
	/// <summary>
	/// Game 包的 SceneType 常量（partial）：每个常量 = <c>PackageType.StateSync * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class SceneType
	{
		/// <summary>HTTP Scene：处理 HTTP 请求（HttpComponent 挂在这里）。</summary>
		public const int Http = PackageType.StateSync * 1000 + 1;
		/// <summary>Map Scene：游戏地图主逻辑（Battle/Unit/战斗循环）。</summary>
		public const int Map = PackageType.StateSync * 1000 + 2;
		/// <summary>Robot Scene：AI 机器人/压测实例。</summary>
		public const int Robot = PackageType.StateSync * 1000 + 3;

		// 客户端
		/// <summary>客户端主 StateSync Scene：状态同步逻辑。</summary>
		public const int StateSync = PackageType.StateSync * 1000 + 20;
		/// <summary>客户端当前 Scene：大世界场景管理容器。</summary>
		public const int Current = PackageType.StateSync * 1000 + 21;
		/// <summary>客户端 StateSync 视图层（纯表现，不参与逻辑）。</summary>
		public const int StateSyncView = PackageType.StateSync * 1000 + 24;
	}
}
