using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 异步加载
    /// </summary>
    [FriendOf(typeof(UILoadComponent))]
    public static partial class UILoadComponentSystem
    {
        /// <summary>
        /// 异步加载资源对象
        /// 返回类型
        /// </summary>
        internal static async Promise<T> LoadAssetAsync<T>(this UILoadComponent self, string pkgName, string resName) where T : UnityObject
        {
            var load = LoadHelper.GetLoad(pkgName, resName);
            load.AddRefCount();
            if (load.Object != null)
            {
                return (T)load.Object;
            }

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UILoad, load.NameCode);

            if (load.Object != null)
            {
                return (T)load.Object;
            }

            var (obj, hashCode) = await UILoadDI.LoadAssetAsyncFunc(pkgName, resName, typeof(T));

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
            return (T)obj;
        }

        /// <summary>
        /// 异步加载资源对象
        /// 回调类型
        /// </summary>
        internal static void LoadAssetAsync<T>(this UILoadComponent self, string pkgName, string resName, Action<T> action) where T : UnityObject
        {
            self.LoadAssetAsyncAction(pkgName, resName, action).NoContext();
        }

        private static async Promise LoadAssetAsyncAction<T>(this UILoadComponent self, string pkgName, string resName, Action<T> action)
                where T : UnityObject
        {
            var asset = await self.LoadAssetAsync<T>(pkgName, resName);
            if (asset == null)
            {
                Debug.LogError($"异步加载对象失败 {pkgName} {resName}");
                return;
            }

            action?.Invoke(asset);
        }
    }
}