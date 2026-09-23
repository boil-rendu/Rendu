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
    public static partial class UIFactory
    {
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