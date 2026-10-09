using System;
using System.Text;

namespace RD.Core
{
	/// <summary>字符串哈希工具：BKDR 哈希（seed = 1313，按 char 高/低字节分别累乘）→ 64 位 long，配合 <see cref="Mode"/> 做分桶取模。</summary>
	public static class StringHashHelper
	{
		// bkdr hash
		/// <summary>BKDR 哈希：把 <paramref name="str"/> 的每个 char 拆成高/低字节，乘以 seed（1313）累加，返回 64 位 long（<c>ulong</c> 转 <c>long</c>，可能为负）。</summary>
		public static long GetLongHashCode(this string str)
		{
			const uint seed = 1313; // 31 131 1313 13131 131313 etc..

			ulong hash = 0;
			for (int i = 0; i < str.Length; ++i)
			{
				char c = str[i];
				byte high = (byte)(c >> 8);
				byte low = (byte)(c & byte.MaxValue);
				hash = hash * seed + high;
				hash = hash * seed + low;
			}
			return (long)hash;
		}

		/// <summary>字符串按 BKDR 哈希对 <paramref name="mode"/> 取模：用于把字符串均匀分到 <c>mode</c> 个桶里（如玩家名/聊天频道分桶）。<paramref name="mode"/> &lt;= 0 抛异常。</summary>
		public static int Mode(this string strText, int mode)
		{
			if (mode <= 0)
			{
				throw new Exception($"string mode < 0: {strText} {mode}");
			}
			return (int)((ulong)strText.GetLongHashCode() % (uint)mode);
		}
	}
}
