using System;
using UnityEngine;
using RD.Client;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    [FriendOf(typeof(LobbyPanelComponent))]
    public static partial class LobbyPanelComponentSystem
    {
        [EntitySystem]
        private static void UIInitialize(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static async Promise<bool> UIOpen(this LobbyPanelComponent self)
        {
            await Promise.CompletedTask;
            return true;
        }

        #region UIEvent开始

        [UIInvoke(LobbyPanelComponent.OnEventEnterMapInvoke)]
        private static async Promise OnEventEnterMapInvoke(this LobbyPanelComponent self)
        {
            self.UIBase.SetActive(false);
            await EnterMapHelper.EnterMapAsync(self.Root());
            await self.UIPanel.CloseAsync();
        }

        #endregion UIEvent结束
    }
}