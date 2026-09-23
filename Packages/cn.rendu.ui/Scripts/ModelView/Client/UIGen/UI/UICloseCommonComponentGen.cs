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
    [UI(EUICodeType.Common)]
    [ComponentOf(typeof(UIChild))]
    public partial class UICloseCommonComponent : Entity, IDestroy, IAwake, IUIBind, IUIInitialize
    {
        public const string PkgName = "UI";
        public const string ResName = "UICloseCommon";

        public EntityRef<UIChild> u_UIBase;
        public UIChild UIBase => u_UIBase;
        public UITaskEventP0 u_EventClose;
        public UITaskEventHandleP0 u_EventCloseHandle;
        public const string OnEventCloseInvoke = "UICloseCommonComponent.OnEventCloseInvoke";

    }
}