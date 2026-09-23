using System.Net;
using RD.Core;

namespace RD.Server
{
    [Invoke(SceneType.RouterManager)]
    public class RouterManagerFiberInit: AInvokeHandler<FiberInit, Promise>
    {
        public override async Promise Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<TimerComponent>();
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.Get((int)root.Id);
            root.AddComponent<HttpComponent, string>($"http://*:{startSceneConfig.Port}/");

            await Promise.CompletedTask;
        }
    }
}