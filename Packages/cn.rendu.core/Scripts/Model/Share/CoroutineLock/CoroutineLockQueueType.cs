using RD.Core;

namespace RD
{
	/// <summary>
	/// 协程锁队列的「type 维度」容器：挂在 <see cref="CoroutineLockComponent"/> 下，按 <see cref="CoroutineLock.type"/> 索引所有 <see cref="CoroutineLockQueue"/>。
	/// 每个 Scene 内全局唯一。
	/// </summary>
	[ChildOf(typeof(CoroutineLockComponent))]
	public class CoroutineLockQueueType: Entity, IAwake
	{
	}
}
