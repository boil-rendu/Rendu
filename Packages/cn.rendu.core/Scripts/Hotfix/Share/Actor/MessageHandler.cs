using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// Actor 消息处理器基类（无回包）：<see cref="IMHandler"/> 的通用实现，业务继承 <c>class XxxHandler : MessageHandler&lt;E, Message&gt;</c> 后只需实现 <see cref="Run(E, Message)"/>。
	/// 处理过程：校验类型转换 → <c>await Run(...)</c>；类型不匹配打 <see cref="Log.Error"/> 直接 return（不回包）。
	/// </summary>
	public abstract class MessageHandler<E, Message>: HandlerObject, IMHandler where E : Entity where Message : class, IMessage
	{
		/// <summary>业务侧实现：处理到达的 Actor 消息。</summary>
		protected abstract Promise Run(E entity, Message message);

		/// <summary>Actor 消息处理入口：先做 Entity/Message 类型转换，再 await 业务 <see cref="Run"/>。</summary>
		public async Promise Handle(Entity entity, Address fromAddress, MessageObject actorMessage)
		{
			if (actorMessage is not Message msg)
			{
				Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
				return;
			}

			if (entity is not E e)
			{
				Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).Name} --{typeof (Message).FullName}");
				return;
			}

			await this.Run(e, msg);
		}

		/// <inheritdoc />
		public Type GetRequestType()
		{
			if (typeof (ILocationMessage).IsAssignableFrom(typeof (Message)))
			{
				Log.Error($"message is IActorLocationMessage but handler is AMActorHandler: {typeof (Message)}");
			}

			return typeof (Message);
		}

		/// <inheritdoc />
		public Type GetResponseType()
		{
			return null;
		}
	}

	/// <summary>
	/// Actor RPC 处理器基类（有回包）：业务继承后实现 <see cref="Run(E, Request, Response)"/>，框架自动：池化 response、捕获异常映射到 error、回包给 fromAddress。
	/// 异常处理：<see cref="RpcException"/> → <c>response.Error = exception.Error</c>；其他 <see cref="Exception"/> → <c>response.Error = ErrorCode.ERR_RpcFail</c>。
	/// </summary>
	public abstract class MessageHandler<E, Request, Response>: HandlerObject, IMHandler where E : Entity where Request : MessageObject, IRequest where Response : MessageObject, IResponse
	{
		/// <summary>业务侧实现：处理 RPC 请求，结果写入 <paramref name="response"/>（框架负责回包）。</summary>
		protected abstract Promise Run(E unit, Request request, Response response);

		/// <summary>Actor RPC 处理入口：池化 response + try/catch 异常映射 + 回包。</summary>
		public async Promise Handle(Entity entity, Address fromAddress, MessageObject actorMessage)
		{
			try
			{
				Fiber fiber = entity.Fiber();
				if (actorMessage is not Request request)
				{
					Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
					return;
				}

				if (entity is not E ee)
				{
					Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).FullName} --{typeof (Request).FullName}");
					return;
				}

				int rpcId = request.RpcId;
				Response response = ObjectPool.Fetch<Response>();
				try
				{
					await this.Run(ee, request, response);
				}
				catch (RpcException exception)
				{
					response.Error = exception.Error;
					response.Message = exception.ToString();
				}
				catch (Exception exception)
				{
					response.Error = ErrorCode.ERR_RpcFail;
					response.Message = exception.ToString();
				}

				response.RpcId = rpcId;
				fiber.Root.GetComponent<ProcessInnerSender>().Reply(fromAddress, response);
			}
			catch (Exception e)
			{
				throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
			}
		}

		/// <inheritdoc />
		public Type GetRequestType()
		{
			if (typeof (ILocationRequest).IsAssignableFrom(typeof (Request)))
			{
				Log.Error($"message is IActorLocationMessage but handler is AMActorRpcHandler: {typeof (Request)}");
			}

			return typeof (Request);
		}

		/// <inheritdoc />
		public Type GetResponseType()
		{
			return typeof (Response);
		}
	}
}
