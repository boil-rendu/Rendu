using System;
using System.Collections.Generic;
using System.Linq;
using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="HashWait"/> 的对象版本扩展：把 <see cref="object.GetHashCode"/> 作为 key 等待，避免业务侧手算 long hash。
	/// </summary>
	public static partial class HashWaitSystem
	{
		/// <summary>等 <paramref name="target"/> 触发：以 <c>target.GetHashCode()</c> 作 key，内部走 <see cref="Wait(HashWait, long)"/>。</summary>
		public static async Promise<EHashWaitError> WaitObject(this HashWait self, object target)
		{
			var hash = target.GetHashCode();
			if (self.m_HashWaitTasks.ContainsKey(hash))
			{
				Log.Error($"已经有Wait在等待 {target.GetType().Name} 的结果 不能重复等待");
				return EHashWaitError.Error;
			}

			return await self.Wait(hash);
		}

		/// <summary>触发 <paramref name="target"/>：以 <c>target.GetHashCode()</c> 作 key，内部走 <see cref="Notify(HashWait, long, EHashWaitError, bool)"/>。</summary>
		public static void NotifyObject(this HashWait self, object target, EHashWaitError error = EHashWaitError.Success, bool waitFrame = true)
		{
			self.Notify(target.GetHashCode(), error, waitFrame);
		}
	}
}
