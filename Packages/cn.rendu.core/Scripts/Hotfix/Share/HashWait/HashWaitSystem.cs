using System;
using System.Collections.Generic;
using System.Linq;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="HashWait"/> 的系统扩展（Hotfix/Share）：按 hash 等待某事件完成的集合。
	/// 行为：<see cref="Wait(long)"/> 注册 hash → tcs；<see cref="Notify(long, EHashWaitError, bool)"/> 移除并 SetResult（默认下一帧再 SetResult，<paramref name="waitFrame"/> = false 立即）。
	/// 关键点：通过 <see cref="PromiseHelper.GetContextAsync{T}"/> 关联 <see cref="PromiseCancellationToken"/>，外部 Cancel 时自动 Notify(<see cref="EHashWaitError.Cancel"/>)。
	/// </summary>
	[EntitySystemOf(typeof(HashWait))]
	public static partial class HashWaitSystem
	{
		[EntitySystem]
		private static void Awake(this HashWait self)
		{
			self.m_HashWaitTasks.Clear();
		}

		[EntitySystem]
		private static void Destroy(this HashWait self)
		{
			foreach (var hashCode in self.m_HashWaitTasks.Keys.ToArray())
			{
				self.Notify(hashCode, EHashWaitError.Destroy, false);
			}
		}

		/// <summary>等待 <paramref name="hashCode"/> 触发：已存在则 <see cref="Log.Error"/> + 返回 <see cref="EHashWaitError.Error"/>（不抛）；否则创建 tcs，关联当前上下文的取消令牌（Cancel 时自动 <see cref="EHashWaitError.Cancel"/> 唤醒）。</summary>
		public static async Promise<EHashWaitError> Wait(this HashWait self, long hashCode)
		{
			if (self.m_HashWaitTasks.ContainsKey(hashCode))
			{
				Log.Error($"已经有Wait在等待 {hashCode} 的结果 不能重复等待");
				return EHashWaitError.Error;
			}

			EntityRef<HashWait> selfRef = self;
			var cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
			self = selfRef.Entity;
			var task = Promise<EHashWaitError>.Create(true);
			self.m_HashWaitTasks.Add(hashCode, task);

			try
			{
				cancellationToken?.Add(CancelAction);
				return await task;
			}
			finally
			{
				cancellationToken?.Remove(CancelAction);
			}

			void CancelAction()
			{
				selfRef.Entity?.Notify(hashCode, EHashWaitError.Cancel, false);
			}
		}

		/// <summary>触发 <paramref name="hashCode"/>：从字典移除 tcs；<paramref name="waitFrame"/> = true 时先 <c>await WaitFrameAsync()</c> 再 SetResult（避免在事件源同步路径里立即 SetResult 引发递归），否则立即 SetResult。</summary>
		public static void Notify(this HashWait self, long hashCode, EHashWaitError error = EHashWaitError.Success, bool waitFrame = true)
		{
			if (!self.m_HashWaitTasks.Remove(hashCode, out var task))
			{
				return;
			}

			if (waitFrame)
			{
				Notify(self.Root(), task, error).NoContext();
			}
			else
			{
				task.SetResult(error);
			}
		}

		/// <summary>下一帧再 SetResult 的实现：<c>await WaitFrameAsync()</c> → SetResult。</summary>
		private static async Promise Notify(Scene scene, Promise<EHashWaitError> task, EHashWaitError error)
		{
			await scene?.GetComponent<TimerComponent>().WaitFrameAsync();
			task?.SetResult(error);
		}
	}
}
