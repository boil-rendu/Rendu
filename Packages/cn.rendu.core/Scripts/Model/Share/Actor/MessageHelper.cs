using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 消息工具门面：集中放消息相关的零碎工具。
	/// </summary>
	public static class MessageHelper
	{
		/// <summary>
		/// 构造并池化一个 <see cref="IResponse"/>：用 <see cref="OpcodeType.Instance.GetResponseType"/> 查 request 对应的 response 类型（项目用 <c>[ResponseType]</c> 特性登记），
		/// 从 <see cref="ObjectPool"/> 取实例，写入 <paramref name="rpcId"/> 和 <paramref name="error"/> 后返回。
		/// </summary>
		public static IResponse CreateResponse(Type requestType, int rpcId, int error)
		{
			Type responseType = OpcodeType.Instance.GetResponseType(requestType);
			IResponse response = (IResponse)ObjectPool.Fetch(responseType);
			response.Error = error;
			response.RpcId = rpcId;
			return response;
		}
	}
}
