using RD.Core;
namespace RD.Client
{
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeClosePanelSyncHandler : AInvokeEntityHandler<UIInvokeEntity_ClosePanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_ClosePanel args)
        {
            entity.UIMgr()?.ClosePanel(args.PanelName, args.Tween, args.IgnoreElse, args.IgnoreLock);
        }
    }

    [Invoke(EInvokeType.Async)]
    public class UIInvokeClosePanelAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_ClosePanel, Promise<bool>>
    {
        public override async Promise<bool> Handle(Entity entity, UIInvokeEntity_ClosePanel args)
        {
            var uiMgr = entity.UIMgr();
            if (uiMgr == null) return false;
            return await uiMgr.ClosePanelAsync(args.PanelName, args.Tween, args.IgnoreElse, args.IgnoreLock);
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeViewClosePanelSyncHandler : AInvokeEntityHandler<UIInvokeEntity_ViewClosePanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_ViewClosePanel args)
        {
            var panel = entity?.Parent?.Parent?.Parent?.GetComponent<UIPanelComponent>();
            if (panel == null)
            {
                Log.Error($"错误当前view的结构不满足标准 无法向上找到Panel 并且关闭 请检查 {entity}");
                return;
            }

            panel.Close(args.Tween, args.IgnoreElse);
        }
    }

    [Invoke(EInvokeType.Async)]
    public class UIInvokeViewClosePanelAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_ViewClosePanel, Promise<bool>>
    {
        public override async Promise<bool> Handle(Entity entity, UIInvokeEntity_ViewClosePanel args)
        {
            var panel = entity?.Parent?.Parent?.Parent?.GetComponent<UIPanelComponent>();
            if (panel == null)
            {
                Log.Error($"错误当前view的结构不满足标准 无法向上找到Panel 并且关闭 请检查 {entity}");
                return false;
            }

            return await panel.CloseAsync(args.Tween, args.IgnoreElse);
        }
    }
}