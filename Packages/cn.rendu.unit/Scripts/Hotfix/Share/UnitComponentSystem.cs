namespace RD
{
	/// <summary>
	/// <see cref="UnitComponent"/> 的便捷扩展（partial）：<c>Add</c>/<c>Get</c>/<c>Remove</c> 三个语义清晰的子操作（目前 <c>Add</c> 是空方法占位，保留扩展点）。
	/// </summary>
	public static partial class UnitComponentSystem
	{
		/// <summary>占位扩展：当前未实现（实际 unit 创建走 <c>unitComponent.AddChildWithId</c> 流程）。</summary>
		public static void Add(this UnitComponent self, Unit unit)
		{
		}

		/// <summary>按 <paramref name="id"/> 取 <see cref="Unit"/>，无则返回 null。</summary>
		public static Unit Get(this UnitComponent self, long id)
		{
			Unit unit = self.GetChild<Unit>(id);
			return unit;
		}

		/// <summary>按 <paramref name="id"/> Dispose <see cref="Unit"/>，无则静默 return。</summary>
		public static void Remove(this UnitComponent self, long id)
		{
			Unit unit = self.GetChild<Unit>(id);
			unit?.Dispose();
		}
	}
}
