using RD.Client;
using RD.Core;

namespace RD.Client
{
    public static partial class UIRootComponentSystem
    {
        public static async Promise<bool> PreLoadPanelAsync<T>(this UIRootComponent self, bool loadEntity = true)
                where T : Entity, IAwake, IUIOpen
        {
            return await self.UIMgr.PreLoadPanelAsync<T>(self, loadEntity);
        }

        public static async Promise<bool> PreLoadPanelAsync(this UIRootComponent self, string panelName, bool loadEntity = true)
        {
            return await self.UIMgr.PreLoadPanelAsync(panelName, self, loadEntity);
        }
    }
}