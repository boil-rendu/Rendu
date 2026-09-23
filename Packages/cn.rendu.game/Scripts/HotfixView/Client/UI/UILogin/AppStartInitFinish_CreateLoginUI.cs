using RD.Core;

namespace RD.Client
{
	[Event(SceneType.StateSync)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async Promise Run(Scene root, AppStartInitFinish args)
		{
			// 背景动画存在期间为「加载 → 登陆」：此处兜底确保存在。
			// LaunchScreen 只在开启热更检查时创建它，编辑器直跑或从 Lobby 退回登陆时都需在此重建。
			GlobalBackgroundAnim.Ensure();
			await root.UIRoot().OpenPanelAsync<LoginPanelComponent>();
		}
	}
}
