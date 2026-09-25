using RD.Core;
namespace RD.Client
{
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeRemoveUIResetHandler : AInvokeEntityHandler<UIInvokeEntity_RemoveUIReset>
    {
        public override void Handle(Entity entity, UIInvokeEntity_RemoveUIReset args)
        {
            entity.UIMgr().RemoveUIReset(args.PanelName);
        }
    }
}