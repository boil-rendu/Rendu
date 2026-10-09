using RD.Core;

namespace RD
{
	// 刚accept的session只持续5秒，必须通过验证，否则断开
	/// <summary>
	/// Session 接受超时组件：刚 accept 的新连接在 5 秒内必须通过业务层验证（登录/握手等），否则由该组件触发关闭。
	/// 配合 <see cref="TimerInvokeType.SessionAcceptTimeout"/> 定时器使用。
	/// </summary>
	[ComponentOf(typeof(Session))]
	public class SessionAcceptTimeoutComponent: Entity, IAwake, IDestroy
	{
		/// <summary>超时定时器句柄（5s）。</summary>
		public long Timer;
	}
}
