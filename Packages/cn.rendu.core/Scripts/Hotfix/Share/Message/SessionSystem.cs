using System;
using System.Linq;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="Session"/> 的系统扩展（Hotfix/Share）：网络会话核心。
	/// <list type="bullet">
	///   <item><see cref="Awake"/>：记 AService + 初始化 LastRecv/LastSend 时间 + 清 requestCallbacks。</item>
	///   <item><see cref="Destroy"/>：从 AService 移除 + 异常唤醒所有等待中的 RPC + 清字典。</item>
	///   <item><see cref="Call(IRequest)"/>：发 RPC，关联当前上下文 <see cref="PromiseCancellationToken"/> 实现可取消；<see cref="Call(IRequest, int)"/> 加超时变体。</item>
	///   <item><see cref="Send(IMessage)"/> / <see cref="Send(ActorId, IMessage)"/>：发单向包，更新 LastSendTime + <see cref="LogMsg"/> 调试 + 序列化为 <see cref="MemoryBuffer"/> 后 <see cref="AService.Send"/>。</item>
	/// </list>
	/// </summary>
	[EntitySystemOf(typeof(Session))]
	public static partial class SessionSystem
	{
		[EntitySystem]
		private static void Awake(this Session self, AService aService)
		{
			self.AService = aService;
			long timeNow = TimeInfo.Instance.ClientNow();
			self.LastRecvTime = timeNow;
			self.LastSendTime = timeNow;

			self.requestCallbacks.Clear();

			Log.Info($"session create: zone: {self.Zone()} id: {self.Id} {timeNow} ");
		}

		[EntitySystem]
		private static void Destroy(this Session self)
		{
			self.AService.Remove(self.Id, self.Error);

			foreach (RpcInfo responseCallback in self.requestCallbacks.Values.ToArray())
			{
				responseCallback.SetException(new RpcException(self.Error, $"session dispose: {self.Id} {self.RemoteAddress}"));
			}

			Log.Info($"session dispose: {self.RemoteAddress} id: {self.Id} ErrorCode: {self.Error}, please see ErrorCode.cs! {TimeInfo.Instance.ClientNow()}");

			self.requestCallbacks.Clear();
		}

		/// <summary>回包处理：按 <paramref name="response"/>.RpcId 找等待中的 <see cref="RpcInfo"/> 并 SetResult；找不到静默 return（超时/重复回包都走这里）。</summary>
		public static void OnResponse(this Session self, IResponse response)
		{
			if (!self.requestCallbacks.Remove(response.RpcId, out RpcInfo action))
			{
				return;
			}
			action.SetResult(response);
		}

		/// <summary>
		/// 发起 Session RPC：分配 rpcId → 注册 <see cref="RpcInfo"/> → 关联 <see cref="PromiseCancellationToken"/> 实现可取消（Cancel 时回 <see cref="ErrorCode.ERR_Cancel"/>） → <c>await tcs</c>。
		/// 取消/超时的响应通过 <c>Activator.CreateInstance</c> 临时构造（不走池，因为 await 完不归还）。
		/// </summary>
		public static async Promise<IResponse> Call(this Session self, IRequest request)
		{
			int rpcId = ++self.RpcId;
			RpcInfo rpcInfo = new(request.GetType());
			self.requestCallbacks[rpcId] = rpcInfo;

			request.RpcId = rpcId;

			self.Send(request);

			void CancelAction()
			{
				if (!self.requestCallbacks.Remove(rpcId, out RpcInfo action))
				{
					return;
				}

				Type responseType = OpcodeType.Instance.GetResponseType(action.RequestType);
				IResponse response = (IResponse) Activator.CreateInstance(responseType);
				response.Error = ErrorCode.ERR_Cancel;
				action.SetResult(response);
			}

			PromiseCancellationToken cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
			IResponse ret;
			try
			{
				cancellationToken?.Add(CancelAction);
				ret = await rpcInfo.Wait();
			}
			finally
			{
				cancellationToken?.Remove(CancelAction);
			}
			return ret;
		}

		/// <summary><see cref="Call(IRequest)"/> 的超时变体：用 <see cref="Promise.TimeoutAsync(int)"/> 包装。</summary>
		public static async Promise<IResponse> Call(this Session self, IRequest request, int timeout)
		{
			return await self.Call(request).TimeoutAsync(timeout);
		}

		/// <summary>发单向包（无 ActorId）：更新 LastSendTime + <see cref="LogMsg"/> 调试 + 序列化 + <see cref="AService.Send"/>。</summary>
		public static void Send(this Session self, IMessage message)
		{
			self.Send(default, message);
		}

		/// <summary>发单向包（带 ActorId）：用 <see cref="MessageSerializeHelper.ToMemoryBuffer"/> 序列化后 <see cref="AService.Send"/>。</summary>
		public static void Send(this Session self, ActorId actorId, IMessage message)
		{
			self.LastSendTime = TimeInfo.Instance.ClientNow();
			LogMsg.Instance.Debug(self.Fiber(), message);

			(ushort opcode, MemoryBuffer memoryBuffer) = MessageSerializeHelper.ToMemoryBuffer(self.AService, actorId, message);
			self.AService.Send(self.Id, memoryBuffer);
		}
	}
}
