using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="CoroutineLockComponent"/> 的系统扩展（Hotfix/Share）：协程锁组件入口。
	/// 核心思路：业务侧 <c>coroutineLockComponent.Wait(type, key)</c> → 下一帧由 <see cref="Update"/> 轮转 <c>nextFrameRun</c> 真正派发，避免在 Actor/Entity 系统同步路径里递归触发锁释放。
	/// </summary>
	[EntitySystemOf(typeof(CoroutineLockComponent))]
	public static partial class CoroutineLockComponentSystem
	{
		[EntitySystem]
		public static void Awake(this CoroutineLockComponent self)
		{
		}

		/// <summary>每帧轮转：消费 <c>nextFrameRun</c> 队列，对每条 (type, key, level) 调 <see cref="Notify(long, long, int)"/> 唤醒等待者。</summary>
		[EntitySystem]
		public static void Update(this CoroutineLockComponent self)
		{
			// 循环过程中会有对象继续加入队列
			while (self.nextFrameRun.Count > 0)
			{
				(long coroutineLockType, long key, int count) = self.nextFrameRun.Dequeue();
				self.Notify(coroutineLockType, key, count);
			}
		}

		/// <summary>把 (type, key, level) 加入下一帧运行队列（<c>level == 100</c> 警告嵌套过深）。</summary>
		public static void RunNextCoroutine(this CoroutineLockComponent self, long coroutineLockType, long key, int level)
		{
			// 一个协程队列一帧处理超过100个,说明比较多了,打个warning,检查一下是否够正常
			if (level == 100)
			{
				Log.Warning($"too much coroutine level: {coroutineLockType} {key}");
			}

			self.nextFrameRun.Enqueue((coroutineLockType, key, level));
		}

		/// <summary>业务侧主入口：取/建 <see cref="CoroutineLockQueueType"/>（按 type 索引）后调其 <see cref="CoroutineLockQueueTypeSystem.Wait"/>。</summary>
		public static async Promise<CoroutineLock> Wait(this CoroutineLockComponent self, long coroutineLockType, long key, int time = 60000)
		{
			CoroutineLockQueueType coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType) ?? self.AddChildWithId<CoroutineLockQueueType>(coroutineLockType);
			return await coroutineLockQueueType.Wait(key, time);
		}

		/// <summary>对 (type, key) 队列发 level+1 唤醒：若队列不存在静默返回。</summary>
		private static void Notify(this CoroutineLockComponent self, long coroutineLockType, long key, int level)
		{
			CoroutineLockQueueType coroutineLockQueueType = self.GetChild<CoroutineLockQueueType>(coroutineLockType);
			if (coroutineLockQueueType == null)
			{
				return;
			}
			coroutineLockQueueType.Notify(key, level);
		}
	}
}
