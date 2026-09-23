using RD.Core;
namespace RD.Client
{
    [Event(SceneType.All)]
    public class On_UIEventInitializeBefore_CountDownHandler : AEvent<Scene, UIEventInitializeBefore>
    {
        protected override async Promise Run(Scene scene, UIEventInitializeBefore arg)
        {
            var uiMgr = scene.GetComponent<UIMgrComponent>();
            uiMgr.AddComponent<CountDownMgr>();
            await Promise.CompletedTask;
        }
    }
}