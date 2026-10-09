//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIFactory 工厂（Destroy 部分）</summary>
    public static partial class UIFactory
    {
        /// <summary>销毁普通 UI 预制体并立刻释放其实例引用（Destroy + ReleaseInstantiate）</summary>
        internal static void Destroy(Scene scene, GameObject obj)
        {
            if (obj == null) return;
            UnityEngine.Object.Destroy(obj);
            EventSystem.Instance?.UIInvokeEntitySyncSafety(scene, new UIInvokeEntity_ReleaseInstantiate
            {
                obj = obj
            });
        }
    }
}