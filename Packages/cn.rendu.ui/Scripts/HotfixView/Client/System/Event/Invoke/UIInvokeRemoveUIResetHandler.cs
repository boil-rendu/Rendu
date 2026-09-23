using RD.Core;
namespace RD.Client
{
    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeRemoveUIResetHandler : AInvokeEntityHandler<UIInvokeEntity_RemoveUIReset>
    {
        public override void Handle(Entity entity, UIInvokeEntity_RemoveUIReset args)
        {
            entity.UIMgr().RemoveUIReset(args.PanelName);
        }
    }
}