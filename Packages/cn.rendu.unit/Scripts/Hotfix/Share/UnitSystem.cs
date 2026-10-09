using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="Unit"/> 的系统扩展（Hotfix/Share）：<see cref="Config(Unit)"/> 拿配置 + <see cref="Type(Unit)"/> 拿 UnitType。
	/// </summary>
	[EntitySystemOf(typeof(Unit))]
	public static partial class UnitSystem
	{
		[EntitySystem]
		private static void Awake(this Unit self, int configId)
		{
			self.ConfigId = configId;
		}

		/// <summary>取 Unit 关联的 <see cref="UnitConfig"/>（从 <see cref="UnitConfigCategory"/> 按 <see cref="Unit.ConfigId"/> 查表）。</summary>
		public static UnitConfig Config(this Unit self)
		{
			return UnitConfigCategory.Instance.Get(self.ConfigId);
		}

		/// <summary>取 Unit 的类型（<see cref="UnitConfig.Type"/>，即 <see cref="UnitType"/> 常量之一 Player/Monster/NPC）。</summary>
		public static int Type(this Unit self)
		{
			return self.Config().Type;
		}
	}
}
