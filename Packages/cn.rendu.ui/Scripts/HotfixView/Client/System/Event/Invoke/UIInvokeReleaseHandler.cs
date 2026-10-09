using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>同步释放资源引用（UILoad.Release）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeReleaseSyncHandler : AInvokeEntityHandler<UIInvokeEntity_Release>
    {
        public override void Handle(Entity entity, UIInvokeEntity_Release args)
        {
            entity.UILoad().Release(args.obj);
        }
    }

    /// <summary>同步释放实例化出来的对象（UILoad.ReleaseInstantiate）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeReleaseInstantiateSyncHandler : AInvokeEntityHandler<UIInvokeEntity_ReleaseInstantiate>
    {
        public override void Handle(Entity entity, UIInvokeEntity_ReleaseInstantiate args)
        {
            entity.UILoad().ReleaseInstantiate(args.obj);
        }
    }
}