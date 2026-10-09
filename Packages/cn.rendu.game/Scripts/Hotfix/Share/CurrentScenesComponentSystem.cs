using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="CurrentScenesComponent"/> 的便捷扩展：业务侧 <c>root.CurrentScene()</c> 一行拿到当前主 Scene。
	/// </summary>
	public static partial class CurrentScenesComponentSystem
	{
		/// <summary>从 <paramref name="root"/> Scene 上取 <see cref="CurrentScenesComponent.Scene"/>（无则返回 null）。</summary>
		public static Scene CurrentScene(this Scene root)
		{
			return root.GetComponent<CurrentScenesComponent>()?.Scene;
		}
	}
}
