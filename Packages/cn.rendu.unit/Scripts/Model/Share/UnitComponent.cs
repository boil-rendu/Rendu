using RD.Core;

namespace RD
{
	/// <summary>
	/// Unit 容器组件：挂在 <see cref="Scene"/> 下，作为该场景内所有 <see cref="Unit"/> 实体的父节点（用 ChildOf 索引）。
	/// 业务侧 <c>unitComponent.AddChildWithId(unit, id)</c> / 移除等。
	/// </summary>

	[ComponentOf(typeof(Scene))]
	public class UnitComponent: Entity, IAwake, IDestroy
	{
	}
}
