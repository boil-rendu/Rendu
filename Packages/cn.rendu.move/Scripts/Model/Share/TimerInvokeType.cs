namespace RD
{
	/// <summary>
	/// Move 包的 Timer Invoke 常量（partial）：每个常量 = <c>PackageType.Move * 1000 + 业务子码</c>。
	/// </summary>
	public static partial class TimerInvokeType
	{
		/// <summary>移动协程推进定时器（按帧/固定周期更新 Unit 位置）。</summary>
		public const int MoveTimer = PackageType.Move * 1000 + 1;
	}
}
