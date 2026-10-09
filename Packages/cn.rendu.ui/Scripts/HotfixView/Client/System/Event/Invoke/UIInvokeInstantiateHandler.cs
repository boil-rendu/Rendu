using UnityEngine;
using RD.Client;
using UnityGameObject = UnityEngine.GameObject;
using RD.Core;

namespace RD.Client
{
    /// <summary>异步实例化 UI（按加载类型）：走 UIFactory.InstantiateAsync</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeLoadInstantiateAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadInstantiate, Promise<Entity>>
    {
        public override async Promise<Entity> Handle(Entity entity, UIInvokeEntity_LoadInstantiate args)
        {
            return await UIFactory.InstantiateAsync(entity.Scene(), args.LoadType, args.ParentEntity, args.ParentTransform);
        }
    }

    #if !UIMACRO_SYNCLOAD_CLOSE
    /// <summary>同步实例化 UI：走 UIFactory.Instantiate（宏关闭时不可用）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeLoadInstantiateSyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadInstantiate, Entity>
    {
        public override Entity Handle(Entity entity, UIInvokeEntity_LoadInstantiate args)
        {
            return UIFactory.Instantiate(entity.Scene(), args.LoadType, args.ParentEntity, args.ParentTransform);
        }
    }
    #endif

    /// <summary>异步实例化 UI（按 Vo 配置）：走 UIFactory.InstantiateAsync</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeLoadInstantiateByVoAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadInstantiateByVo, Promise<Entity>>
    {
        public override async Promise<Entity> Handle(Entity entity, UIInvokeEntity_LoadInstantiateByVo args)
        {
            return await UIFactory.InstantiateAsync(entity.Scene(), args.BindVo, args.ParentEntity, args.ParentTransform);
        }
    }

    #if !UIMACRO_SYNCLOAD_CLOSE
    /// <summary>同步实例化 UI（按 Vo 配置）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeLoadInstantiateByVoSyncHandler : AInvokeEntityHandler<UIInvokeEntity_LoadInstantiateByVo, Entity>
    {
        public override Entity Handle(Entity entity, UIInvokeEntity_LoadInstantiateByVo args)
        {
            return UIFactory.Instantiate(entity.Scene(), args.BindVo, args.ParentEntity, args.ParentTransform);
        }
    }

    /// <summary>同步实例化一个纯 GameObject（不进 Entity 体系）</summary>
    [Invoke(EInvokeType.Sync)]
    public class UIInvokeInstantiateGameObjectSyncHandler : AInvokeEntityHandler<UIInvokeEntity_InstantiateGameObject, UnityGameObject>
    {
        public override UnityGameObject Handle(Entity entity, UIInvokeEntity_InstantiateGameObject args)
        {
            return UIFactory.InstantiateGameObject(entity.Scene(), args.PkgName, args.ResName);
        }
    }
    #endif

    /// <summary>异步实例化一个纯 GameObject（不进 Entity 体系）</summary>
    [Invoke(EInvokeType.Async)]
    public class UIInvokeInstantiateGameObjectAsyncHandler : AInvokeEntityHandler<UIInvokeEntity_InstantiateGameObject, Promise<UnityGameObject>>
    {
        public override async Promise<UnityGameObject> Handle(Entity entity, UIInvokeEntity_InstantiateGameObject args)
        {
            return await UIFactory.InstantiateGameObjectAsync(entity.Scene(), args.PkgName, args.ResName);
        }
    }
}