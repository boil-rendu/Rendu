using System;

namespace RD
{
	/// <summary>
	/// Session 消息处理器接口：每种 <see cref="ISessionMessage"/> 对应一个 <see cref="IMessageSessionHandler"/> 实例，
	/// 启动期被 <see cref="MessageSessionDispatcher"/> 自动收集并按 opcode 索引。
	/// 区别于 <see cref="IMHandler"/>（Actor 消息走 MailBox），<see cref="IMessageSessionHandler"/> 是直接走 Session 收发的「不队列化」消息。
	/// </summary>
	public interface IMessageSessionHandler
	{
		/// <summary>实际处理逻辑：<paramref name="session"/> 是收包 Session，<paramref name="message"/> 是反序列化好的消息体（同步，无 Promise）。</summary>
		void Handle(Session session, object message);

		/// <summary>声明本 handler 接收的消息类型（用于 <see cref="OpcodeType"/> 反查）。</summary>
		Type GetMessageType();

		/// <summary>声明本 handler 返回的 response 类型（同上；若是单向消息可返回 <c>typeof(void)</c>）。</summary>
		Type GetResponseType();
	}
}
