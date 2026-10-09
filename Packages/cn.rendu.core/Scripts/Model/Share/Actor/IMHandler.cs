using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 消息处理器接口：每种 request 消息对应一个 <see cref="IMHandler"/> 实例，
	/// 由 SourceGenerator 在编译期扫描 <c>class XxxHandler : IMHandler</c> 自动注册到 <see cref="EventSystem"/>。
	/// </summary>
	public interface IMHandler
	{
		/// <summary>实际处理逻辑：<c>entity</c> 是消息接收方（MailBox 所在 Entity），<paramref name="fromAddress"/> 是发来方地址，<paramref name="actorMessage"/> 是消息体。返回 <see cref="Promise"/> 表示异步/链式处理。</summary>
		Promise Handle(Entity entity, Address fromAddress, MessageObject actorMessage);

		/// <summary>声明本 handler 接收的 request 类型（用于 <see cref="OpcodeType"/> 反查）。</summary>
		Type GetRequestType();

		/// <summary>声明本 handler 返回的 response 类型（同上）。</summary>
		Type GetResponseType();
	}
}
