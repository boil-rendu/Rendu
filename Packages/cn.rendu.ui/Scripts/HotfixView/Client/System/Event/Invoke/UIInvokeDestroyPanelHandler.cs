using RD.Core;
namespace RD.Client
{
    /// <summary>同步摧毁面板：调用 UIMgr.DestroyPanel（不移除回收，直接销毁）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeDestroyPanelHandler : AInvokeEntityHandler<UIInvokeEntity_DestroyPanel>
    {
        public override void Handle(Entity entity, UIInvokeEntity_DestroyPanel args)
        {
            entity.UIMgr().DestroyPanel(args.PanelName);
        }
    }
}