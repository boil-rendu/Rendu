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
    [EntitySystemOf(typeof(UICloseCommonComponent))]
    public static partial class UICloseCommonComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UICloseCommonComponent self)
        {
        }

        [EntitySystem]
        private static void UIBind(this UICloseCommonComponent self)
        {
            self.UIBindInit();
        }

        private static void UIBindInit(this UICloseCommonComponent self)
        {
            self.u_UIBase = self.GetParent<UIChild>();

            self.u_EventClose = self.UIBase.EventTable.FindEvent<UITaskEventP0>("u_EventClose");
            self.u_EventCloseHandle = self.u_EventClose.Add(self,UICloseCommonComponent.OnEventCloseInvoke);

        }
    }
}
