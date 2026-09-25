using RD.Client;
using YooAsset;
using RD.Core;

namespace RD.Client
{
    [Invoke]
    public class UIInvokeYooAssetsHandler : AInvokeEntityHandler<UIInvokeEntity_LoadInitialize, Promise<bool>>
    {
        public override async Promise<bool> Handle(Entity entity, UIInvokeEntity_LoadInitialize args)
        {
            EntityRef<UILoadComponent> loadComponentRef = (UILoadComponent)entity;
            if (loadComponentRef.Entity == null)
            {
                Log.Error($"LoadComponent is null!");
                return false;
            }

            return await loadComponentRef.Entity.AddComponent<UIYooAssetsLoadComponent>().Initialize();
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetAssetsInfoSyncHandler : AInvokeEntityHandler<UIInvokeEntity_GetAssetInfo, AssetInfo>
    {
        public override AssetInfo Handle(Entity entity, UIInvokeEntity_GetAssetInfo args)
        {
            return entity.UILoad()?.GetComponent<UIYooAssetsLoadComponent>().GetAssetInfo(args.Location, args.AssetType);
        }
    }

    [Invoke(EInvokeType.Sync)]
    public class UIInvokeGetAssetInfoByGUIDSyncHandler : AInvokeEntityHandler<UIInvokeEntity_GetAssetInfoByGUID, AssetInfo>
    {
        public override AssetInfo Handle(Entity entity, UIInvokeEntity_GetAssetInfoByGUID args)
        {
            return entity.UILoad()?.GetComponent<UIYooAssetsLoadComponent>().GetAssetInfoByGUID(args.AssetGUID, args.AssetType);
        }
    }
}