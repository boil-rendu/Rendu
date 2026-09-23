using RD.Core;

namespace RD.Client
{
    [Event(SceneType.StateSync)]
    public class LoginFinish_RemoveLoginUI: AEvent<Scene, LoginFinish>
    {
        protected override async Promise Run(Scene scene, LoginFinish args)
        {
            await scene.UIMgr().ClosePanelAsync<LoginPanelComponent>();
        }
    }
}
