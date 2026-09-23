using System;
using RD.Core;

namespace RD.Client
{
    [Event(SceneType.All)]
    public class On_UIEvent_YooAssetsLoad_Completed_SpriteHandler : AEvent<Scene, UIEvent_YooAssetsLoad_Completed>
    {
        protected override async Promise Run(Scene scene, UIEvent_YooAssetsLoad_Completed args)
        {
            var atlas = args.YooAssetsLoadRef.Entity.GetParent<UILoadComponent>().AddComponent<UIYooAssetsSpriteComponent>();
#if RD_UI_ATLAS_MODULE
            await atlas.LoadAtlasAsync();
#else
            await Promise.CompletedTask;
#endif
        }
    }
}