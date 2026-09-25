using RD.Core;
namespace RD.Client
{
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeHomePanelHandler : AInvokeEntityHandler<UIInvokeEntity_HomePanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_HomePanel args)
        {
            entity.UIMgr().HomePanel(args.PanelName, args.Tween, args.ForceHome).NoContext();
        }
    }

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