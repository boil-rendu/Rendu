using RD.Core;
namespace RD.Client
{
    public static partial class UIPanelComponentSystem
    {
        public static void Close(this UIPanelComponent self, bool tween = true, bool ignoreElse = false, bool ignoreLock = false)
        {
            self.CloseAsync(tween, ignoreElse, ignoreLock).NoContext();
        }

        public static async Promise<bool> CloseAsync(this UIPanelComponent self, bool tween = true, bool ignoreElse = false, bool ignoreLock = false)
        {
            return await EventSystem.Instance?.UIInvokeEntityAsync<UIInvokeEntity_ClosePanel, Promise<bool>>(self, new UIInvokeEntity_ClosePanel
            {
                PanelName = self.UIBindVo.ComponentType.Name,
                Tween = tween,
                IgnoreElse = ignoreElse,
                IgnoreLock = ignoreLock
            });
        }

        public static async Promise<bool> Home<T>(this UIPanelComponent self, bool tween = true) where T : Entity
        {
            return await EventSystem.Instance?.UIInvokeEntityAsync<UIInvokeEntity_HomePanel, Promise<bool>>(self, new UIInvokeEntity_HomePanel
            {
                PanelName = typeof(T).Name,
                Tween = tween
            });
        }

        public static async Promise<bool> Home(this UIPanelComponent self, string homeName, bool tween = true)
        {
            return await EventSystem.Instance?.UIInvokeEntityAsync<UIInvokeEntity_HomePanel, Promise<bool>>(self, new UIInvokeEntity_HomePanel
            {
                PanelName = homeName,
                Tween = tween
            });
        }
    }
}