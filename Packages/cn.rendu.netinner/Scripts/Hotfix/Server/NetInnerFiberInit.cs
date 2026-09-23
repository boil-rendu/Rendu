using System.Net;
using RD.Core;

namespace RD.Server
{
    [Invoke(SceneType.NetInner)]
    public class NetInnerFiberInit: AInvokeHandler<FiberInit, Promise>
    {
        public override async Promise Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            StartProcessConfig startProcessConfig = StartProcessConfigCategory.Instance.Get(fiberInit.Fiber.Process);
            root.AddComponent<ProcessOuterSender, IPEndPoint>(startProcessConfig.IPEndPoint);
            root.AddComponent<ProcessInnerSender>();

            await Promise.CompletedTask;
        }
    }
}