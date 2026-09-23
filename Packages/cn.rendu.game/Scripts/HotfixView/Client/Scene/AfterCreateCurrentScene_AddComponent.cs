using RD.Core;

namespace RD.Client
{
    [Event(SceneType.Current)]
    public class AfterCreateCurrentScene_AddComponent: AEvent<Scene, AfterCreateCurrentScene>
    {
        protected override async Promise Run(Scene scene, AfterCreateCurrentScene args)
        {
            // UIMgrComponent 为全局唯一组件, 已由 ClientInitEvent 挂载在根 Scene 上,
            // 此处重复挂载会导致其 Awake 再次向 Root 添加 UIRootComponent 而抛异常
            scene.AddComponent<ResourcesLoaderComponent>();
            await Promise.CompletedTask;
        }
    }
}