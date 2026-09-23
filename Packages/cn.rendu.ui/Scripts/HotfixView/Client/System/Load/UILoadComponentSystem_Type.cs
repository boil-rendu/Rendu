using System;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 不使用泛型 使用type加载的方式
    /// </summary>
    [FriendOf(typeof(UILoadComponent))]
    public static partial class UILoadComponentSystem
    {
        #if !UIMACRO_SYNCLOAD_CLOSE
        public static UnityObject LoadAsset(this UILoadComponent self, string pkgName, string resName, Type assetType)
        {
            var load = LoadHelper.GetLoad(pkgName, resName);
            load.AddRefCount();
            var loadObj = load.Object;
            if (loadObj != null)
            {
                return loadObj;
            }

            var (obj, hashCode) = UILoadDI.LoadAssetFunc(pkgName, resName, assetType);
            if (obj == null)
            {
                load.RemoveRefCount();
                return null;
            }

            if (!LoadHelper.AddLoadHandle(obj, load))
            {
                load.RemoveRefCount();
                return null;
            }

            load.ResetHandle(obj, hashCode);
            return obj;
        }
        #endif

        public static async Promise<UnityObject> LoadAssetAsync(this UILoadComponent self, string pkgName, string resName, Type assetType)
        {
            var load = LoadHelper.GetLoad(pkgName, resName);
            load.AddRefCount();
            if (load.Object != null)
            {
                return load.Object;
            }

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UILoad, load.NameCode);

            if (load.Object != null)
            {
                return load.Object;
            }

            var (obj, hashCode) = await UILoadDI.LoadAssetAsyncFunc(pkgName, resName, assetType);

            if (obj == null)
            {
                load.RemoveRefCount();
                return null;
            }

            if (!LoadHelper.AddLoadHandle(obj, load))
            {
                load.RemoveRefCount();
                return null;
            }

            load.ResetHandle(obj, hashCode);
            return obj;
        }

        private static void LoadAssetAsync(this UILoadComponent self, string pkgName, string resName, Type assetType, Action<UnityObject> action)
        {
            self.LoadAssetAsyncAction(pkgName, resName, assetType, action).NoContext();
        }

        private static async Promise LoadAssetAsyncAction(this UILoadComponent self, string pkgName, string resName, Type assetType, Action<UnityObject> action)
        {
            var asset = await self.LoadAssetAsync(pkgName, resName, assetType);
            if (asset == null)
            {
                Log.Error($"异步加载对象失败 {pkgName} {resName}");
                return;
            }

            action?.Invoke(asset);
        }
    }
}