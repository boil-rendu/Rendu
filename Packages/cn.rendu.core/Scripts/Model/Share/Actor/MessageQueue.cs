using System.Collections.Concurrent;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
	/// <summary>
	/// Actor 消息队列元素：来源 <see cref="ActorId"/> + 消息体。
	/// </summary>
	public struct MessageInfo
	{
		/// <summary>发送方的 ActorId（其中 <c>Address</c> 部分用于回包路由）。</summary>
		public ActorId ActorId;
		/// <summary>消息体（含 Opcode + 业务数据）。</summary>
		public MessageObject MessageObject;
	}

	/// <summary>
	/// 进程级 Actor 消息总线（<see cref="Singleton{T}"/>）：以 <c>actorId.Address.Fiber</c> 为 key 维护 <see cref="System.Collections.Concurrent.ConcurrentQueue{T}"/>，
	/// 跨纤程/跨进程的 Actor 消息先 Enqueue 到这里，对应 Fiber 的 <c>MailBoxComponent</c> 在 Update 时 Fetch 消费。
	/// </summary>
	public class MessageQueue: Singleton<MessageQueue>, ISingletonAwake
	{
		private readonly ConcurrentDictionary<int, ConcurrentQueue<MessageInfo>> messages = new();

		public void Awake()
		{
		}

		/// <summary>外部投递：用 <paramref name="actorId"/>.Address 当 key 入队（无队列时静默丢弃返回 false）。</summary>
		public bool Send(ActorId actorId, MessageObject messageObject)
		{
			return this.Send(actorId.Address, actorId, messageObject);
		}

		/// <summary>回包语义：与 <see cref="Send"/> 相同但返回 <c>void</c>（表示必定发到 <paramref name="actorId"/>.Address 所在 Fiber）。</summary>
		public void Reply(ActorId actorId, MessageObject messageObject)
		{
			this.Send(actorId.Address, actorId, messageObject);
		}

		/// <summary>Send 的完整版：来源地址 + 目标 ActorId + 消息体。</summary>
		public bool Send(Address fromAddress, ActorId actorId, MessageObject messageObject)
		{
			if (!this.messages.TryGetValue(actorId.Address.Fiber, out var queue))
			{
				return false;
			}
			queue.Enqueue(new MessageInfo() {ActorId = new ActorId(fromAddress, actorId.InstanceId), MessageObject = messageObject});
			return true;
		}

		/// <summary>Fiber Update 时调：把 <paramref name="fiberId"/> 队列里最多 <paramref name="count"/> 条消息搬到 <paramref name="list"/>。无队列直接 return。</summary>
		public void Fetch(int fiberId, int count, List<MessageInfo> list)
		{
			if (!this.messages.TryGetValue(fiberId, out var queue))
			{
				return;
			}

			for (int i = 0; i < count; ++i)
			{
				if (!queue.TryDequeue(out MessageInfo message))
				{
					break;
				}
				list.Add(message);
			}
		}

		/// <summary>为指定 FiberId 创建一个空队列（在 Fiber 创建时调）。</summary>
		public void AddQueue(int fiberId)
		{
			var queue = new ConcurrentQueue<MessageInfo>();
			this.messages[fiberId] = queue;
		}

		/// <summary>销毁指定 FiberId 的队列（Fiber 销毁时调）。</summary>
		public void RemoveQueue(int fiberId)
		{
			this.messages.TryRemove(fiberId, out _);
		}
	}
}
