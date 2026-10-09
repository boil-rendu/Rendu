using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="NetComponentOnRead"/> 事件：<see cref="NetComponent"/> Update 时每收到一条包 publish 一次（让业务侧统一拦截/打点）。
	/// </summary>
	public struct NetComponentOnRead
	{
		/// <summary>收到包所属的 Session。</summary>
		public Session Session;
		/// <summary>已反序列化好的消息体。</summary>
		public object Message;
	}

	/// <summary>
	/// 网络组件（每 Scene 挂一个）：持有本 Scene 的 <see cref="AService"/>（TCP/KCP 适配），
	/// Update 轮转底层服务的收发/超时；<see cref="IAwake{T}"/> 入参为 <see cref="IKcpTransport"/>（KCP 场景）或 <see cref="TcpTransport"/>。
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class NetComponent: Entity, IAwake<IKcpTransport>, IDestroy, IUpdate
	{
		/// <summary>底层网络服务（由 Awake 时构造的 <see cref="AService"/>）。</summary>
		public AService AService { get; set; }
	}
}
