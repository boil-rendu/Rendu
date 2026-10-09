using RD.Core;
namespace RD.Client
{
    /// <summary>同步移除 UI 重置项：调用 UIMgr.RemoveUIReset，使该面板不再被回收复用</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeRemoveUIResetHandler : AInvokeEntityHandler<UIInvokeEntity_RemoveUIReset>
    {
        public override void Handle(Entity entity, UIInvokeEntity_RemoveUIReset args)
        {
            entity.UIMgr().RemoveUIReset(args.PanelName);
        }
    }
}