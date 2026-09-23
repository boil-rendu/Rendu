using System;
using UnityEngine;
using RD.Client;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 由UI工具自动创建 请勿修改
    /// </summary>
    [FriendOf(typeof(UIChild))]
    [FriendOf(typeof(UIWindowComponent))]
    [FriendOf(typeof(UIPanelComponent))]
    [EntitySystemOf(typeof(LobbyPanelComponent))]
    public static partial class LobbyPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LobbyPanelComponent self)
        {
        }

        [EntitySystem]
        private static void UIBind(this LobbyPanelComponent self)
        {
            self.UIBindInit();
        }

        private static void UIBindInit(this LobbyPanelComponent self)
        {
            self.u_UIBase = self.GetParent<UIChild>();
            self.u_UIWindow = self.UIBase.GetComponent<UIWindowComponent>();
            self.u_UIPanel = self.UIBase.GetComponent<UIPanelComponent>();
            self.UIWindow.WindowOption = EWindowOption.None;
            self.UIPanel.Layer = EPanelLayer.Panel;
            self.UIPanel.PanelOption = EPanelOption.TimeCache;
            self.UIPanel.StackOption = EPanelStackOption.VisibleTween;
            self.UIPanel.Priority = 0;
            self.UIPanel.CachePanelTime = 10;

            self.u_EventEnterMap = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventEnterMap");
            self.u_EventEnterMapHandle = self.u_EventEnterMap.Add(self,LobbyPanelComponent.OnEventEnterMapInvoke);

        }
    }
}
