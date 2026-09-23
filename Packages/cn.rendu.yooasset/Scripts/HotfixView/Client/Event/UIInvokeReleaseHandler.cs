using RD.Client;
using RD.Core;

namespace RD.Client
{
    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeReleaseSpriteSyncHandler : AInvokeEntityHandler<UIInvokeEntity_ReleaseSprite>
    {
        public override void Handle(Entity entity, UIInvokeEntity_ReleaseSprite args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();
            if (loadRef.Entity == null)
            {
                return;
            }

            loadRef.Entity.GetComponent<UIYooAssetsSpriteComponent>()?.ReleaseSprite(args.obj);
        }
    }
}