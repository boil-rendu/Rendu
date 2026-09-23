using System;
using UnityEngine;
using RD.Client;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    [FriendOf(typeof(MainPanelComponent))]
    public static partial class MainPanelComponentSystem
    {
        [EntitySystem]
        private static void UIInitialize(this MainPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this MainPanelComponent self)
        {
        }

        [EntitySystem]
        private static async Promise<bool> UIOpen(this MainPanelComponent self)
        {
            await Promise.CompletedTask;
            return true;
        }

        #region UIEvent开始
        #endregion UIEvent结束
    }
}
