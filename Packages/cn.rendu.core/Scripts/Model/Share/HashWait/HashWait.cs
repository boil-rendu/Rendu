using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
	/// <summary>
	/// Hash 维度的等待集合容器：以 <c>long</c> hash 为 key 维护一组 <see cref="Promise{T}"/>（携带 <see cref="EHashWaitError"/>），业务侧 <c>await tcs</c> 等该 hash 触发事件。
	/// 配合 <see cref="HashWaitSystem"/>（Hotfix/Share 内的 system）做 Add / Set / Remove / Broadcast。
	/// </summary>
	[ComponentOf]
	public class HashWait : Entity, IAwake, IDestroy
	{
		/// <summary>hash → Promise 的等待字典。</summary>
		public Dictionary<long, Promise<EHashWaitError>> m_HashWaitTasks = new();
	}
}
