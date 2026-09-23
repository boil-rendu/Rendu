using RD.Client;
using RD.Core;

namespace RD.Client
{
    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeBanLayerOptionForeverHandler : AInvokeEntityHandler<UIInvokeEntity_BanLayerOptionForever, long>
    {
        public override long Handle(Entity entity, UIInvokeEntity_BanLayerOptionForever args)
        {
            return entity.UIMgr().BanLayerOptionForever();
        }
    }

    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeRecoverLayerOptionForeverHandler : AInvokeEntityHandler<UIInvokeEntity_RecoverLayerOptionForever>
    {
        public override void Handle(Entity entity, UIInvokeEntity_RecoverLayerOptionForever args)
        {
            entity.UIMgr().RecoverLayerOptionForever(args.ForeverCode);
        }
    }
}