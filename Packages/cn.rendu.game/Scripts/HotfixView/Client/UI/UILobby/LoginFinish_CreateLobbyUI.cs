using RD.Core;

namespace RD.Client
{
	[Event(SceneType.StateSync)]
	public class LoginFinish_CreateLobbyUI: AEvent<Scene, LoginFinish>
	{
		protected override async Promise Run(Scene scene, LoginFinish args)
		{
			await scene.UIRoot().OpenPanelAsync<LobbyPanelComponent>();

			// 进入 Lobby：背景动画的存在期间（加载 → 登陆）到此结束，释放视频与 RenderTexture
			GlobalBackgroundAnim.Hide();
		}
	}
}
