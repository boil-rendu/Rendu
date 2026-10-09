using MemoryPack;

namespace RD.Core
{
	/// <summary>
	/// MemoryPack 自定义 Formatter 注册入口：把 <see cref="MemoryPackChildrenCollectionFormatter"/> 和 <see cref="MemoryPackComponentsCollectionFormatter"/>
	/// 注册到全局 <see cref="MemoryPackFormatterProvider"/>，被 <see cref="World.CodeTypes"/> 初始化后调一次。
	/// </summary>
	public static class MemoryPackRegister
	{
		/// <summary>注册两个集合类型的自定义 Formatter：分别支持 <see cref="ChildrenCollection"/> 和 <see cref="ComponentsCollection"/> 的 MemoryPack 序列化。</summary>
		public static void Init()
		{
			MemoryPackFormatterProvider.Register(new MemoryPackChildrenCollectionFormatter());
			MemoryPackFormatterProvider.Register(new MemoryPackComponentsCollectionFormatter());
		}
	}
}
