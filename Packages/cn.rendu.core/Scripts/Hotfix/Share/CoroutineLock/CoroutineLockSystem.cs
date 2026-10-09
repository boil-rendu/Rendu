using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="CoroutineLock"/> 的系统扩展（Hotfix/Share）：单条协程锁的生命周期。
	/// 关键点：<see cref="Destroy"/> 时不直接通知下一帧运行，而是 <see cref="CoroutineLockComponentSystem.RunNextCoroutine"/> 把 (type, key, level+1) 推到组件的 nextFrameRun 队列，下一帧再唤醒后续等待者（避免在 Entity 销毁同步路径里递归触发）。
	/// </summary>
	[EntitySystemOf(typeof(CoroutineLock))]
	public static partial class CoroutineLockSystem
	{
		[EntitySystem]
		private static void Awake(this CoroutineLock self, long type, long k, int count)
		{
			self.type = type;
			self.key = k;
			self.level = count;
		}

		[EntitySystem]
		private static void Destroy(this CoroutineLock self)
		{
			self.Scene<CoroutineLockComponent>().RunNextCoroutine(self.type, self.key, self.level + 1);
			self.type = 0;
			self.key = 0;
			self.level = 0;
		}
	}
}
