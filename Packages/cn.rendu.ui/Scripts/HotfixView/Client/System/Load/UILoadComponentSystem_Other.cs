using UnityObject = UnityEngine.Object;
using RD.Core;

namespace RD.Client
{
    [FriendOf(typeof(UILoadComponent))]
    public static partial class UILoadComponentSystem
    {
        /// <summary>
        /// 资源验证
        /// </summary>
        internal static bool VerifyAssetValidity(this UILoadComponent self, string pkgName, string resName)
        {
            return UILoadDI.VerifyAssetValidityFunc(pkgName, resName);
        }

        /// <summary>
        /// 释放某个资源对象
        /// 不包含实例化的对象
        /// 实例化的对象请调用另外一个实例化释放 ReleaseInstantiate
        /// </summary>
        internal static void Release(this UILoadComponent self, UnityObject obj)
        {
            LoadHelper.GetLoadHandle(obj)?.RemoveRefCount();
        }

        /// <summary>
        /// 一键释放所有 慎用
        /// </summary>
        internal static void ReleaseAll(this UILoadComponent self)
        {
            LoadHelper.PutAll();
            UILoadDI.ReleaseAllAction?.Invoke();
        }
    }
}
