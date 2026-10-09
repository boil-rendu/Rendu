using UnityEngine;
using RD.Core;

namespace RD.Client
{
    using Object = UnityEngine.Object;
    /// <summary>UnityEngine.Object 的扩展方法</summary>
    public static class UnityObjectExtensions
    {
        /// <summary>
        /// 安全销毁自身（null 直接忽略）
        /// 编辑器非运行态用 DestroyImmediate，运行态用 Destroy，避免编辑器下销毁延迟带来的误用
        /// </summary>
        public static void SafeDestroySelf(this Object obj)
        {
            if (obj == null) return;

            #if UNITY_EDITOR
            if (!Application.isPlaying)
                Object.DestroyImmediate(obj);
            else
                Object.Destroy(obj);
            #else
            Object.Destroy(obj);
            #endif
        }
    }
}
