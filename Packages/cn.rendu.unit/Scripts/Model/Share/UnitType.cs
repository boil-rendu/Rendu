namespace RD
{
	/// <summary>
	/// Unit 类型常量（partial）：每个常量 = <c>PackageType.Unit * 1000 + 业务子码</c>，用于区分 Player / Monster / NPC。
	/// 业务侧 <c>UnitFactory.Create(unitType, ...)</c> 的 unitType 参数取自这里。
	/// </summary>
	public static partial class UnitType
	{
		/// <summary>玩家 Unit（主控/其他玩家）。</summary>
		public const int Player = PackageType.Unit * 1000 + 1;
		/// <summary>怪物 Unit。</summary>
		public const int Monster = PackageType.Unit * 1000 + 2;
		/// <summary>NPC Unit。</summary>
		public const int NPC = PackageType.Unit * 1000 + 3;
	}
}
