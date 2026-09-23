using System;
using UnityEngine;
using RD.Client;
using System.Collections.Generic;
using RD.Core;
using RD.Loader;

namespace RD.Client
{
    [FriendOf(typeof(LoginPanelComponent))]
    public static partial class LoginPanelComponentSystem
    {
        [EntitySystem]
        private static void UIInitialize(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LoginPanelComponent self)
        {
        }

        [EntitySystem]
        private static async Promise<bool> UIOpen(this LoginPanelComponent self)
        {
            await Promise.CompletedTask;
            return true;
        }

        #region UIEvent开始

        [UIInvoke(LoginPanelComponent.OnEventLoginInvoke)]
        private static async Promise OnEventLoginInvoke(this LoginPanelComponent self)
        {
            Log.Info($"登录");
            await self.DoLogin("账号");
        }

        [UIInvoke(LoginPanelComponent.OnEventWeChatInvoke)]
        private static async Promise OnEventWeChatInvoke(this LoginPanelComponent self)
        {
            Log.Info($"第三方登录点击：微信（未接入SDK，按普通登录进入）");
            await self.DoLogin("微信");
        }

        [UIInvoke(LoginPanelComponent.OnEventQQInvoke)]
        private static async Promise OnEventQQInvoke(this LoginPanelComponent self)
        {
            Log.Info($"第三方登录点击：QQ（未接入SDK，按普通登录进入）");
            await self.DoLogin("QQ");
        }

        /// <summary>
        /// 统一登录入口：账号登录与第三方登录渠道都走这里
        /// </summary>
        private static async Promise DoLogin(this LoginPanelComponent self, string channel)
        {
            if (string.IsNullOrWhiteSpace(self.u_ComAccount.text))
            {
                // 账号为空时兜底默认测试账号，保证第三方登录可直接进入
                self.u_ComAccount.text = "test";
            }

            if (string.IsNullOrWhiteSpace(self.u_ComPassword.text))
            {
                self.u_ComPassword.text = "123456";
            }

            Log.Info($"[登录] 渠道={channel} 账号={self.u_ComAccount.text}");

            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            await LoginHelper.Login(self.Root(),
                globalComponent.GlobalConfig.Address,
                self.u_ComAccount.text,
                self.u_ComPassword.text);
        }

        #endregion UIEvent结束
    }
}