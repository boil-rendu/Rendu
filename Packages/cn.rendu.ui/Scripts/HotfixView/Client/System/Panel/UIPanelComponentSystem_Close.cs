using RD.Core;
namespace RD.Client
{
    /// <summary>UIPanelComponent 扩展（关闭/回主页）</summary>
    public static partial class UIPanelComponentSystem
    {
        /// <summary>关闭面板（发起后不等，走 CloseAsync.NoContext）</summary>
        public static void Close(this UIPanelComponent self, bool tween = true, bool ignoreElse = false, bool ignoreLock = false)
        {
            self.CloseAsync(tween, ignoreElse, ignoreLock).NoContext();
        }

        /// <summary>
        /// 异步关闭面板：以自身组件类型名作为 PanelName 发起 ClosePanel 请求
        /// ignoreElse 忽略其他面板、ignoreLock 忽略层级锁
        /// </summary>
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

        /// <summary>回到指定泛型面板（按类型名作为 PanelName）</summary>
        public static async Promise<bool> Home<T>(this UIPanelComponent self, bool tween = true) where T : Entity
        {
            return await EventSystem.Instance?.UIInvokeEntityAsync<UIInvokeEntity_HomePanel, Promise<bool>>(self, new UIInvokeEntity_HomePanel
            {
                PanelName = typeof(T).Name,
                Tween = tween
            });
        }

        /// <summary>回到指定名字的面板</summary>
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