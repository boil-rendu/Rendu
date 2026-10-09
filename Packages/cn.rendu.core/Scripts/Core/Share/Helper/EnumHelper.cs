using System;

namespace RD.Core
{
	/// <summary>枚举工具：<see cref="EnumIndex{T}"/> 查 int 值在枚举中的声明序位，<see cref="FromString{T}"/> 字符串转枚举（未定义值返回 <c>default</c>）。</summary>
	public static class EnumHelper
	{
		/// <summary>按 int 值查 <typeparamref name="T"/> 枚举中的声明顺序（从 0 开始）：找不到返回 -1。常用于「按 enum 值查表下标」。</summary>
		public static int EnumIndex<T>(int value)
		{
			int i = 0;
			foreach (object v in Enum.GetValues(typeof (T)))
			{
				if ((int) v == value)
				{
					return i;
				}
				++i;
			}
			return -1;
		}

		/// <summary>字符串转 <typeparamref name="T"/> 枚举：未定义值返回 <c>default</c>（不抛异常），与 <c>Enum.Parse</c> 的「未定义值抛异常」行为不同。</summary>
		public static T FromString<T>(string str)
		{
			if (!Enum.IsDefined(typeof(T), str))
			{
				return default(T);
			}
			return (T)Enum.Parse(typeof(T), str);
		}
	}
}
