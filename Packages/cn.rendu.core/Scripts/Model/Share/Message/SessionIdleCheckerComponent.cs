using RD.Core;

namespace RD
{
	/// <summary>
	/// Session 闲置检测器组件：挂在 <see cref="Session"/> 上，定期检查 <see cref="Session.LastRecvTime"/>，超过阈值未收到数据则关闭连接。
	/// 配合 <see cref="TimerInvokeType.SessionIdleChecker"/> 定时器使用。
	/// </summary>
	[ComponentOf(typeof(Session))]
	public class SessionIdleCheckerComponent: Entity, IAwake, IDestroy
	{
		/// <summary>周期检测定时器句柄。</summary>
		public long RepeatedTimer;
	}
}
