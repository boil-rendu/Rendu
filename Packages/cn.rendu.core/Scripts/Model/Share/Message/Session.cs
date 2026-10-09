using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 进程内 Session RPC 等待记录：绑定一个 <see cref="Promise{T}"/>（携 <see cref="IResponse"/>），业务侧 <c>await</c> 出参等回包。
	/// </summary>
	public readonly struct RpcInfo
	{
		/// <summary>request 类型，用于回包类型校验。</summary>
		public Type RequestType { get; }

		private readonly Promise<IResponse> tcs;

		public RpcInfo(Type requestType)
		{
			this.RequestType = requestType;

			this.tcs = Promise<IResponse>.Create(true);
		}

		/// <summary>回包到达时调：唤醒等待者。</summary>
		public void SetResult(IResponse response)
		{
			this.tcs.SetResult(response);
		}

		/// <summary>异常路径：唤醒等待者并传异常。</summary>
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

	/// <summary>
	/// 网络会话 Entity（与 <see cref="AService"/> 1:1）：持有底层 <see cref="AService"/>（TCP/KCP/WebSocket 等）、
	/// 维护本会话的 <c>rpcId → RpcInfo</c> 回包字典、最后收发时间、远端 IP 等。
	/// 由 <see cref="NetComponent"/> 在收到新连接时创建，对端断开时销毁。
	/// </summary>
	[ChildOf]
	public sealed class Session: Entity, IAwake<AService>, IDestroy
	{
		/// <summary>底层 <see cref="AService"/> 引用：用于实际发包/收包/关闭。</summary>
		public AService AService { get; set; }

		/// <summary>下一个待分配的 RPC Id（自增，Session 范围内唯一）。</summary>
		public int RpcId
		{
			get;
			set;
		}

		/// <summary>已发出但未收到响应的 RPC 字典：<c>rpcId → RpcInfo</c>。</summary>
		public readonly Dictionary<int, RpcInfo> requestCallbacks = new();

		/// <summary>最近一次收到数据的时间（Unix 毫秒）：用于 <see cref="SessionIdleCheckerComponent"/> 闲置超时判断。</summary>
		public long LastRecvTime
		{
			get;
			set;
		}

		/// <summary>最近一次发数据的时间（Unix 毫秒）。</summary>
		public long LastSendTime
		{
			get;
			set;
		}

		/// <summary>最近一次错误码：0 表示无错。</summary>
		public int Error
		{
			get;
			set;
		}

		/// <summary>对端 IP+端口（用于日志/统计）。</summary>
		public IPEndPoint RemoteAddress
		{
			get;
			set;
		}
	}
}
