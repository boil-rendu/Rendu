using RD.Core;

namespace RD
{
	/// <summary>
	/// 泛型定时器基类：<see cref="ATimer{T}"/> &lt; <see cref="AInvokeHandler{TimerCallback}"/>，由 TimerComponent 调 <see cref="Handle(TimerCallback)"/> 时把 <c>a.Args</c> 还原为 <typeparamref name="T"/>。
	/// 业务侧继承 <see cref="ATimer{T}"/> 并标 <c>[Invoke(TimerInvokeType.Xxx)]</c> 即可。
	/// </summary>
	public abstract class ATimer<T>: AInvokeHandler<TimerCallback> where T: class
	{
		/// <summary>TimerComponent 触发：把 <paramref name="a"/> 的 <c>Args</c> 强转为 <typeparamref name="T"/> 后调 <see cref="Run(T)"/>。</summary>
		public override void Handle(TimerCallback a)
		{
			this.Run(a.Args as T);
		}

		/// <summary>定时器实际逻辑入口：业务子类实现。</summary>
		protected abstract void Run(T t);
	}
}
