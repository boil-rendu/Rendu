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
    [UI(EUICodeType.Panel, EPanelLayer.Panel)]
    [ComponentOf(typeof(UIChild))]
    public partial class LoginPanelComponent : Entity, IDestroy, IAwake, IUIBind, IUIInitialize, IUIOpen
    {
        public const string PkgName = "Login";
        public const string ResName = "LoginPanel";

        public EntityRef<UIChild> u_UIBase;
        public UIChild UIBase => u_UIBase;
        public EntityRef<UIWindowComponent> u_UIWindow;
        public UIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<UIPanelComponent> u_UIPanel;
        public UIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.UI.InputField u_ComAccount;
        public UnityEngine.UI.InputField u_ComPassword;
        public UITaskEventP0 u_EventLogin;
        public UITaskEventHandleP0 u_EventLoginHandle;
        public const string OnEventLoginInvoke = "LoginPanelComponent.OnEventLoginInvoke";
        public UITaskEventP0 u_EventWeChat;
        public UITaskEventHandleP0 u_EventWeChatHandle;
        public const string OnEventWeChatInvoke = "LoginPanelComponent.OnEventWeChatInvoke";
        public UITaskEventP0 u_EventQQ;
        public UITaskEventHandleP0 u_EventQQHandle;
        public const string OnEventQQInvoke = "LoginPanelComponent.OnEventQQInvoke";

    }
}