using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 客户端初始化入口。
    /// 由 MainFiberInit 在根 Scene（其 SceneType 已切换为 Options.SceneName 对应的实际场景类型）上发布 InitClient 触发。
    /// 负责挂载客户端根组件、初始化 UI 框架，并在就绪后发布 AppStartInitFinish 进入登录界面。
    /// </summary>
    [Event(SceneType.StateSync)]
    public class ClientInitEvent: AEvent<Scene, InitClient>
    {
        protected override async Promise Run(Scene root, InitClient args)
        {
            root.AddComponent<GlobalComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();

            var result = await root.AddComponent<UIMgrComponent>().Initialize();
            if (!result)
            {
                Log.Error("初始化UI失败");
                return;
            }

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
        }
    }
}
