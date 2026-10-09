using RD.Core;
namespace RD
{
	/// <summary>
	/// Game 包业务常量集合（partial）：服务端/客户端的运行期常量。
	/// </summary>
	public static partial class ConstValue
	{
		/// <summary>Router HTTP 服务地址（开发环境默认本地）。</summary>
		public const string RouterHttpHost = "127.0.0.1";
		/// <summary>Router HTTP 服务端口。</summary>
		public const int RouterHttpPort = 30300;
		/// <summary>Session 闲置超时（30s，超时未收到消息断开）。</summary>
		public const int SessionTimeoutTime = 30 * 1000;

		/// <summary>Excel 配置表 Unity package 路径（ConfigLoader 加载源）。</summary>
		public const string ExcelPackagePath = "Packages/cn.rendu.excel";
		/// <summary>Recast 寻路 Unity package 路径。</summary>
		public const string RecastPackagePath = "Packages/cn.rendu.recast";

	}
}
