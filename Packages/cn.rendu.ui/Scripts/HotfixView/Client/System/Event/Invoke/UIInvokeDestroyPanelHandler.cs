using RD.Core;
namespace RD.Client
{
    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeDestroyPanelHandler : AInvokeEntityHandler<UIInvokeEntity_DestroyPanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_DestroyPanel args)
        {
            entity.UIMgr().DestroyPanel(args.PanelName);
        }
    }
}