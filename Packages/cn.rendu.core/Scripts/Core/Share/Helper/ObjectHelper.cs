namespace RD.Core
{
	/// <summary>通用对象工具。</summary>
	public static class ObjectHelper
	{
		/// <summary>交换两个 <c>ref</c> 变量（基于 C# 7 元组解构，零分配）。</summary>
		public static void Swap<T>(ref T t1, ref T t2)
		{
			(t1, t2) = (t2, t1);
		}
	}
}
