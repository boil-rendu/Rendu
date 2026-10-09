namespace RD
{
	/// <summary>
	/// 进程内包（dll）Id 常量（partial）：用 <c>[UniqueId]</c> SourceGenerator 自动注入排序保证唯一，业务侧 <c>PackageType.Xxx * 1000 + 子码</c> 派生 Scene/Error/Opcode 等常量。
	/// 子 dll 用 partial 形式追加自己的 PackageType 常量。
	/// </summary>
	[UniqueId]
	public static partial class PackageType
	{
		/// <summary>cn.rendu.core 包 Id（基线，子 dll 在此基础上递增）。</summary>
		public const int Core = 1;
	}
}
