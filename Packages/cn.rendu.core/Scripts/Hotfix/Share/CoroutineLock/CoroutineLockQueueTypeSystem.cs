using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="CoroutineLockQueueType"/> 的系统扩展（Hotfix/Share）：按 (type) 索引的协程锁队列容器。
	/// 提供 <see cref="Get"/>/<see cref="New"/>/<see cref="Remove"/> 对 <see cref="CoroutineLockQueue"/>（按 key 索引）的 CRUD。
	/// </summary>
	[EntitySystemOf(typeof(CoroutineLockQueueType))]
	public static partial class CoroutineLockQueueTypeSystem
	{
		[EntitySystem]
		private static void Awake(this CoroutineLockQueueType self)
		{
		}

		/// <summary>取 <paramref name="key"/> 对应的 <see cref="CoroutineLockQueue"/>，无则返回 null（不创建）。</summary>
		public static CoroutineLockQueue Get(this CoroutineLockQueueType self, long key)
		{
			return self.GetChild<CoroutineLockQueue>(key);
		}

		/// <summary>新建 <paramref name="key"/> 对应的 <see cref="CoroutineLockQueue"/>，已存在会抛（用 <see cref="Get"/> ?? <see cref="New"/> 模式）。</summary>
		public static CoroutineLockQueue New(this CoroutineLockQueueType self, long key)
		{
			CoroutineLockQueue queue = self.AddChildWithId<CoroutineLockQueue, long>(key, self.Id, true);
			return queue;
		}

		/// <summary>移除 <paramref name="key"/> 对应的 <see cref="CoroutineLockQueue"/>（销毁时清空）。</summary>
		public static void Remove(this CoroutineLockQueueType self, long key)
		{
			self.RemoveChild(key);
		}

		/// <summary>取/建 <paramref name="key"/> 队列后调其 <see cref="CoroutineLockQueueSystem.Wait"/>，最终返回 <see cref="CoroutineLock"/>。</summary>
		public static async Promise<CoroutineLock> Wait(this CoroutineLockQueueType self, long key, int time)
		{
			CoroutineLockQueue queue = self.Get(key) ?? self.New(key);
			return await queue.Wait(time);
		}

		/// <summary>对 <paramref name="key"/> 队列发 level 唤醒：队列空则移除该队列。</summary>
		public static void Notify(this CoroutineLockQueueType self, long key, int level)
		{
			CoroutineLockQueue queue = self.Get(key);
			if (queue == null)
			{
				return;
			}

			if (!queue.Notify(level))
			{
				self.Remove(key);
			}
		}
	}
}
