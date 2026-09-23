using RD.Core;

namespace RD.Server
{
    [Event(SceneType.StateSync)]
    public class ServerInitEvent: AEvent<Scene, InitServer>
    {
        protected override async Promise Run(Scene root, InitServer args)
        {
            if (Options.Instance.Console == 1)
            {
                root.AddComponent<ConsoleComponent>();
            }
            
            World.Instance.AddSingleton<NavmeshComponent>();
            
            int process = root.Fiber.Process;
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(process);
            if (startProcessConfig.Port != 0)
            {
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, SceneType.NetInner, 0, SceneType.NetInner, "NetInner");
            }

            // 根据配置创建纤程
            var scenes = StartSceneConfigCategory.Instance.GetByProcess(process);
            
            foreach (StartSceneConfig startConfig in scenes)
            {
                
                int sceneType = SceneTypeSingleton.Instance.GetSceneType(startConfig.SceneType);
                await FiberManager.Instance.Create(SchedulerType.ThreadPool, startConfig.Id, startConfig.Zone, sceneType, startConfig.Name);
            }
        }
    }
}