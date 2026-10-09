using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// Session 消息处理器基类（无回包，单向消息）：<see cref="IMessageSessionHandler"/> 的通用实现，
	/// 业务继承 <c>class XxxHandler : MessageSessionHandler&lt;Message&gt;</c> 后只实现 <see cref="Run(Session, Message)"/>。
	/// 注意：与 Actor 体系走 MailBox 不同，Session 消息是「不队列化、收到就处理」的直达路径。
	/// </summary>
	public abstract class MessageSessionHandler<Message>: HandlerObject, IMessageSessionHandler where Message : MessageObject
	{
		/// <summary>业务侧实现：处理到达的 Session 消息。</summary>
		protected abstract Promise Run(Session session, Message message);

		/// <summary>Entry 入口：起 fire-and-forget <see cref="HandleAsync"/>（不 await，session handler 同步路径下不允许 await）。</summary>
		public void Handle(Session session, object msg)
		{
			HandleAsync(session, msg).NoContext();
		}

		/// <summary>实际处理：校验 session 存活 + 类型转换 + <c>await Run</c>。</summary>
		private async Promise HandleAsync(Session session, object message)
		{
			if (message == null)
			{
				Log.Error($"消息类型转换错误: {message.GetType().FullName} to {typeof (Message).Name}");
				return;
			}

			if (session.IsDisposed)
			{
				Log.Error($"session disconnect {message}");
				return;
			}

			await this.Run(session, (Message)message);
		}

		/// <inheritdoc />
		public Type GetMessageType()
		{
			return typeof (Message);
		}

		/// <inheritdoc />
		public Type GetResponseType()
		{
			return null;
		}
	}

	/// <summary>
	/// Session RPC 处理器基类（有回包）：业务继承后实现 <see cref="Run(Session, Request, Response)"/>，框架自动：池化 response、捕获异常映射到 error、回包（用 <c>using</c> 保证 dispose）。
	/// 异常处理：<see cref="RpcException"/> → <c>response.Error = exception.Error</c>；其他 → <c>response.Error = ErrorCode.ERR_RpcFail</c>（都不回堆栈给客户端）。
	/// 回包前再校验 <c>session.InstanceId</c> 没变（防止 await 期间 session 被替换/重连）。
	/// </summary>
	public abstract class MessageSessionHandler<Request, Response>: HandlerObject, IMessageSessionHandler where Request : MessageObject, IRequest where Response : MessageObject, IResponse
	{
		/// <summary>业务侧实现：处理 Session RPC，结果写入 <paramref name="response"/>（框架负责回包）。</summary>
		protected abstract Promise Run(Session session, Request request, Response response);

		/// <summary>Entry 入口：起 fire-and-forget <see cref="HandleAsync"/>。</summary>
		public void Handle(Session session, object message)
		{
			HandleAsync(session, message).NoContext();
		}

		/// <summary>实际处理：池化 response（using 保证 dispose）→ try/catch 异常映射 → session 存活校验 → 回包。</summary>
		private async Promise HandleAsync(Session session, object message)
		{
			try
			{
				Request request = message as Request;
				if (request == null)
				{
					throw new Exception($"消息类型转换错误: {message.GetType().FullName} to {typeof (Request).FullName}");
				}

				int rpcId = request.RpcId;
				long instanceId = session.InstanceId;

				// 这里用using很安全，因为后面是session发送出去了
				using Response response = ObjectPool.Fetch<Response>();
				try
				{
					await this.Run(session, request, response);
				}
				catch (RpcException exception)
				{
					// 这里不能返回堆栈给客户端
					Log.Error(exception.ToString());
					response.Error = exception.Error;
				}
				catch (Exception exception)
				{
					// 这里不能返回堆栈给客户端
					Log.Error(exception.ToString());
					response.Error = ErrorCode.ERR_RpcFail;
				}

				// 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
				if (session.InstanceId != instanceId)
				{
					return;
				}

				response.RpcId = rpcId; // 在这里设置rpcId是为了防止在Run中不小心修改rpcId字段
				session.Send(response);
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
			}
		}

		/// <inheritdoc />
		public Type GetMessageType()
		{
			return typeof (Request);
		}

		/// <inheritdoc />
		public Type GetResponseType()
		{
			return typeof (Response);
		}
	}
}
