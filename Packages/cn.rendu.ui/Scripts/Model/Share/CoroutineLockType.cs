using RD.Core;
namespace RD
{
	/// <summary>
	/// UI 包的协程锁类型常量（partial）：每个常量 = <c>PackageType.UI * 1000 + 业务子码</c>。
	/// 业务侧应基于这些常量扩展自己的协程锁类型，避免与 UI 框架 key 冲突。
	/// </summary>
	public static partial class CoroutineLockType
	{
		//这个是UI框架用的，用来区分协程锁的类型
		//你应该定义自己的协程锁类型 以免Key相同时发生冲突
		/// <summary>UI 框架层互斥。</summary>
		public const int UIFramework = PackageType.UI * 1000 + 1;
		/// <summary>UI 调用协程锁（一个 UI 回调同时多次触发的互斥）。</summary>
		public const int UIInvokeCoroutineLock = PackageType.UI * 1000 + 2;
		/// <summary>UI 资源加载互斥。</summary>
		public const int UILoad = PackageType.UI * 1000 + 3;
		/// <summary>UI 面板互斥（同一面板同时只允许一个打开动作）。</summary>
		public const int UIPanel = PackageType.UI * 1000 + 4;
		/// <summary>UI 资源包（atlas/prefab 等）互斥。</summary>
		public const int UIPackage = PackageType.UI * 1000 + 5;
	}
}
