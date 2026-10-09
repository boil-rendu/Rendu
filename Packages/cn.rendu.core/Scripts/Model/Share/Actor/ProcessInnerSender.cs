using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 进程内 Actor 消息发送器（每 Scene 一份）：缓存当前 Scene 已发出的 Actor RPC 请求（<c>rpcId → MessageSenderStruct</c>），
	/// 在 Update 时从 <see cref="MessageQueue"/> Fetch 并按 rpcId 匹配回调。
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class ProcessInnerSender: Entity, IAwake, IDestroy, IUpdate
	{
		/// <summary>RPC 默认超时（40s）。</summary>
		public const long TIMEOUT_TIME = 40 * 1000;

		/// <summary>下一个待分配的 RPC Id（自增，Scene 范围内唯一）。</summary>
		public int RpcId;

		/// <summary>已发出但未收到响应的 RPC 字典：<c>rpcId → MessageSenderStruct</c>，用于回包时按 id 唤醒对应 tcs。</summary>
		public readonly Dictionary<int, MessageSenderStruct> requestCallback = new();

		/// <summary>Update 时复用的中间列表：避免每帧 <c>new List&lt;MessageInfo&gt;</c>。</summary>
		public readonly List<MessageInfo> list = new();
	}
}
