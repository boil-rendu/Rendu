using System;
using UnityEngine;
using RD.Client;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    [FriendOf(typeof(UICloseCommonComponent))]
    public static partial class UICloseCommonComponentSystem
    {
        [EntitySystem]
        private static void UIInitialize(this UICloseCommonComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this UICloseCommonComponent self)
        {
        }

        private static async Promise<bool> CloseUI(this UICloseCommonComponent self, EntityRef<Entity> parent)
        {
            if (parent.Entity.Parent == null)
            {
                Debug.LogError($"结构错误 无法找到可关闭UI {self.UIBase.OwnerGameObject}", self.UIBase.OwnerGameObject);
                return false;
            }

            EntityRef<UICloseCommonComponent> selfRef = self;
            if (parent.Entity.Parent is UIChild uiChild)
            {
                var panelComponent = uiChild.GetComponent<UIPanelComponent>();
                if (panelComponent != null)
                {
                    return await panelComponent.CloseAsync();
                }

                var viewComponent = uiChild.GetComponent<UIViewComponent>();
                if (viewComponent != null)
                {
                    return await viewComponent.CloseAsync();
                }
            }

            self = selfRef;
            return await self.CloseUI(parent.Entity.Parent);
        }

        #region UIEvent开始

        [Invoke(UICloseCommonComponent.OnEventCloseInvoke)]
        private static async Promise OnEventCloseInvoke(this UICloseCommonComponent self)
        {
            await self.CloseUI(self.Parent);
        }

        #endregion UIEvent结束
    }
}