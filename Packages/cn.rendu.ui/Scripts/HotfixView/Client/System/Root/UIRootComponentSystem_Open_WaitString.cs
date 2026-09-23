using RD.Core;
namespace RD.Client
{
    public static partial class UIRootComponentSystem
    {
        public static async Promise<EHashWaitError> OpenPanelWaitAsync(this UIRootComponent self, string componentName)
        {
            return await self.UIMgr.OpenPanelWaitAsync(componentName, self);
        }

        public static async Promise<EHashWaitError> OpenPanelWaitParamAsync(this UIRootComponent self, string componentName, params object[] paramMore)
        {
            return await self.UIMgr.OpenPanelWaitParamAsync(componentName, self, paramMore);
        }

        public static async Promise<EHashWaitError> OpenPanelWaitAsync<P1>(this UIRootComponent self, string componentName, P1 p1)
        {
            return await self.UIMgr.OpenPanelWaitAsync(componentName, self, p1);
        }

        public static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2>(this UIRootComponent self, string componentName, P1 p1, P2 p2)
        {
            return await self.UIMgr.OpenPanelWaitAsync(componentName, self, p1, p2);
        }

        public static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2, P3>(this UIRootComponent self, string componentName, P1 p1, P2 p2, P3 p3)
        {
            return await self.UIMgr.OpenPanelWaitAsync(componentName, self, p1, p2, p3);
        }

        public static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2, P3, P4>(this UIRootComponent self, string componentName, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            return await self.UIMgr.OpenPanelWaitAsync(componentName, self, p1, p2, p3, p4);
        }

        public static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2, P3, P4, P5>(this UIRootComponent self, string componentName, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            return await self.UIMgr.OpenPanelWaitAsync(componentName, self, p1, p2, p3, p4, p5);
        }
    }
}
