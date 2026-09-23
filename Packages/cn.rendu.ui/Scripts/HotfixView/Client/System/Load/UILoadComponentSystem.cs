//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI面板组件
    /// </summary>
    [FriendOf(typeof(UILoadComponent))]
    [EntitySystemOf(typeof(UILoadComponent))]
    public static partial class UILoadComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILoadComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this UILoadComponent self)
        {
        }

        public static async Promise<bool> Initialize(this UILoadComponent self)
        {
            return await EventSystem.Instance.InvokeEntity<UIInvokeEntity_LoadInitialize, Promise<bool>>(self, new UIInvokeEntity_LoadInitialize());
        }
    }
}