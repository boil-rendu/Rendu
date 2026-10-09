namespace RD
{
	/// <summary>空标记事件：Unit 开始移动（<see cref="MoveComponent"/> 启动协程时 publish）。</summary>
	public struct MoveStart
	{
		/// <summary>开始移动的 Unit。</summary>
		public Unit Unit;
	}

	/// <summary>空标记事件：Unit 停止移动（<see cref="MoveComponent"/> 协程完成/被中断时 publish）。</summary>
	public struct MoveStop
	{
		/// <summary>停止移动的 Unit。</summary>
		public Unit Unit;
	}
}
