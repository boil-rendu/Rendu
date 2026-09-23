using RD.Core;

namespace RD.Client
{
    [Event(SceneType.StateSync)]
    public class AfterCreateClientScene_AddComponent: AEvent<Scene, AfterCreateClientScene>
    {
        protected override async Promise Run(Scene scene, AfterCreateClientScene args)
        {
            scene.AddComponent<UIMgrComponent>();
            scene.AddComponent<ResourcesLoaderComponent>();
            await Promise.CompletedTask;
        }
    }
}