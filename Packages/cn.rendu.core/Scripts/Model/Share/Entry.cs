using System;
using RD.Core;

namespace RD
{
	/// <summary>空标记结构体，标识「Share 层初始化完成」事件（SourceGenerator 生成的 Initialize 流程会消费）。</summary>
	public struct InitShare
	{
	}

	/// <summary>空标记结构体，标识「Server 子模块初始化完成」事件。</summary>
	public struct InitServer
	{
	}

	/// <summary>空标记结构体，标识「Client 子模块初始化完成」事件。</summary>
	public struct InitClient
	{
	}

	/// <summary>
	/// 进程启动入口：注册 Bson/MemoryPack/Entity 序列化器、添加进程级 Singleton、创建 Main Fiber。
	/// 各业务 dll（actorlocation/ai/login/game/...）会在 <see cref="World.Instance"/> 上挂额外的 Singleton。
	/// </summary>
	public static class Entry
	{
		/// <summary>由 main() / Unity 启动钩子调（同步版本，目前未实现）。</summary>
		public static void Init()
		{

		}

		/// <summary>启动流程：fire-and-forget 调 <see cref="StartAsync"/>（不再等待）。</summary>
		public static void Start()
		{
			StartAsync().NoContext();
		}

		/// <summary>
		/// 异步启动：
		/// <list type="number">
		///   <item><see cref="WinPeriod.Init"/>：Windows 1ms 定时器精度</item>
		///   <item><see cref="MongoRegister.Init"/>：注册 Bson 序列化器 + 预热 classmap</item>
		///   <item><see cref="MemoryPackRegister.Init"/>：注册自定义 Formatter</item>
		///   <item><see cref="EntitySerializeRegister.Init"/>：注册 Entity 类型</item>
		///   <item>注册进程级 Singleton：<c>SceneTypeSingleton</c> / <c>ObjectPool</c> / <c>IdGenerator</c> / <c>OpcodeType</c> / <c>MessageQueue</c> / <c>NetServices</c> / <c>LogMsg</c></item>
		///   <item><c>CodeTypes.Instance.CodeProcess()</c>：SourceGenerator 生成的 Initialize 钩子</item>
		///   <item>异步加载 <c>ConfigLoader</c></item>
		///   <item>创建 <c>Main Fiber</c>（<see cref="FiberManager"/>）</item>
		/// </list>
		/// </summary>
		private static async Promise StartAsync()
		{
			WinPeriod.Init();

			// 注册Mongo type
			MongoRegister.Init();

			MemoryPackRegister.Init();

			// 注册Entity序列化器
			EntitySerializeRegister.Init();

			World.Instance.AddSingleton<SceneTypeSingleton, Type>(typeof(SceneType));
			World.Instance.AddSingleton<ObjectPool>();
			World.Instance.AddSingleton<IdGenerator>();
			World.Instance.AddSingleton<OpcodeType>();

			World.Instance.AddSingleton<MessageQueue>();
			World.Instance.AddSingleton<NetServices>();

			LogMsg logMsg = World.Instance.AddSingleton<LogMsg>();
			// logMsg.AddIgnore(LoginOuter.C2G_Ping);
			// logMsg.AddIgnore(LoginOuter.G2C_Ping);

			// 创建需要reload的code singleton
			CodeTypes.Instance.CodeProcess();

			await World.Instance.AddSingleton<ConfigLoader>().LoadAsync();

			await FiberManager.Instance.Create(SchedulerType.Main, SceneType.Main, 0, SceneType.Main, "Main");
		}
	}
}
