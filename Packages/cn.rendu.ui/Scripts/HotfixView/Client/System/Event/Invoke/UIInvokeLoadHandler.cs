using UnityEngine;
using RD.Client;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    #if !UIMACRO_SYNCLOAD_CLOSE
    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeLoadSyncHandler : AInvokeEntityHandler<UIInvokeEntity_Load, UnityObject>
    {
        public override UnityObject Handle(Entity entity, UIInvokeEntity_Load args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();

            if (loadRef.Entity == null) return null;

            var resName = args.ResName;

            var obj = loadRef.Entity.LoadAsset(args.PkgName, resName, args.LoadType);

            if (obj == null)
            {
                Log.Error($"加载失败 没有这个资源 请检查 {resName}");
                return null;
            }

            return obj;
        }
    }
    #endif

    [Invoke(EUIInvokeType.Async)]
    public class UIInvokeLoadAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_Load, Promise<UnityObject>>
    {
        public override async Promise<UnityObject> Handle(Entity entity, UIInvokeEntity_Load args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();

            if (loadRef.Entity == null) return null;

            var resName = args.ResName;

            var obj = await loadRef.Entity.LoadAssetAsync(args.PkgName, resName, args.LoadType);

            if (obj == null)
            {
                Log.Error($"加载失败 没有这个资源 请检查 {resName}");
                return null;
            }

            return obj;
        }
    }

    #if !UIMACRO_SYNCLOAD_CLOSE
    [Invoke(EUIInvokeType.Sync)]
    public class UIInvokeLoadTexture2DSyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadTexture2D, Texture2D>
    {
        public override Texture2D Handle(Entity entity, UIInvokeEntity_LoadTexture2D args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();

            if (loadRef.Entity == null) return null;

            var resName = args.ResName;

            #if UNITY_EDITOR
            if (!loadRef.Entity.VerifyAssetValidity(resName))
            {
                Log.Error($"验证资产有效性 没有这个资源 图片无法加载 请检查 {resName}");
                return null;
            }
            #endif

            var texture2D = loadRef.Entity.LoadAsset<Texture2D>(resName);

            if (texture2D == null)
            {
                Log.Error($"加载失败 没有这个资源 图片无法加载 请检查 {resName}");
                return null;
            }

            return texture2D;
        }
    }
    #endif

    [Invoke(EUIInvokeType.Async)]
    public class UIInvokeLoadTexture2DAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadTexture2D, Promise<Texture2D>>
    {
        public override async Promise<Texture2D> Handle(Entity entity, UIInvokeEntity_LoadTexture2D args)
        {
            EntityRef<UILoadComponent> loadRef = entity.UILoad();

            if (loadRef.Entity == null) return null;

            var resName = args.ResName;

            #if UNITY_EDITOR
            if (!loadRef.Entity.VerifyAssetValidity(resName))
            {
                Log.Error($"验证资产有效性 没有这个资源 图片无法加载 请检查 {resName}");
                return null;
            }
            #endif

            var texture2D = await loadRef.Entity.LoadAssetAsync<Texture2D>(resName);

            if (texture2D == null)
            {
                Log.Error($"加载失败 没有这个资源 图片无法加载 请检查 {resName}");
                return null;
            }

            return texture2D;
        }
    }
}