using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIRootComponent 扩展（面板预加载）</summary>
    public static partial class UIRootComponentSystem
    {
        /// <summary>按泛型类型预加载面板（可只加载预制体不建 Entity）</summary>
        public static async Promise<bool> PreLoadPanelAsync<T>(this UIRootComponent self, bool loadEntity = true)
                where T : Entity, IAwake, IUIOpen
        {
            return await self.UIMgr.PreLoadPanelAsync<T>(self, loadEntity);
        }

        /// <summary>按名称预加载面板</summary>
        public static async Promise<bool> PreLoadPanelAsync(this UIRootComponent self, string panelName, bool loadEntity = true)
        {
            return await self.UIMgr.PreLoadPanelAsync(panelName, self, loadEntity);
        }
    }
}