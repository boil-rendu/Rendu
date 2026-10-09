namespace RD
{
	// 这个可弄个配置表生成
	/// <summary>
	/// 数值类型常量（partial）：每个数值由 <c>100 * n</c> 区分业务（如 Speed=1000、Hp=1001、MaxHp=1002、AOI=1003），
	/// 每个业务数值配 5 个基础值（*10+1/2/3/4/5 = base/add/pct/finalAdd/finalPct），用 <see cref="Max"/> = 10000 区分「基础值」与「最终值」（&lt; 10000 为基础值，&gt;= 10000 为衍生值，触发重算）。
	/// </summary>
	public static partial class NumericType
	{
		/// <summary>基础值 / 衍生值分界：&lt; <see cref="Max"/> 视为基础值，&gt;= 视为衍生最终值（写衍生值会触发 <see cref="NumericComponentSystem.Update"/> 重算）。</summary>
	    public const int Max = 10000;

		/// <summary>移动速度（衍生最终值 Id）。</summary>
	    public const int Speed = 1000;
		/// <summary>速度基础值（直接数值）。</summary>
	    public const int SpeedBase = Speed * 10 + 1;
		/// <summary>速度加成（绝对值）。</summary>
	    public const int SpeedAdd = Speed * 10 + 2;
		/// <summary>速度百分比加成（如 +10%）。</summary>
	    public const int SpeedPct = Speed * 10 + 3;
		/// <summary>最终绝对值加成（在百分比之后）。</summary>
	    public const int SpeedFinalAdd = Speed * 10 + 4;
		/// <summary>最终百分比加成（在最终绝对值之后）。</summary>
	    public const int SpeedFinalPct = Speed * 10 + 5;

		/// <summary>当前 HP（衍生最终值 Id）。</summary>
	    public const int Hp = 1001;
		/// <summary>HP 基础值。</summary>
	    public const int HpBase = Hp * 10 + 1;

		/// <summary>最大 HP（衍生最终值 Id）。</summary>
	    public const int MaxHp = 1002;
		/// <summary>最大 HP 基础值。</summary>
	    public const int MaxHpBase = MaxHp * 10 + 1;
		/// <summary>最大 HP 绝对值加成。</summary>
	    public const int MaxHpAdd = MaxHp * 10 + 2;
		/// <summary>最大 HP 百分比加成。</summary>
	    public const int MaxHpPct = MaxHp * 10 + 3;
		/// <summary>最大 HP 最终绝对值加成。</summary>
	    public const int MaxHpFinalAdd = MaxHp * 10 + 4;
		/// <summary>最大 HP 最终百分比加成。</summary>
	    public const int MaxHpFinalPct = MaxHp * 10 + 5;

		/// <summary>视野半径（AOI，Area Of Interest，衍生最终值 Id）。</summary>
	    public const int AOI = 1003;
		/// <summary>AOI 基础值。</summary>
	    public const int AOIBase = AOI * 10 + 1;
		/// <summary>AOI 绝对值加成。</summary>
	    public const int AOIAdd = AOI * 10 + 2;
		/// <summary>AOI 百分比加成。</summary>
	    public const int AOIPct = AOI * 10 + 3;
		/// <summary>AOI 最终绝对值加成。</summary>
	    public const int AOIFinalAdd = AOI * 10 + 4;
		/// <summary>AOI 最终百分比加成。</summary>
	    public const int AOIFinalPct = AOI * 10 + 5;
	}
}
