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
    [EntitySystemOf(typeof(LoginPanelComponent))]
    public static partial class LoginPanelComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void UIBind(this LoginPanelComponent self)
        {
            self.UIBindInit();
        }

        private static void UIBindInit(this LoginPanelComponent self)
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

            self.u_ComAccount = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.InputField>("u_ComAccount");
            self.u_ComPassword = self.UIBase.ComponentTable.FindComponent<UnityEngine.UI.InputField>("u_ComPassword");
            self.u_EventLogin = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventLogin");
            self.u_EventLoginHandle = self.u_EventLogin.Add(self,LoginPanelComponent.OnEventLoginInvoke);
            self.u_EventWeChat = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventWeChat");
            self.u_EventWeChatHandle = self.u_EventWeChat.Add(self,LoginPanelComponent.OnEventWeChatInvoke);
            self.u_EventQQ = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventQQ");
            self.u_EventQQHandle = self.u_EventQQ.Add(self,LoginPanelComponent.OnEventQQInvoke);

        }
    }
}
