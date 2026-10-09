using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>永久禁用层级选项：返回一个 code 供之后恢复</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeBanLayerOptionForeverHandler : AInvokeEntityHandler<UIInvokeEntity_BanLayerOptionForever, long>
    {
        public override long Handle(Entity entity, UIInvokeEntity_BanLayerOptionForever args)
        {
            return entity.UIMgr().BanLayerOptionForever();
        }
    }

    /// <summary>按 code 恢复被永久禁用的层级选项</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeRecoverLayerOptionForeverHandler : AInvokeEntityHandler<UIInvokeEntity_RecoverLayerOptionForever>
    {
        public override void Handle(Entity entity, UIInvokeEntity_RecoverLayerOptionForever args)
        {
            entity.UIMgr().RecoverLayerOptionForever(args.ForeverCode);
        }
    }
}