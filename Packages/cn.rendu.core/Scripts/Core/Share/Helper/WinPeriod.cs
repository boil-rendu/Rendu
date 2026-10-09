using System.Runtime.InteropServices;

namespace RD.Core
{
	/// <summary>Windows 平台 1ms 定时器精度设置：调 <c>winmm!timeBeginPeriod(1)</c> 让 <see cref="System.Threading.Thread.Sleep(int)"/> 实际能睡到 1ms（非 Windows 静默跳过）。</summary>
	public static class WinPeriod
	{
		// 一般默认的精度不止1毫秒（不同操作系统有所不同），需要调用timeBeginPeriod与timeEndPeriod来设置精度
		[DllImport("winmm")]
		private static extern void timeBeginPeriod(int t);
		//[DllImport("winmm")]
		//static extern void timeEndPeriod(int t);

		/// <summary>在 Windows 上设置全局 1ms 定时器精度：用于 <c>ThreadScheduler</c> 等「每帧 sleep 1ms」场景，避免 15ms 精度浪费帧时间。非 Windows 静默返回。</summary>
		public static void Init()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				timeBeginPeriod(1);
			}
		}
	}
}
