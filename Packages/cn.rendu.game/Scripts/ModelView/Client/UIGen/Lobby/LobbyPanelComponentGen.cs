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
    public partial class LobbyPanelComponent : Entity, IDestroy, IAwake, IUIBind, IUIInitialize, IUIOpen
    {
        public const string PkgName = "Lobby";
        public const string ResName = "LobbyPanel";

        public EntityRef<UIChild> u_UIBase;
        public UIChild UIBase => u_UIBase;
        public EntityRef<UIWindowComponent> u_UIWindow;
        public UIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<UIPanelComponent> u_UIPanel;
        public UIPanelComponent UIPanel => u_UIPanel;
        public UITaskEventP0 u_EventEnterMap;
        public UITaskEventHandleP0 u_EventEnterMapHandle;
        public const string OnEventEnterMapInvoke = "LobbyPanelComponent.OnEventEnterMapInvoke";

    }
}