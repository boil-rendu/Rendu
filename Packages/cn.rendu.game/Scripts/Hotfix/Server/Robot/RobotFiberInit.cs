using RD.Core;

namespace RD.Server
{
    [Invoke(SceneType.Robot)]
    public class RobotFiberInit: AInvokeHandler<FiberInit, Promise>
    {
        public override async Promise Handle(FiberInit fiberInit)
        {
            // TODO: Robot 初始化逻辑待后续整理
            /*
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, int>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            root.AddComponent<ObjectWait>();

            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
            await LoginHelper.Login(root, "127.0.0.1:10101", root.Name, "");
            await EnterMapHelper.EnterMapAsync(root);

            root.AddComponent<AIComponent, int>(1);
            */

            await Promise.CompletedTask;
        }
    }
}