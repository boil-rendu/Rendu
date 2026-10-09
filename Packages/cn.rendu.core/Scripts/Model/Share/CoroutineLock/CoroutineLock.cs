using RD.Core;

namespace RD
{
	/// <summary>
	/// 单条协程锁记录：标识一把「类型/键/层级」三段组合的锁实例。
	/// 由 <see cref="CoroutineLockComponent"/> 系统颁发（<c>Wait&lt;CoroutineLockType, CoroutineLockLevel, key&gt;</c>），调用方 <c>await</c> 出参获得锁所有权。
	/// </summary>
	[ChildOf(typeof(CoroutineLockQueue))]
	public class CoroutineLock: Entity, IAwake<long, long, int>, IDestroy
	{
		/// <summary>锁类型（业务域，由 <see cref="CoroutineLockType"/> 常量填入）。</summary>
		public long type;

		/// <summary>锁 key（同一 type 下进一步细分，如 UnitId）。</summary>
		public long key;

		/// <summary>锁层级（用于嵌套协程锁，超时按层级计算）。</summary>
		public int level;
	}
}
