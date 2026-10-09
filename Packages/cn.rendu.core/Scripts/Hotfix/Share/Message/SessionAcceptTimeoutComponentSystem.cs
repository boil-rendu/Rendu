using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="SessionAcceptTimeoutComponent"/> 的系统扩展（Hotfix/Share）：
	/// Awake 启 5s 一次性定时器，到期若组件未销毁（即业务未完成验证）就 Dispose 父 Session（关闭连接）。
	/// 防外挂：accept 后不发验证包也连不上。
	/// </summary>
	[EntitySystemOf(typeof(SessionAcceptTimeoutComponent))]
	public static partial class SessionAcceptTimeoutComponentSystem
	{
		/// <summary>5s 超时定时器（<c>[Invoke(TimerCoreInvokeType.SessionAcceptTimeout)]</c>）：Dispose 父 Session（关闭连接）。</summary>
		[Invoke(TimerCoreInvokeType.SessionAcceptTimeout)]
		public class SessionAcceptTimeout: ATimer<SessionAcceptTimeoutComponent>
		{
			protected override void Run(SessionAcceptTimeoutComponent self)
			{
				try
				{
					self.Parent.Dispose();
				}
				catch (Exception e)
				{
					Log.Error($"move timer error: {self.Id}\n{e}");
				}
			}
		}

		[EntitySystem]
		private static void Awake(this SessionAcceptTimeoutComponent self)
		{
			self.Timer = self.Root().GetComponent<TimerComponent>().NewOnceTimer(TimeInfo.Instance.ServerNow() + 5000, TimerCoreInvokeType.SessionAcceptTimeout, self);
		}

		[EntitySystem]
		private static void Destroy(this SessionAcceptTimeoutComponent self)
		{
			self.Root().GetComponent<TimerComponent>()?.Remove(ref self.Timer);
		}


	}



}
