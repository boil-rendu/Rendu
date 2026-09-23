using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIPanelComponentSystem
    {
        internal static void CacheTimeCountDownDestroyPanel(this UIPanelComponent self)
        {
            if (self.CachePanelTime <= 0)
            {
                self.RemoveUIReset();
                return;
            }

            self.StopCountDownDestroyPanel();
            self.m_Token = new PromiseCancellationToken();
            self.DoCountDownDestroyPanel().WithContext(self.m_Token);
        }

        internal static void StopCountDownDestroyPanel(this UIPanelComponent self)
        {
            if (self.m_Token == null) return;
            self.m_Token.Cancel();
            self.m_Token = null;
        }

        private static async Promise DoCountDownDestroyPanel(this UIPanelComponent self)
        {
            EntityRef<UIPanelComponent> selfRef = self;

            try
            {
                var oldCancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();

                self = selfRef;
                await PromiseSafely.Await(selfRef.Entity?.Root()?.GetComponent<TimerComponent>()?.WaitAsync((long)(selfRef.Entity?.CachePanelTime * 1000 ?? 0)));

                if (oldCancellationToken != null && oldCancellationToken.IsCancel()) //取消倒计时
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return;
            }

            self = selfRef;
            self.m_Token = null;
            self.RemoveUIReset();
        }

        private static void RemoveUIReset(this UIPanelComponent self)
        {
            EventSystem.Instance?.UIInvokeEntitySync(self, new UIInvokeEntity_RemoveUIReset
            {
                PanelName = self.UIBindVo.ComponentType.Name
            });
        }
    }
}