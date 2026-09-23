using RD.Core;

namespace RD.Client
{
    [Event(SceneType.Current)]
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<Scene, SceneChangeFinish>
    {
        protected override async Promise Run(Scene scene, SceneChangeFinish args)
        {
            await scene.UIRoot().OpenPanelAsync<MainPanelComponent>();
        }
    }
}
