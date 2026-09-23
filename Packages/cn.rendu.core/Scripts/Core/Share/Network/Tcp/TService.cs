using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RD.Core
{
	/// <summary>TCP 异步操作类型，用于 <see cref="TArgs"/> 中标记需要驱动 TChannel 执行的指令。</summary>
	public enum TcpOp
	{
		/// <summary>驱动 TChannel 发起发送（StartSend）。</summary>
		StartSend,
		/// <summary>驱动 TChannel 发起接收（StartRecv）。</summary>
		StartRecv,
		/// <summary>驱动 TChannel 发起连接（ConnectAsync）。</summary>
		Connect,
	}
	
	/// <summary>TCP 服务事件队列（<see cref="TService.Queue"/>）中的元素。
	/// 元素有两种语义：当 <see cref="SocketAsyncEventArgs"/> 非空时表示一次 socket 完成事件（按 <see cref="SocketAsyncOperation"/> 分派）；
	/// 当其为空时表示一条驱动指令，按 <see cref="Op"/> 指定的 <see cref="TcpOp"/> 驱动对应 TChannel。</summary>
	public struct TArgs
	{
		/// <summary>需要驱动的异步操作（None 表示本元素携带的是 socket 完成事件而非驱动指令）。</summary>
		public TcpOp Op;
		/// <summary>目标通道 Id。</summary>
		public long ChannelId;
		/// <summary>socket 异步完成事件参数；为空表示这是一个驱动指令。</summary>
		public SocketAsyncEventArgs SocketAsyncEventArgs;
	}

	/// <summary>
	/// 基于 TCP 的传输服务（Service），管理一组 <see cref="TChannel"/> 的生命周期与收发。
	/// <para>与 <see cref="KService"/>（KCP over UDP）不同，TCP 的可靠传输、重传与超时由操作系统内核提供，
	/// 因此本类无需像 KService 那样抽取连接表（KChannelTable）、超时调度（KTimeoutScheduler）与路由（Router）等组件。</para>
	/// <para>职责扁平且单文件足够清晰，故不拆分 partial；主要组成：</para>
	/// <list type="bullet">
	///   <item><b>接入</b>：作为服务端时由 <see cref="AcceptAsync"/> 监听，新连接经 <see cref="OnAcceptComplete"/> 创建 <see cref="TChannel"/> 并入表。</item>
	///   <item><b>通道表</b>：以单个 <c>Dictionary&lt;long, TChannel&gt;</c>（idChannels）维护活跃通道，提供 Create/Get/Remove/Dispose。</item>
	///   <item><b>事件驱动</b>：socket 完成事件与驱动指令统一入 <see cref="Queue"/>，由 <see cref="Update"/> 取出分派到对应 <see cref="TChannel"/> 回调。</item>
	/// </list>
	/// <para>本类与 KService 同继承 <see cref="AService"/>、override 同一组契约（Create/Get/Remove/Send/Update），但内部复杂度更低。</para>
	/// </summary>
	public sealed class TService : AService
	{
		/// <summary>活跃通道表：channelId -> TChannel。</summary>
		private readonly Dictionary<long, TChannel> idChannels = new();

		/// <summary>监听 accept 专用的 SocketAsyncEventArgs（完成回调统一入队，延迟到 Update 处理）。</summary>
		private readonly SocketAsyncEventArgs innArgs = new();
		
		/// <summary>服务端监听 socket；客户端构造时为空。</summary>
		private Socket acceptor;

		/// <summary>socket 完成事件与驱动指令的队列，由 Update 每帧消费。</summary>
		public ConcurrentQueue<TArgs> Queue = new();

		/// <summary>客户端构造：仅设置服务类型，不创建监听 socket；连接由上层通过 Create + TChannel 发起。</summary>
		public TService(AddressFamily addressFamily, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
		}

		/// <summary>服务端构造：创建监听 socket、绑定并 listen，注册 accept 完成回调后发起首次异步 accept。</summary>
		public TService(IPEndPoint ipEndPoint, ServiceType serviceType)
		{
			this.ServiceType = serviceType;
			this.acceptor = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			// 容易出问题，先注释掉，按需开启
			//this.acceptor.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.innArgs.Completed += this.OnComplete;
			try
			{
				this.acceptor.Bind(ipEndPoint);
			}
			catch (Exception e)
			{
				throw new Exception($"bind error: {ipEndPoint}", e);
			}
			
			this.acceptor.Listen(1000);
			
			this.AcceptAsync();
		}

		/// <summary>
		/// accept 完成回调（仅用于监听）：把完成的 accept 事件原样入队，
		/// 由 <see cref="Update"/> 在帧循环中处理，避免在 IO 回调上下文直接执行业务逻辑。
		/// </summary>
		private void OnComplete(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					this.Queue.Enqueue(new TArgs() {SocketAsyncEventArgs = e});
					break;
				default:
					throw new Exception($"socket error: {e.LastOperation}");
			}
		}

		/// <summary>
		/// 处理一个接入完成的连接：校验监听 socket 仍存在、检查错误后创建 <see cref="TChannel"/> 入表，
		/// 触发 <see cref="AService.AcceptCallback"/>；无论成功失败都立即重新发起 <see cref="AcceptAsync"/> 以持续监听。
		/// </summary>
		private void OnAcceptComplete(SocketError socketError, Socket acceptSocket)
		{
			if (this.acceptor == null)
			{
				return;
			}

			if (socketError != SocketError.Success)
			{
				Log.Error($"accept error {socketError}");
				this.AcceptAsync();
				return;
			}

			try
			{
				long id = NetServices.Instance.CreateAcceptChannelId();
				TChannel channel = new TChannel(id, acceptSocket, this);
				this.idChannels.Add(channel.Id, channel);
				long channelId = channel.Id;
				
				this.AcceptCallback(channelId, channel.RemoteAddress);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}		
			
			// 开始新的accept
			this.AcceptAsync();
		}
		
		/// <summary>
		/// 发起一次异步 accept；若同步完成则直接调用 <see cref="OnAcceptComplete"/>。
		/// </summary>
		private void AcceptAsync()
		{
			this.innArgs.AcceptSocket = null;
			if (this.acceptor.AcceptAsync(this.innArgs))
			{
				return;
			}
			OnAcceptComplete(this.innArgs.SocketError, this.innArgs.AcceptSocket);
		}

		/// <summary>按 Id 创建（或复用）一个面向指定对端的 <see cref="TChannel"/> 并入表；已存在则忽略。</summary>
		public override void Create(long id, IPEndPoint ipEndPoint)
		{
			if (this.idChannels.TryGetValue(id, out TChannel _))
			{
				return;
			}

			TChannel channel = new(id, ipEndPoint, this);
			this.idChannels.Add(channel.Id, channel);
		}

		/// <summary>按 Id 获取活跃通道，不存在返回 null。</summary>
		public TChannel Get(long id)
		{
			TChannel channel = null;
			this.idChannels.TryGetValue(id, out channel);
			return channel;
		}
		
		/// <summary>
		/// 释放服务：关闭监听 socket、释放 accept 用 SAEA，并级联关闭所有活跃通道。
		/// 先 ToArray 拷贝 key 集合，避免遍历中修改字典导致异常。
		/// </summary>
		public override void Dispose()
		{
			if (this.IsDisposed())
			{
				return;
			}
			
			base.Dispose();
			
			this.acceptor?.Close();
			this.acceptor = null;
			this.innArgs.Dispose();
			
			foreach (long id in this.idChannels.Keys.ToArray())
			{
				TChannel channel = this.idChannels[id];
				channel.Dispose();
			}
		}

		/// <summary>按 Id 移除通道：标记错误码并级联关闭通道，再从表中删除。</summary>
		public override void Remove(long id, int error = 0)
		{
			if (this.idChannels.TryGetValue(id, out TChannel channel))
			{
				channel.Error = error;
				channel.Dispose();	
			}
			this.idChannels.Remove(id);
		}

		/// <summary>按 Id 发送缓冲区：通道不存在则触发 <see cref="AService.ErrorCallback"/>（ERR_SendMessageNotFoundTChannel）。</summary>
		public override void Send(long channelId, MemoryBuffer memoryBuffer)
		{
			try
			{
				TChannel aChannel = this.Get(channelId);
				if (aChannel == null)
				{
					this.ErrorCallback(channelId, ErrorCore.ERR_SendMessageNotFoundTChannel);
					return;
				}
				
				aChannel.Send(memoryBuffer);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
		
		/// <summary>
		/// 每帧事件循环：从 <see cref="Queue"/> 取出所有 pending 的 socket 完成事件与驱动指令并分派。
		/// <list type="bullet">
		///   <item><b>驱动指令</b>（SocketAsyncEventArgs 为空）：按 <see cref="TcpOp"/> 驱动 TChannel 的 StartSend / StartRecv / ConnectAsync。</item>
		///   <item><b>socket 完成事件</b>（SocketAsyncEventArgs 非空）：按 <see cref="SocketAsyncOperation"/> 分派到对应 TChannel 回调（Accept/Connect/Disconnect/Receive/Send）。</item>
		/// </list>
		/// 所有分派前都会用 <see cref="Get"/> 校验通道仍存活，避免对已释放通道操作。
		/// </summary>
		public override void Update()
		{
			while (true)
			{
				if (!this.Queue.TryDequeue(out var result))
				{
					break;
				}
				
				SocketAsyncEventArgs e = result.SocketAsyncEventArgs;
				if (e == null)
				{
					switch (result.Op)
					{
						case TcpOp.StartSend:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.StartSend();
							}
							break;
						}
						case TcpOp.StartRecv:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.StartRecv();
							}
							break;
						}
						case TcpOp.Connect:
						{
							TChannel tChannel = this.Get(result.ChannelId);
							if (tChannel != null)
							{
								tChannel.ConnectAsync();
							}
							break;
						}
					}
					continue;
				}

				switch (e.LastOperation)
				{
					case SocketAsyncOperation.Accept:
					{
						SocketError socketError = e.SocketError;
						Socket acceptSocket = e.AcceptSocket;
						this.OnAcceptComplete(socketError, acceptSocket);
						break;
					}
					case SocketAsyncOperation.Connect:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnConnectComplete(e);
						}

						break;
					}
					case SocketAsyncOperation.Disconnect:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnDisconnectComplete(e);
						}
						break;
					}
					case SocketAsyncOperation.Receive:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnRecvComplete(e);
						}
						break;
					}
					case SocketAsyncOperation.Send:
					{
						TChannel tChannel = this.Get(result.ChannelId);
						if (tChannel != null)
						{
							tChannel.OnSendComplete(e);
						}
						break;
					}
					default:
						throw new ArgumentOutOfRangeException($"{e.LastOperation}");
				}
			}
		}
	}
}