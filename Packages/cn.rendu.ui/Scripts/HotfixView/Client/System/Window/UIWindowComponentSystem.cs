//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UI窗口组件
    /// </summary>
    [FriendOf(typeof(UIWindowComponent))]
    [EntitySystemOf(typeof(UIWindowComponent))]
    public static partial class UIWindowComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UIWindowComponent self)
        {
            self.UIBindVo = self.UIBase.UIBindVo;
        }

        [EntitySystem]
        private static void Destroy(this UIWindowComponent self)
        {
        }

        internal static async Promise InternalOnWindowOpenTween(this UIWindowComponent self, bool tween = true)
        {
            self.UIBase.SetActive(true);

            if (tween && (!self.WindowBanRepetitionOpenTween || !self.m_FirstOpenTween))
            {
                self.m_FirstOpenTween = true;

                if (self.WindowBanAwaitOpenTween)
                {
                    self.SealedOnWindowOpenTween().NoContext();
                }
                else
                {
                    await self.SealedOnWindowOpenTween();
                }
            }
            else
            {
                self.OnOpenTweenEnd();
            }
        }

        internal static async Promise InternalOnWindowCloseTween(this UIWindowComponent self, bool tween = true)
        {
            if (tween && (!self.WindowBanRepetitionCloseTween || !self.m_FirstCloseTween))
            {
                self.m_FirstCloseTween = true;

                if (self.WindowBanAwaitCloseTween)
                {
                    self.SealedOnWindowCloseTween().NoContext();
                }
                else
                {
                    await self.SealedOnWindowCloseTween();
                }
            }
            else
            {
                self.OnCloseTweenEnd();
            }
        }

        //有可能没有动画 也有可能动画被跳过 反正无论如何都会有动画结束回调
        private static void OnOpenTweenEnd(this UIWindowComponent self)
        {
            if (self.OpenTweenEnd)
            {
                UIEventSystem.OpenTweenEnd(self.UIBase.OwnerUIEntity);
            }
        }

        private static void OnCloseTweenEnd(this UIWindowComponent self)
        {
            if (self.CloseTweenEnd)
            {
                UIEventSystem.CloseTweenEnd(self.UIBase.OwnerUIEntity);
            }

            self.UIBase.SetActive(false);
        }

        private static async Promise SealedOnWindowOpenTween(this UIWindowComponent self)
        {
            if (UIConstHelper.Const.IsLowQuality || self.WindowBanOpenTween)
            {
                self.OnOpenTweenEnd();
                return;
            }

            var foreverCode = self.WindowAllowOptionByTween ? 0 : EventSystem.Instance?.UIInvokeEntitySync<UIInvokeEntity_BanLayerOptionForever, long>(self, new UIInvokeEntity_BanLayerOptionForever()) ?? 0;
            EntityRef<UIWindowComponent> selfRef = self;
            try
            {
                await self.OnOpenTween();
            }
            catch (Exception e)
            {
                self = selfRef;
                Debug.LogError($"{self.UIBase.UIResName} 打开动画执行报错 {e}");
            }
            finally
            {
                self = selfRef;
                if (!self.WindowAllowOptionByTween)
                {
                    EventSystem.Instance?.UIInvokeEntitySync(self, new UIInvokeEntity_RecoverLayerOptionForever
                    {
                        ForeverCode = foreverCode
                    });
                }

                self.OnOpenTweenEnd();
            }
        }

        private static async Promise SealedOnWindowCloseTween(this UIWindowComponent self)
        {
            if (!self.UIBase.ActiveSelf || UIConstHelper.Const.IsLowQuality || self.WindowBanCloseTween)
            {
                self.OnCloseTweenEnd();
                return;
            }

            var foreverCode = self.WindowAllowOptionByTween ? 0 : EventSystem.Instance?.UIInvokeEntitySync<UIInvokeEntity_BanLayerOptionForever, long>(self, new UIInvokeEntity_BanLayerOptionForever()) ?? 0;

            EntityRef<UIWindowComponent> selfRef = self;
            try
            {
                await self.OnCloseTween();
            }
            catch (Exception e)
            {
                self = selfRef;
                Debug.LogError($"{self.UIBase.UIResName} 关闭动画执行报错 {e}");
            }
            finally
            {
                if (!self.WindowAllowOptionByTween)
                {
                    EventSystem.Instance?.UIInvokeEntitySync(self, new UIInvokeEntity_RecoverLayerOptionForever
                    {
                        ForeverCode = foreverCode
                    });
                }

                self.OnCloseTweenEnd();
            }
        }

        private static async Promise OnOpenTween(this UIWindowComponent self)
        {
            if (self.OpenTween)
            {
                await UIEventSystem.OpenTween(self.UIBase.OwnerUIEntity);
            }
            else
            {
                //panel会有默认动画
                //不要动画请在界面参数上调整 WindowBanTween
                //需要其他动画请实现动画事件
                if (self.UIBase.UIBindVo.CodeType == EUICodeType.Panel)
                {
                    await WindowFadeAnim.In(self.UIBase?.OwnerGameObject);
                }
            }
        }

        private static async Promise OnCloseTween(this UIWindowComponent self)
        {
            if (self.CloseTween)
            {
                await UIEventSystem.CloseTween(self.UIBase.OwnerUIEntity);
            }
            else
            {
                if (self.UIBase.UIBindVo.CodeType == EUICodeType.Panel)
                {
                    await WindowFadeAnim.Out(self.UIBase?.OwnerGameObject);
                }
            }
        }
    }
}