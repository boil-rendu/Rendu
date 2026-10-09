using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIRootComponent 扩展（按泛型类型打开面板并等待 NotifyWait）</summary>
    public static partial class UIRootComponentSystem
    {
        /// <summary>按泛型类型打开面板并等待（0 参）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitAsync<T>(this UIRootComponent self)
                where T : Entity, IAwake, IUIOpen
        {
            return await self.UIMgr.OpenPanelWaitAsync<T>(self);
        }

        /// <summary>按泛型类型打开面板并等待（变长参数版）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitParamAsync<T>(this UIRootComponent self, params object[] paramMore)
                where T : Entity, IUIOpen<ParamVo>
        {
            return await self.UIMgr.OpenPanelWaitParamAsync<T>(self, paramMore);
        }

        /// <summary>按泛型类型打开面板并等待（1 参）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1>(this UIRootComponent self, P1 p1)
                where T : Entity, IUIOpen<P1>
        {
            return await self.UIMgr.OpenPanelWaitAsync<T, P1>(self, p1);
        }

        /// <summary>按泛型类型打开面板并等待（2 参）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2>(this UIRootComponent self, P1 p1, P2 p2)
                where T : Entity, IUIOpen<P1, P2>
        {
            return await self.UIMgr.OpenPanelWaitAsync<T, P1, P2>(self, p1, p2);
        }

        /// <summary>按泛型类型打开面板并等待（3 参）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3>(this UIRootComponent self, P1 p1, P2 p2, P3 p3)
                where T : Entity, IUIOpen<P1, P2, P3>
        {
            return await self.UIMgr.OpenPanelWaitAsync<T, P1, P2, P3>(self, p1, p2, p3);
        }

        /// <summary>按泛型类型打开面板并等待（4 参）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3, P4>(this UIRootComponent self, P1 p1, P2 p2, P3 p3, P4 p4)
                where T : Entity, IUIOpen<P1, P2, P3, P4>
        {
            return await self.UIMgr.OpenPanelWaitAsync<T, P1, P2, P3, P4>(self, p1, p2, p3, p4);
        }

        /// <summary>按泛型类型打开面板并等待（5 参）</summary>
        public static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3, P4, P5>(this UIRootComponent self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
                where T : Entity, IUIOpen<P1, P2, P3, P4, P5>
        {
            return await self.UIMgr.OpenPanelWaitAsync<T, P1, P2, P3, P4, P5>(self, p1, p2, p3, p4, p5);
        }
    }
}