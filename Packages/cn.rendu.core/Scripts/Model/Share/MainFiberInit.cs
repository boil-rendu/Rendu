using RD.Core;

namespace RD
{
    [Invoke(SceneType.Main)]
    public class MainFiberInit: AInvokeHandler<FiberInit, Promise>
    {
        public override async Promise Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            
            int sceneType = SceneTypeSingleton.Instance.GetSceneType(Options.Instance.SceneName);
            root.SceneType = sceneType;
            
            await EventSystem.Instance.PublishAsync(root, new InitShare());
            await EventSystem.Instance.PublishAsync(root, new InitServer());
            await EventSystem.Instance.PublishAsync(root, new InitClient());
        }
    }
}