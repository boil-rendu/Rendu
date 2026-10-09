using System.Collections.Generic;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 同一 type 下的所有 key 共用一个 <see cref="CoroutineLockQueue"/>：FIFO 排列 <see cref="WaitCoroutineLock"/>（先来先获取锁）。
	/// 顶层 <see cref="CoroutineLockQueueType"/> 按 type 索引，再到本类按 key 索引。
	/// </summary>
	[ChildOf(typeof(CoroutineLockQueueType))]
	public class CoroutineLockQueue: Entity, IAwake<long>, IDestroy
	{
		/// <summary>锁类型（与 <see cref="CoroutineLock.type"/> 同源）。</summary>
		public long type;

		/// <summary>是否已启动处理（避免重复入队）。</summary>
		public bool isStart;

		/// <summary>按 key 串行排队的等待者集合：先来先服务。</summary>
		public Queue<WaitCoroutineLock> queue = new();

		/// <summary>当前队列长度（即 <see cref="WaitCoroutineLock"/> 个数）。</summary>
		public int Count
		{
			get
			{
				return this.queue.Count;
			}
		}
	}
}
