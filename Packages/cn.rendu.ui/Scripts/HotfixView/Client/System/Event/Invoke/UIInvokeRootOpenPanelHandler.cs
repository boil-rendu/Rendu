using RD.Core;
namespace RD.Client
{
    /// <summary>异步在场景根下打开面板：等待 OpenPanelAsync，按是否返回实体判定成功</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeRootOpenPanelAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_SceneOpenPanel, Promise<bool>>
    {
        public override async Promise<bool> Handle(Entity entity, UIInvokeEntity_SceneOpenPanel args)
        {
            return await entity.UISceneRoot().OpenPanelAsync(args.PanelName) != null;
        }
    }

    /// <summary>同步在场景根下打开面板：发起后立即返回（NoContext 不等待）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeRootOpenPanelSyncHandler : AInvokeEntityHandler<UIInvokeEntity_SceneOpenPanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_SceneOpenPanel args)
        {
            entity.UISceneRoot().OpenPanelAsync(args.PanelName).NoContext();
        }
    }
}