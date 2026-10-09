using MemoryPack;
using RD.Core;

namespace RD
{
	/// <summary>MemoryPack 序列化器/反序列化器测试用 Entity：<c>[MemoryPackable]</c> + 标 <c>[ComponentOf(typeof(Scene))]</c>，用于验证 <c>cn.rendu.core</c> 的 MemoryPack 钩子是否正确生成。</summary>
	[MemoryPackable]
	[ComponentOf(typeof(Scene))]
	public partial class AA : Entity, IAwake
	{
	}

	/// <summary><see cref="AA"/> 的组件：标 <c>[MemoryPackInclude]</c> 的 <see cref="B"/> 会被 MemoryPack 持久化；<c>ISerializeToEntity</c> 标记使父 <see cref="AA"/> 持久化时连同本组件一起写。</summary>
	[MemoryPackable]
	[ComponentOf(typeof(AA))]
	public partial class BB : Entity, IAwake, ISerializeToEntity
	{
		[MemoryPackInclude]
		public int B { get; set; }
	}

	/// <summary><see cref="AA"/> 的另一个组件：与 <see cref="BB"/> 区别是注释掉了 <c>ISerializeToEntity</c>，用于测试父实体不写本组件的场景。</summary>
	[MemoryPackable]
	[ComponentOf(typeof(AA))]
	public partial class CC : Entity, IAwake //, ISerializeToEntity
	{
		[MemoryPackInclude]
		public int C { get; set; }
	}
}
