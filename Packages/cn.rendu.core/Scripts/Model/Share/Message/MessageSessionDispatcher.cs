using System.Collections.Generic;
using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="MessageSessionDispatcher"/> 内部用的 (sceneType, handler) 二元组：一个 opcode 可对应多个（不同 Scene 多个 handler）。
	/// </summary>
	public struct MessageSessionDispatcherInfo
	{
		/// <summary>handler 生效的 SceneType。</summary>
		public int SceneType { get; }
		/// <summary>具体 handler 实例。</summary>
		public IMessageSessionHandler IMHandler { get; }

		public MessageSessionDispatcherInfo(int sceneType, IMessageSessionHandler imHandler)
		{
			this.SceneType = sceneType;
			this.IMHandler = imHandler;
		}
	}

	/// <summary>
	/// Session 消息路由器（<see cref="Singleton{T}"/>，<c>[CodeProcess]</c> 启动期初始化）：
	/// Awake 时反射收集标 <c>[MessageSessionHandler]</c> 的 <see cref="IMessageSessionHandler"/>，按 opcode 索引为字典。
	/// 业务侧 <see cref="NetComponent"/> 收到包后调 <see cref="Handle(Session, object)"/>：先按 opcode 查 handlers，
	/// 再按当前 session 所在 SceneType 过滤（用 <see cref="SceneTypeSingleton.IsSame"/>）后逐个执行，单个 handler 异常被 <see cref="Log.Error"/> 吞掉不影响后续。
	/// </summary>
	[CodeProcess]
	public class MessageSessionDispatcher: Singleton<MessageSessionDispatcher>, ISingletonAwake
	{
		private readonly Dictionary<ushort, List<MessageSessionDispatcherInfo>> handlers = new();

		public void Awake()
		{
			HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (MessageSessionHandlerAttribute));

			foreach (Type type in types)
			{
				IMessageSessionHandler iMessageSessionHandler = Activator.CreateInstance(type) as IMessageSessionHandler;
				if (iMessageSessionHandler == null)
				{
					Log.Error($"message handle {type.Name} 需要继承 IMHandler");
					continue;
				}

				object[] attrs = type.GetCustomAttributes(typeof(MessageSessionHandlerAttribute), true);

				foreach (object attr in attrs)
				{
					MessageSessionHandlerAttribute messageSessionHandlerAttribute = attr as MessageSessionHandlerAttribute;

					Type messageType = iMessageSessionHandler.GetMessageType();

					ushort opcode = OpcodeType.Instance.GetOpcode(messageType);
					if (opcode == 0)
					{
						Log.Error($"消息opcode为0: {messageType.Name}");
						continue;
					}

					MessageSessionDispatcherInfo messageSessionDispatcherInfo = new (messageSessionHandlerAttribute.SceneType, iMessageSessionHandler);
					this.RegisterHandler(opcode, messageSessionDispatcherInfo);
				}
			}
		}

		/// <summary>opcode → handlers 注册：单 opcode 可注册多个 handler（按 SceneType 过滤）。</summary>
		private void RegisterHandler(ushort opcode, MessageSessionDispatcherInfo handler)
		{
			if (!this.handlers.ContainsKey(opcode))
			{
				this.handlers.Add(opcode, new List<MessageSessionDispatcherInfo>());
			}

			this.handlers[opcode].Add(handler);
		}

		/// <summary>分发入口：按 <paramref name="message"/> 的 opcode 查 handlers，按当前 <see cref="Session"/> 所在 SceneType 过滤后逐个调 <see cref="IMessageSessionHandler.Handle(Session, object)"/>，单 handler 抛异常被 <see cref="Log.Error"/> 吞掉不中断后续。</summary>
		public void Handle(Session session, object message)
		{
			List<MessageSessionDispatcherInfo> actions;
			ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());
			if (!this.handlers.TryGetValue(opcode, out actions))
			{
				Log.Error($"消息没有处理: {opcode} {message}");
				return;
			}

			int sceneType = session.IScene.SceneType;
			foreach (MessageSessionDispatcherInfo ev in actions)
			{
				if (!SceneTypeSingleton.IsSame(ev.SceneType, sceneType))
				{
					continue;
				}

				try
				{
					ev.IMHandler.Handle(session, message);
				}
				catch (Exception e)
				{
					Log.Error(e);
				}
			}
		}
	}
}
