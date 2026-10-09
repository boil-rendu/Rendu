using System;
using System.IO;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 已知对端 InstanceId 的 Actor 消息发送器：把"目标 ActorId + request 类型 + 是否需要异常回调"封装在一起，绑定一个 <see cref="Promise{T}"/>。
	/// 业务侧 <c>await sender.Wait()</c> 等回包，回包到达时由 <see cref="ProcessInnerSender"/> 调 <see cref="SetResult"/>。
	/// </summary>
	// 知道对方的instanceId，使用这个类发actor消息
	public readonly struct MessageSenderStruct
	{
		/// <summary>目标 ActorId（InstanceId 是关键，Address 部分用于定位 Fiber）。</summary>
		public ActorId ActorId { get; }

		/// <summary>request 类型，用于回包时类型校验。</summary>
		public Type RequestType { get; }

		private readonly Promise<IResponse> tcs;

		/// <summary>是否在 <paramref name="needException"/> = true 时把异常一并回传（否则吞掉异常返回 default response）。</summary>
		public bool NeedException { get; }

		public MessageSenderStruct(ActorId actorId, Type requestType, bool needException)
		{
			this.ActorId = actorId;

			this.RequestType = requestType;

			this.tcs = Promise<IResponse>.Create(true);
			this.NeedException = needException;
		}

		/// <summary>回包到达时由 <see cref="ProcessInnerSender"/> 调：唤醒等待者。</summary>
		public void SetResult(IResponse response)
		{
			this.tcs.SetResult(response);
		}

		/// <summary>异常路径（超时/对方销毁）：唤醒等待者并传异常。</summary>
		public void SetException(Exception exception)
		{
			this.tcs.SetException(exception);
		}

		/// <summary>业务侧 <c>await this.Wait()</c> 阻塞直到回包/超时/异常。</summary>
		public async Promise<IResponse> Wait()
		{
			return await this.tcs;
		}
	}
}
