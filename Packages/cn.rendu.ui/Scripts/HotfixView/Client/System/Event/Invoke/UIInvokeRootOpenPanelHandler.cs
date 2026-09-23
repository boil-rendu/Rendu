using RD.Core;
namespace RD.Client
{
    [Invoke(EUIInvokeType.Async)]
    public class UIInvokeRootOpenPanelAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_SceneOpenPanel, Promise<bool>>
    {
        public override async Promise<bool> Handle(Entity entity, UIInvokeEntity_SceneOpenPanel args)
        {
            return await entity.UISceneRoot().OpenPanelAsync(args.PanelName) != null;
        }
    }

    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeRootOpenPanelSyncHandler : AInvokeEntityHandler<UIInvokeEntity_SceneOpenPanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_SceneOpenPanel args)
        {
            entity.UISceneRoot().OpenPanelAsync(args.PanelName).NoContext();
        }
    }
}