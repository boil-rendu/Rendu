using RD.Client;
using RD.Core;

namespace RD.Client
{
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeReleaseSyncHandler : AInvokeEntityHandler<UIInvokeEntity_Release>
    {
        public override void Handle(Entity entity, UIInvokeEntity_Release args)
        {
            entity.UILoad().Release(args.obj);
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeReleaseInstantiateSyncHandler : AInvokeEntityHandler<UIInvokeEntity_ReleaseInstantiate>
    {
        public override void Handle(Entity entity, UIInvokeEntity_ReleaseInstantiate args)
        {
            entity.UILoad().ReleaseInstantiate(args.obj);
        }
    }
}