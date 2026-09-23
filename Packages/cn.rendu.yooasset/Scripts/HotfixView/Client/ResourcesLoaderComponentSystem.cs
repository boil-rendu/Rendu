using System;
using System.Collections.Generic;
using RD.Core;
using RD.YooAsset;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;

namespace RD.Client
{
    [EntitySystemOf(typeof(ResourcesLoaderComponent))]
    public static partial class ResourcesLoaderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self)
        {
            self.package = YooAssets.GetPackage(ResourcesComponent.Instance.PackageName);
        }

        [EntitySystem]
        private static void Awake(this ResourcesLoaderComponent self, string packageName)
        {
            self.package = YooAssets.GetPackage(packageName);
        }

        [EntitySystem]
        private static void Destroy(this ResourcesLoaderComponent self)
        {
            foreach (var kv in self.handlers)
            {
                switch (kv.Value)
                {
                    case AssetHandle handle:
                        handle.Release();
                        break;
                    case AllAssetsHandle handle:
                        handle.Release();
                        break;
                    case SubAssetsHandle handle:
                        handle.Release();
                        break;
                    case BundleFileHandle handle:
                        handle.Release();
                        break;
                    case SceneHandle handle:
                        handle.UnloadSceneAsync();
                        break;
                }
            }
        }

        public static async Promise<T> LoadAssetAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            // 命中缓存
            if (self.handlers.TryGetValue(location, out var handleBase))
            {
                if (handleBase is AssetHandle cachedHandle)
                    return (T)cachedHandle.AssetObject;

                // 缓存类型不匹配（如被 LoadAllAssetsAsync 占用），移除旧 handle 重新加载
                self.handlers.Remove(location);
            }

            AssetHandle assetHandle = self.package.LoadAssetAsync<T>(location);
            await assetHandle;

            if (assetHandle.Status != EOperationStatus.Succeeded)
            {
                Debug.LogError($"Load asset [{location}] failed: {assetHandle.Error}");
                return null;
            }

            self.handlers[location] = assetHandle;
            return (T)assetHandle.AssetObject;
        }

        public static async Promise<Dictionary<string, T>> LoadAllAssetsAsync<T>(this ResourcesLoaderComponent self, string location) where T : UnityEngine.Object
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            HandleBase handler;
            if (!self.handlers.TryGetValue(location, out handler))
            {
                AllAssetsHandle allAssetsAsyncHandle = self.package.LoadAllAssetsAsync<T>(location);
                await allAssetsAsyncHandle;
                self.handlers.Add(location, allAssetsAsyncHandle);
                handler = allAssetsAsyncHandle;
            }

            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            foreach (UnityEngine.Object assetObj in ((AllAssetsHandle)handler).AllAssetObjects)
            {
                T t = assetObj as T;
                dictionary.Add(t.name, t);
            }

            return dictionary;
        }

        public static async Promise LoadSceneAsync(this ResourcesLoaderComponent self, string location, LoadSceneMode loadSceneMode, Action<float> progressCallback = null)
        {
            using CoroutineLock coroutineLock = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.ResourcesLoader, location.GetHashCode());

            if (self.handlers.TryGetValue(location, out var handler))
            {
                progressCallback?.Invoke(1f);
                return;
            }
            SceneHandle handle = self.package.LoadSceneAsync(location);

            // 加载过程中实时回调进度
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            while (!handle.IsDone)
            {
                progressCallback?.Invoke(handle.Progress);
                await timerComponent.WaitFrameAsync();
            }

            await handle;
            self.handlers.Add(location, handle);
        }
    }
}

