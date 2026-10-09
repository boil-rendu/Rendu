namespace RD
{
	/// <summary>
	/// AI 包的 Timer Invoke 常量（partial）：每个常量 = <c>PackageType.AI * 1000 + 业务子码</c>，用于 <c>[Invoke(TimerInvokeType.AITimer)]</c> 标在定时器类上。
	/// </summary>
	public static partial class TimerInvokeType
	{
		/// <summary>AI 行为节点切到下一个的周期触发器。</summary>
		public const int AITimer = PackageType.AI * 1000 + 1;
	}
}
