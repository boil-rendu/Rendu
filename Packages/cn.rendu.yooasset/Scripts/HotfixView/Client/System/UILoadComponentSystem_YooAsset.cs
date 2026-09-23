using System;
using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// YooAsset扩展  因为他不需要pkgName
    /// </summary>
    [FriendOf(typeof(UILoadComponent))]
    public static partial class UILoadComponentSystem
    {
        #if !UIMACRO_SYNCLOAD_CLOSE
        internal static T LoadAsset<T>(this UILoadComponent self, string resName) where T : UnityObject
        {
            return self.LoadAsset<T>("", resName);
        }
        #endif

        internal static async Promise<T> LoadAssetAsync<T>(this UILoadComponent self, string resName) where T : UnityObject
        {
            return await self.LoadAssetAsync<T>("", resName);
        }

        internal static void LoadAssetAsync<T>(this UILoadComponent self, string resName, Action<T> action) where T : UnityObject
        {
            self.LoadAssetAsync<T>("", resName, action);
        }

        internal static bool VerifyAssetValidity(this UILoadComponent self, string resName)
        {
            return self.VerifyAssetValidity("", resName);
        }

        #region 非泛型

        #if !UIMACRO_SYNCLOAD_CLOSE
        internal static UnityObject LoadAsset(this UILoadComponent self, string resName, Type assetType)
        {
            return self.LoadAsset("", resName, assetType);
        }
        #endif

        internal static async Promise<UnityObject> LoadAssetAsync(this UILoadComponent self, string resName, Type assetType)
        {
            return await self.LoadAssetAsync("", resName, assetType);
        }

        internal static void LoadAssetAsync(this UILoadComponent self, string resName, Type assetType, Action<UnityObject> action)
        {
            self.LoadAssetAsync("", resName, assetType, action);
        }

        #endregion
    }
}