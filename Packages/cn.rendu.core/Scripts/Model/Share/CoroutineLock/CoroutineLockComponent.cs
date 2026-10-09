using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 协程锁组件：每 Scene 挂一个（<see cref="IScene"/> + <see cref="IUpdate"/>），以 (type, key) 为索引的协程锁分配/释放中心。
	/// 队列数据存放在 <see cref="CoroutineLockQueueType"/>（按 type 索引）→ <see cref="CoroutineLockQueue"/>（按 key 索引）→ <see cref="WaitCoroutineLock"/>（等待者）。
	/// 调度方式：<see cref="IUpdate"/> 轮转 <c>nextFrameRun</c>，避免在 <c>EntitySystem</c> 同步代码里直接 Release 引发递归。
	/// </summary>
	[ComponentOf(typeof(Scene))]
	public class CoroutineLockComponent: Entity, IAwake, IScene, IUpdate
	{
		public Fiber Fiber { get; set; }
		public int SceneType { get; set; }

		/// <summary>下一帧统一执行的 (type, key, level) 释放请求队列（避免在当前帧同步路径里递归触发锁的获取）。</summary>
		public readonly Queue<(long, long, int)> nextFrameRun = new();
	}
}
