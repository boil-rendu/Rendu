using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="ProcessInnerSender"/> 的系统扩展（Hotfix/Share）：进程内 Actor 消息分发中心。
	/// <list type="bullet">
	///   <item><see cref="Awake"/>：在 <see cref="MessageQueue"/> 上为当前 FiberId 建一条收件箱。</item>
	///   <item><see cref="Update"/>：每帧从 <see cref="MessageQueue"/> Fetch 最多 1000 条，分类处理：<see cref="IResponse"/> 走 RPC 回包回调，否则按 InstanceId 找到 MailBox 入队。</item>
	///   <item><see cref="Destroy"/>：移除当前 FiberId 的收件箱。</item>
	///   <item><see cref="Call"/>：发 Actor RPC，自带 <see cref="ProcessInnerSender.TIMEOUT_TIME"/> 40s 超时，&gt; 200ms 警告。</item>
	/// </list>
	/// </summary>
	[EntitySystemOf(typeof(ProcessInnerSender))]
	public static partial class ProcessInnerSenderSystem
	{
		[EntitySystem]
		private static void Destroy(this ProcessInnerSender self)
		{
			Fiber fiber = self.Fiber();
			MessageQueue.Instance.RemoveQueue(fiber.Id);
		}

		[EntitySystem]
		private static void Awake(this ProcessInnerSender self)
		{
			Fiber fiber = self.Fiber();
			MessageQueue.Instance.AddQueue(fiber.Id);
		}

		[EntitySystem]
		private static void Update(this ProcessInnerSender self)
		{
			self.list.Clear();
			Fiber fiber = self.Fiber();
			MessageQueue.Instance.Fetch(fiber.Id, 1000, self.list);

			foreach (MessageInfo actorMessageInfo in self.list)
			{
				self.HandleMessage(fiber, actorMessageInfo);
			}
		}

		/// <summary>单条消息处理：<see cref="IResponse"/> → <see cref="HandleIActorResponse"/>；其他 → 查 MailBox 入队（找不到回 <see cref="ErrorCode.ERR_NotFoundActor"/>）。</summary>
		private static void HandleMessage(this ProcessInnerSender self, Fiber fiber, in MessageInfo messageInfo)
		{
			if (messageInfo.MessageObject is IResponse response)
			{
				self.HandleIActorResponse(response);
				return;
			}

			ActorId actorId = messageInfo.ActorId;
			MessageObject message = messageInfo.MessageObject;

			Entity entity = self.Fiber().Mailboxes.Get(actorId.InstanceId);
			MailBoxComponent mailBoxComponent = entity as MailBoxComponent;
			if (mailBoxComponent == null)
			{
				Log.Warning($"actor not found mailbox, from: {actorId} current: {fiber.Address} {message}");
				if (message is IRequest request)
				{
					IResponse resp = MessageHelper.CreateResponse(request.GetType(), request.RpcId, ErrorCode.ERR_NotFoundActor);
					self.Reply(actorId.Address, resp);
				}
				return;
			}
			mailBoxComponent.Add(actorId.Address, message);
		}

		/// <summary>回包处理：按 <paramref name="response"/>.RpcId 找等待中的 <see cref="MessageSenderStruct"/>，转给 <see cref="Run"/>。</summary>
		private static void HandleIActorResponse(this ProcessInnerSender self, IResponse response)
		{
			if (!self.requestCallback.Remove(response.RpcId, out MessageSenderStruct actorMessageSender))
			{
				return;
			}
			Run(actorMessageSender, response);
		}

		/// <summary>回包分类：<see cref="ErrorCode.ERR_MessageTimeout"/> → SetException；<see cref="ErrorCode.IsRpcNeedThrowException"/> + <c>NeedException</c> → SetException；否则 SetResult。</summary>
		private static void Run(MessageSenderStruct self, IResponse response)
		{
			if (response.Error == ErrorCode.ERR_MessageTimeout)
			{
				self.SetException(new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.RequestType.FullName}, response: {response}"));
				return;
			}

			if (self.NeedException && ErrorCode.IsRpcNeedThrowException(response.Error))
			{
				self.SetException(new RpcException(response.Error, $"Rpc error: actorId: {self.ActorId} request: {self.RequestType.FullName}, response: {response}"));
				return;
			}

			self.SetResult(response);
		}

		/// <summary>回包：把 <paramref name="message"/> 当作对端的响应（目标 actorId 来自 fromAddress）。</summary>
		public static void Reply(this ProcessInnerSender self, Address fromAddress, IResponse message)
		{
			self.SendInner(new ActorId(fromAddress, 0), (MessageObject)message);
		}

		/// <summary>发包入口：扔进 <see cref="MessageQueue"/>（同进程）；跨进程会抛异常（应走 <see cref="ActorLocationSender"/>）。</summary>
		public static void Send(this ProcessInnerSender self, ActorId actorId, IMessage message)
		{
			self.SendInner(actorId, (MessageObject)message);
		}

		/// <summary>底层 Send：检查同进程后入 <see cref="MessageQueue"/>；跨进程抛异常（提示应走 ActorLocationSender）。</summary>
		private static bool SendInner(this ProcessInnerSender self, ActorId actorId, MessageObject message)
		{
			Fiber fiber = self.Fiber();

			// 如果发向同一个进程，则扔到消息队列中
			if (actorId.Process != fiber.Process)
			{
				throw new Exception($"actor inner process diff: {actorId.Process} {fiber.Process}");
			}

			return MessageQueue.Instance.Send(fiber.Address, actorId, message);
		}

		/// <summary>取下一个 RPC Id（自增）。</summary>
		private static int GetRpcId(this ProcessInnerSender self)
		{
			return ++self.RpcId;
		}

		/// <summary>
		/// 发起 Actor RPC：分配 rpcId → 写入 request.RpcId → 入 <see cref="MessageQueue"/> → 注册 <see cref="MessageSenderStruct"/> → 启超时定时器（<see cref="ProcessInnerSender.TIMEOUT_TIME"/> = 40s）→ <c>await tcs</c>。
		/// <paramref name="needException"/> = true 时错误按 <see cref="ErrorCode.IsRpcNeedThrowException"/> 转异常；&gt; 200ms 警告。
		/// 找不到目标 FiberId 立即回 <see cref="ErrorCode.ERR_NotFoundActor"/>（不抛）。
		/// </summary>
		public static async Promise<IResponse> Call(
				this ProcessInnerSender self,
				ActorId actorId,
				IRequest request,
				bool needException = true
		)
		{
			int rpcId = self.GetRpcId();
			request.RpcId = rpcId;

			if (actorId == default)
			{
				throw new Exception($"actor id is 0: {request}");
			}

			Fiber fiber = self.Fiber();
			if (fiber.Process != actorId.Process)
			{
				throw new Exception($"actor inner process diff: {actorId.Process} {fiber.Process}");
			}

			Type requestType = request.GetType();

			IResponse response;
			if (!self.SendInner(actorId, (MessageObject)request))  // 纤程不存在
			{
				response = MessageHelper.CreateResponse(requestType, rpcId, ErrorCode.ERR_NotFoundActor);
				return response;
			}

			MessageSenderStruct messageSenderStruct = new(actorId, requestType, needException);
			self.requestCallback.Add(rpcId, messageSenderStruct);

			async Promise Timeout()
			{
				await fiber.Root.GetComponent<TimerComponent>().WaitAsync(ProcessInnerSender.TIMEOUT_TIME);

				if (!self.requestCallback.Remove(rpcId, out MessageSenderStruct action))
				{
					return;
				}

				if (needException)
				{
					action.SetException(new Exception($"actor sender timeout: {requestType.FullName}"));
				}
				else
				{
					IResponse response = MessageHelper.CreateResponse(requestType, rpcId, ErrorCode.ERR_Timeout);
					action.SetResult(response);
				}
			}

			Timeout().NoContext();

			long beginTime = TimeInfo.Instance.ServerFrameTime();

			response = await messageSenderStruct.Wait();

			long endTime = TimeInfo.Instance.ServerFrameTime();

			long costTime = endTime - beginTime;
			if (costTime > 200)
			{
				Log.Warning($"actor rpc time > 200: {costTime} {requestType.FullName}");
			}

			return response;
		}
	}
}
