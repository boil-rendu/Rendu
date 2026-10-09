using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="CoroutineLockQueue"/> 的系统扩展（Hotfix/Share）：同一 (type, key) 下的协程锁队列（先来先获取）。
	/// 关键逻辑：<see cref="Wait"/> 首次直接给锁（<c>isStart == false</c> → 标 true 并立即返回），后续入队 <see cref="WaitCoroutineLock"/> 等前置释放。
	/// </summary>
	[EntitySystemOf(typeof(CoroutineLockQueue))]
	public static partial class CoroutineLockQueueSystem
	{
		[EntitySystem]
		private static void Awake(this CoroutineLockQueue self, long type)
		{
			self.isStart = false;
			self.type = type;
		}

		[EntitySystem]
		private static void Destroy(this CoroutineLockQueue self)
		{
			self.queue.Clear();
			self.type = 0;
			self.isStart = false;
		}

		/// <summary>等待获取锁：首次直接给，后续入队等唤醒。<paramref name="time"/> &gt; 0 时启 <see cref="WaitCoroutineLockTimer"/> 超时器（默认 60s）。</summary>
		public static async Promise<CoroutineLock> Wait(this CoroutineLockQueue self, int time)
		{
			CoroutineLock coroutineLock = null;
			if (!self.isStart)
			{
				self.isStart = true;
				coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.type, self.Id, 1, true);
				return coroutineLock;
			}

			WaitCoroutineLock waitCoroutineLock = WaitCoroutineLock.Create();
			self.queue.Enqueue(waitCoroutineLock);
			if (time > 0)
			{
				long tillTime = TimeInfo.Instance.ClientFrameTime() + time;
				self.Root().GetComponent<TimerComponent>().NewOnceTimer(tillTime, TimerCoreInvokeType.CoroutineTimeout, waitCoroutineLock);
			}
			coroutineLock = await waitCoroutineLock.Wait();
			return coroutineLock;
		}

		// 返回值，是否找到了一个有效的协程锁
		/// <summary>唤醒队首：跳过已 <see cref="WaitCoroutineLock.IsDisposed"/> 的（已超时）项，唤醒首个有效等待者并给锁。返回是否真找到有效等待者。</summary>
		public static bool Notify(this CoroutineLockQueue self, int level)
		{
			// 有可能WaitCoroutineLock已经超时抛出异常，所以要找到一个未处理的WaitCoroutineLock
			while (self.queue.Count > 0)
			{
				WaitCoroutineLock waitCoroutineLock = self.queue.Dequeue();

				if (waitCoroutineLock.IsDisposed())
				{
					continue;
				}

				CoroutineLock coroutineLock = self.AddChild<CoroutineLock, long, long, int>(self.type, self.Id, level, true);

				waitCoroutineLock.SetResult(coroutineLock);
				return true;
			}
			return false;
		}
	}
}
