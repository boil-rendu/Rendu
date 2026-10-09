using RD.Core;
namespace RD.Client
{
    /// <summary>UIRootComponent 扩展（按名称打开面板，转发给 self.UIMgr）</summary>
    public static partial class UIRootComponentSystem
    {
        /// <summary>按名称打开面板（0 参）</summary>
        public static async Promise<Entity> OpenPanelAsync(this UIRootComponent self, string componentName)
        {
            return await self.UIMgr.OpenPanelAsync(componentName, self);
        }

        /// <summary>按名称打开面板（变长参数版）</summary>
        public static async Promise<Entity> OpenPanelParamAsync(this UIRootComponent self, string componentName, params object[] paramMore)
        {
            return await self.UIMgr.OpenPanelParamAsync(componentName, self, paramMore);
        }

        /// <summary>按名称打开面板（1 参）</summary>
        public static async Promise<Entity> OpenPanelAsync<P1>(this UIRootComponent self, string componentName, P1 p1)
        {
            return await self.UIMgr.OpenPanelAsync(componentName, self, p1);
        }

        /// <summary>按名称打开面板（2 参）</summary>
        public static async Promise<Entity> OpenPanelAsync<P1, P2>(this UIRootComponent self, string componentName, P1 p1, P2 p2)
        {
            return await self.UIMgr.OpenPanelAsync(componentName, self, p1, p2);
        }

        /// <summary>按名称打开面板（3 参）</summary>
        public static async Promise<Entity> OpenPanelAsync<P1, P2, P3>(this UIRootComponent self, string componentName, P1 p1, P2 p2, P3 p3)
        {
            return await self.UIMgr.OpenPanelAsync(componentName, self, p1, p2, p3);
        }

        /// <summary>按名称打开面板（4 参）</summary>
        public static async Promise<Entity> OpenPanelAsync<P1, P2, P3, P4>(this UIRootComponent self, string componentName, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            return await self.UIMgr.OpenPanelAsync(componentName, self, p1, p2, p3, p4);
        }

        /// <summary>按名称打开面板（5 参）</summary>
        public static async Promise<Entity> OpenPanelAsync<P1, P2, P3, P4, P5>(this UIRootComponent self, string componentName, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            return await self.UIMgr.OpenPanelAsync(componentName, self, p1, p2, p3, p4, p5);
        }
    }
}