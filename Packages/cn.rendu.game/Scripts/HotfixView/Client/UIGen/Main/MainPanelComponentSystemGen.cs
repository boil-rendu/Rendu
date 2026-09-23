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
    [EntitySystemOf(typeof(MainPanelComponent))]
    public static partial class MainPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MainPanelComponent self)
        {
        }

        [EntitySystem]
        private static void UIBind(this MainPanelComponent self)
        {
            self.UIBindInit();
        }

        private static void UIBindInit(this MainPanelComponent self)
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


        }
    }
}
