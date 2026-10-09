using RD.Core;
namespace RD.Client
{
    /// <summary>同步回主页：发起 HomePanel 后立即返回（NoContext 不等待）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeHomePanelHandler : AInvokeEntityHandler<UIInvokeEntity_HomePanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_HomePanel args)
        {
            entity.UIMgr().HomePanel(args.PanelName, args.Tween, args.ForceHome).NoContext();
        }
    }

    /// <summary>异步回主页：等待 HomePanel 完成，返回是否成功</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeHomePanelWaitHandler : AInvokeEntityHandler<UIInvokeEntity_HomePanel, Promise<bool>>
    {
        public override async Promise<bool> Handle(Entity entity, UIInvokeEntity_HomePanel args)
        {
            var uiMgr = entity.UIMgr();
            if (uiMgr == null) return false;
            return await uiMgr.HomePanel(args.PanelName, args.Tween, args.ForceHome);
        }
    }
}