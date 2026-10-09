using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIPanelComponent 扩展（缓存超时自动销毁面板）</summary>
    public static partial class UIPanelComponentSystem
    {
        /// <summary>
        /// 开始缓存倒计时：CachePanelTime &lt;= 0 表示不缓存，直接 RemoveUIReset；否则用取消令牌启动倒计时
        /// </summary>
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

        /// <summary>停止缓存倒计时（取消并清理令牌）</summary>
        internal static void StopCountDownDestroyPanel(this UIPanelComponent self)
        {
            if (self.m_Token == null) return;
            self.m_Token.Cancel();
            self.m_Token = null;
        }

        /// <summary>
        /// 缓存倒计时协程：等待 CachePanelTime 秒后 RemoveUIReset（使其可被回收）
        /// 等待期间若令牌已被取消（面板被重新打开/主动停止）则直接返回，不执行销毁
        /// </summary>
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

        /// <summary>通知 UIMgr 该面板不再参与回收复用（RemoveUIReset 请求）</summary>
        private static void RemoveUIReset(this UIPanelComponent self)
        {
            EventSystem.Instance?.UIInvokeEntitySync(self, new UIInvokeEntity_RemoveUIReset
            {
                PanelName = self.UIBindVo.ComponentType.Name
            });
        }
    }
}