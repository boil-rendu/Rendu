using UnityEngine;
using UnityEngine.U2D;
using RD.Client;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    #if !UIMACRO_SYNCLOAD_CLOSE
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeLoadSpriteSyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadSprite, Sprite>
    {
        public override Sprite Handle(Entity entity, UIInvokeEntity_LoadSprite args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();
            if (loadRef.Entity == null)
            {
                return null;
            }

            var resName = args.ResName;
            var spriteComponent = loadRef.Entity.GetComponent<UIYooAssetsSpriteComponent>();
            var sprite = spriteComponent.GetSprite(resName);
            if (sprite == null)
            {
                return null;
            }

            return sprite;
        }
    }
    #endif

    [Invoke(EInvokeType.Async)]
    public class UIInvokeLoadSpriteAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadSprite, Promise<Sprite>>
    {
        public override async Promise<Sprite> Handle(Entity entity, UIInvokeEntity_LoadSprite args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();
            if (loadRef.Entity == null)
            {
                return null;
            }

            var resName = args.ResName;
            var spriteComponent = loadRef.Entity.GetComponent<UIYooAssetsSpriteComponent>();
            var sprite = await spriteComponent.GetSpriteAsync(resName);
            if (sprite == null)
            {
                return null;
            }

            return sprite;
        }
    }
}