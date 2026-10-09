using System;
using UnityEngine;
using RD.Client;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    /// <summary>通用关闭按钮组件系统（挂在关闭按钮上，点击后关闭最近的 Panel 或 View）</summary>
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

        /// <summary>
        /// 从当前节点向上找第一个算是「可关闭 UI」的层级：
        /// 命中 UIPanelComponent 就关面板，命中 UIViewComponent 就关视图，都没有则继续向上递归
        /// </summary>
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

        /// <summary>关闭事件回调：从自身父级开始执行 CloseUI</summary>
        [Invoke(UICloseCommonComponent.OnEventCloseInvoke)]
        private static async Promise OnEventCloseInvoke(this UICloseCommonComponent self)
        {
            await self.CloseUI(self.Parent);
        }

        #endregion UIEvent结束
    }
}