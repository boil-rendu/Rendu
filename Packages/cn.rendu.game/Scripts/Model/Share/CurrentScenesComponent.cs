using RD.Core;

namespace RD
{
	/// <summary>
	/// 当前 Scene 容器组件：用于大世界等多场景并存场景，记录当前主 Scene（区别于 ClientScene 下的多个子场景）。
	/// 业务侧 <see cref="Scene"/> 读写该属性。
	/// </summary>
	// 可以用来管理多个客户端场景，比如大世界会加载多块场景
	[ComponentOf(typeof(Scene))]
	public class CurrentScenesComponent: Entity, IAwake
	{
		private EntityRef<Scene> scene;

		/// <summary>当前主 Scene：业务侧读写它以获取/切换"激活"场景。</summary>
		public Scene Scene
		{
			get
			{
				return this.scene;
			}
			set
			{
				this.scene = value;
			}
		}
	}
}
