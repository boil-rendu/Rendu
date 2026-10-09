using System.Net;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="NetComponent"/> 的系统扩展（Hotfix/Share）：KCP 网络组件的具体实现。
	/// <list type="bullet">
	///   <item><see cref="Awake"/>：建 <see cref="KService"/>（KCP 适配）并挂 Accept/Read/Error 回调。</item>
	///   <item><see cref="Update"/>：每帧调 <see cref="AService.Update"/> 轮转收发。</item>
	///   <item><see cref="OnAccept"/>：建 <see cref="Session"/> + 挂 <see cref="SessionAcceptTimeoutComponent"/>（5s 验证超时）+ <see cref="SessionIdleCheckerComponent"/>（2s 收包间隔 10s 闲置）。</item>
	///   <item><see cref="OnRead"/>：反序列化 → <see cref="LogMsg"/> 调试打日志 → publish <see cref="NetComponentOnRead"/>（业务层可拦截/打点）。</item>
	///   <item><see cref="Create(System.Net.IPEndPoint)"/> / <see cref="Create(System.Net.IPEndPoint, System.Net.IPEndPoint, uint)"/>：客户端主动建连的入口（后者走 Router）。</item>
	/// </list>
	/// </summary>
	[EntitySystemOf(typeof(NetComponent))]
	public static partial class NetComponentSystem
	{
		[EntitySystem]
		private static void Awake(this NetComponent self, IKcpTransport kcpTransport)
		{
			self.AService = new KService(kcpTransport, ServiceType.Outer);
			self.AService.AcceptCallback = self.OnAccept;
			self.AService.ReadCallback = self.OnRead;
			self.AService.ErrorCallback = self.OnError;
		}

		[EntitySystem]
		private static void Update(this NetComponent self)
		{
			self.AService.Update();
		}

		[EntitySystem]
		private static void Destroy(this NetComponent self)
		{
			self.AService.Dispose();
		}

		/// <summary>底层网络错误回调：按 channelId 找 Session，设 Error 码后 Dispose。</summary>
		private static void OnError(this NetComponent self, long channelId, int error)
		{
			Session session = self.GetChild<Session>(channelId);
			if (session == null)
			{
				return;
			}

			session.Error = error;
			session.Dispose();
		}

		// 这个channelId是由CreateAcceptChannelId生成的
		/// <summary>accept 回调：建 Session + 挂超时验证 + 闲置检测（防外挂长连接不发消息）。</summary>
		private static void OnAccept(this NetComponent self, long channelId, IPEndPoint ipEndPoint)
		{
			Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
			session.RemoteAddress = ipEndPoint;

			// 挂上这个组件，5秒就会删除session，所以客户端验证完成要删除这个组件。该组件的作用就是防止外挂一直连接不发消息也不进行权限验证
			session.AddComponent<SessionAcceptTimeoutComponent>();
			// 客户端连接，2秒检查一次recv消息，10秒没有消息则断开
			session.AddComponent<SessionIdleCheckerComponent>();
		}

		/// <summary>收包回调：更新 LastRecvTime → 反序列化 → <see cref="LogMsg"/> 调试 → publish <see cref="NetComponentOnRead"/> 事件（业务侧可订阅）。</summary>
		private static void OnRead(this NetComponent self, long channelId, MemoryBuffer memoryBuffer)
		{
			Session session = self.GetChild<Session>(channelId);
			if (session == null)
			{
				return;
			}
			session.LastRecvTime = TimeInfo.Instance.ClientNow();

			(ActorId _, object message) = MessageSerializeHelper.ToMessage(self.AService, memoryBuffer);
			self.AService.Recycle(memoryBuffer);

			LogMsg.Instance.Debug(self.Fiber(), message);

			EventSystem.Instance.Invoke(self.IScene.SceneType, new NetComponentOnRead() {Session = session, Message = message});
		}

		/// <summary>客户端主动建连：分一个 channelId → 建 Session → 挂闲置检测 → 调 <see cref="AService.Create"/> 发起连接，返回 Session。</summary>
		public static Session Create(this NetComponent self, IPEndPoint realIPEndPoint)
		{
			long channelId = NetServices.Instance.CreateConnectChannelId();
			Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
			session.RemoteAddress = realIPEndPoint;
			session.AddComponent<SessionIdleCheckerComponent>();

			self.AService.Create(session.Id, session.RemoteAddress);

			return session;
		}

		/// <summary>通过 Router 建连的版本：<paramref name="localConn"/> 作为 channelId（与 Router 端约定），先连到 Router 再由 Router 转发到 real 端。</summary>
		public static Session Create(this NetComponent self, IPEndPoint routerIPEndPoint, IPEndPoint realIPEndPoint, uint localConn)
		{
			long channelId = localConn;
			Session session = self.AddChildWithId<Session, AService>(channelId, self.AService);
			session.RemoteAddress = realIPEndPoint;
			session.AddComponent<SessionIdleCheckerComponent>();
			self.AService.Create(session.Id, routerIPEndPoint);
			return session;
		}
	}
}
