namespace RD
{
	/// <summary>
	/// 进程内 SceneType 常量集合（partial）：<c>[UniqueId]</c> SourceGenerator 自动注入排序保证唯一，子 dll 在此基础上扩展。
	/// 每个常量 = <c>PackageType.xxx * 1000 + 业务子码</c>。
	/// </summary>
	[UniqueId]
	public static partial class SceneType
	{
		/// <summary>通配 SceneType：用于 EventSystem 监听器声明"对所有 Scene 生效"。</summary>
		public const int All = 0;
		/// <summary>主 Fiber Scene：进程启动的根 Scene，<see cref="MainFiberInit"/> 在它上面跑 InitShare/InitServer/InitClient。</summary>
		public const int Main = PackageType.Core * 1000 + 1;
		/// <summary>内部网络 Scene：服务端之间 TCP/KCP 通信入口（OuterActor/InnerActor 消息）。</summary>
		public const int NetInner = PackageType.Core * 1000 + 2;
		/// <summary>客户端网络 Scene：玩家客户端接入的 Scene（每个客户端一个 NetClient Scene）。</summary>
		public const int NetClient = PackageType.Core * 1000 + 3;
	}
}
