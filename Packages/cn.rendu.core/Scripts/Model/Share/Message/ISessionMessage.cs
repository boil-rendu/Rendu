using RD.Core;

namespace RD
{

	/// <summary>Session 层消息 marker：所有走网络收发的 <see cref="IMessage"/> 子接口都应同时标 <see cref="ISessionMessage"/>，以便 <see cref="MessageSessionDispatcher"/> 区分。</summary>
	public interface ISessionMessage: IMessage
	{
	}

	/// <summary>Session 层 request 消息：包含 <see cref="IRequest.RpcId"/>，可被对方 <c>Response</c> 回包。</summary>
	public interface ISessionRequest: ISessionMessage, IRequest
	{
	}

	/// <summary>Session 层 response 消息：回包用，含 <see cref="IResponse.Error"/> 和 <see cref="IResponse.RpcId"/>。</summary>
	public interface ISessionResponse: ISessionMessage, IResponse
	{

	}
}
