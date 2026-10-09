using Unity.Mathematics;

namespace RD
{
	/// <summary>空标记事件：Unit 位置改变（<see cref="Unit.Position"/> setter 触发）。</summary>
	public struct ChangePosition
	{
		/// <summary>位置变化的 Unit。</summary>
		public Unit Unit;
		/// <summary>旧坐标（用于差量/插值）。</summary>
		public float3 OldPos;
	}

	/// <summary>空标记事件：Unit 旋转改变（<see cref="Unit.Rotation"/> setter 触发）。</summary>
	public struct ChangeRotation
	{
		/// <summary>旋转变化的 Unit。</summary>
		public Unit Unit;
	}

	/// <summary>空标记事件：Unit 刚创建完（AWAKE 后所有组件就位）。</summary>
	public struct AfterUnitCreate
	{
		/// <summary>被创建的 Unit。</summary>
		public Unit Unit;
	}
}
