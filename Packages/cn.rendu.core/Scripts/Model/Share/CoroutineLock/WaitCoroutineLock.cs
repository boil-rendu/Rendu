using System;
using RD.Core;
using Object = RD.Core.Object;

namespace RD
{
	/// <summary>
	/// 协程锁超时计时器：被 <see cref="WaitCoroutineLockTimer"/>（<c>[Invoke(TimerCoreInvokeType.CoroutineTimeout)]</c>）框架触发，
	/// 给超过时未拿到锁的等待者抛 <c>coroutine is timeout!</c> 异常。
	/// </summary>
	[Invoke(TimerCoreInvokeType.CoroutineTimeout)]
	public class WaitCoroutineLockTimer: ATimer<WaitCoroutineLock>
	{
		protected override void Run(WaitCoroutineLock waitCoroutineLock)
		{
			if (waitCoroutineLock.IsDisposed())
			{
				return;
			}
			waitCoroutineLock.SetException(new Exception("coroutine is timeout!"));
		}
	}

	/// <summary>
	/// 协程锁的等待令牌：被 <see cref="CoroutineLockComponent"/> 系统创建，与一个 <see cref="Promise{CoroutineLock}"/> 绑定。
	/// 业务代码 <c>await wait.Wait()</c> 即可阻塞；锁到位时由系统调 <see cref="SetResult"/> 唤醒，超时由 <see cref="WaitCoroutineLockTimer"/> 调 <see cref="SetException"/>。
	/// </summary>
	public class WaitCoroutineLock: Object
	{
		/// <summary>工厂方法：池化创建实例并 <see cref="Promise{T}.Create(bool)"/>（从 <see cref="ObjectPool"/> 拉一个）。</summary>
		public static WaitCoroutineLock Create()
		{
			WaitCoroutineLock waitCoroutineLock = new WaitCoroutineLock();
			waitCoroutineLock.tcs = Promise<CoroutineLock>.Create(true);
			return waitCoroutineLock;
		}

		private Promise<CoroutineLock> tcs;

		/// <summary>锁到位时由系统调：成功唤醒等待者，并清掉 tcs（<see cref="IsDisposed"/> = true）。重复调会抛 <see cref="NullReferenceException"/>。</summary>
		public void SetResult(CoroutineLock coroutineLock)
		{
			if (this.tcs == null)
			{
				throw new NullReferenceException("SetResult tcs is null");
			}
			var t = this.tcs;
			this.tcs = null;
			t.SetResult(coroutineLock);
		}

		/// <summary>超时/取消时调：失败唤醒等待者，并清掉 tcs。</summary>
		public void SetException(Exception exception)
		{
			if (this.tcs == null)
			{
				throw new NullReferenceException("SetException tcs is null");
			}
			var t = this.tcs;
			this.tcs = null;
			t.SetException(exception);
		}

		/// <summary>是否已 Set（Result 或 Exception）过。</summary>
		public bool IsDisposed()
		{
			return this.tcs == null;
		}

		/// <summary>业务侧 <c>await this.Wait()</c> 阻塞协程直到锁到位 / 超时 / 取消。</summary>
		public async Promise<CoroutineLock> Wait()
		{
			return await this.tcs;
		}
	}
}
