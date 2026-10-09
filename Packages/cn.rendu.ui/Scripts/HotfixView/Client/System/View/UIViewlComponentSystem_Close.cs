using RD.Core;
namespace RD.Client
{
    /// <summary>UIViewComponent 扩展（关闭自身 / 快捷关闭所属 Panel）</summary>
    public static partial class UIViewComponentSystem
    {
        //关闭自己 同步
        /// <summary>关闭自己（发起后不等）</summary>
        public static void Close(this UIViewComponent self, bool tween = true)
        {
            self.CloseAsync(tween).NoContext();
        }

        //关闭自己 异步
        /// <summary>
        /// 异步关闭自己
        /// 若 OwnerUIEntity 实现了 IUIClose，先走关闭事件（返回 false 表示被拦截，直接中止关闭）；
        /// 再按 UIWindow 的 WindowCloseTweenBefore 开关决定窗口关闭回调的时机，
        /// 最后 SetActive(false) 并（若为 false 即后置）派发窗口关闭事件
        /// </summary>
        public static async Promise<bool> CloseAsync(this UIViewComponent self, bool tween = true)
        {
            EntityRef<UIViewComponent> selfRef = self;
            var view = self?.OwnerUIEntity;
            if (view != null)
            {
                var success = true;

                if (view is IUIClose)
                {
                    success = await UIEventSystem.Close(view);
                }

                self = selfRef;
                if (self.UIWindow is { WindowCloseTweenBefore: true })
                {
                    await UIEventSystem.WindowClose(self.UIWindow, success);
                }

                if (!success)
                {
                    self = selfRef;
                    Log.Info($"<color=yellow> 关闭事件返回不允许关闭View UI: {self.UIBase?.OwnerGameObject.name} </color>");
                    return false;
                }
            }

            self = selfRef;

            await self.UIWindow.InternalOnWindowCloseTween(tween);

            self = selfRef;

            self.UIBase.SetActive(false);

            if (self.UIWindow is { WindowCloseTweenBefore: false })
            {
                await UIEventSystem.WindowClose(self.UIWindow, true);
            }

            return true;
        }

        //标准view 可快捷关闭panel 需要满足panel的结构 同步
        /// <summary>快捷关闭所属 Panel（发起后不等）；要求 View 满足标准层级结构</summary>
        public static void ClosePanel(this UIViewComponent self, bool tween = true, bool ignoreElse = false)
        {
            EventSystem.Instance?.UIInvokeEntitySync(self, new UIInvokeEntity_ViewClosePanel
            {
                Tween = tween,
                IgnoreElse = ignoreElse
            });
        }

        //标准view 可快捷关闭panel 需要满足panel的结构 异步
        /// <summary>快捷关闭所属 Panel（异步）；要求 View 满足标准层级结构</summary>
        public static async Promise<bool> ClosePanelAsync(this UIViewComponent self, bool tween = true, bool ignoreElse = false)
        {
            return await EventSystem.Instance?.UIInvokeEntityAsync<UIInvokeEntity_ViewClosePanel, Promise<bool>>(self, new UIInvokeEntity_ViewClosePanel
            {
                Tween = tween,
                IgnoreElse = ignoreElse
            });
        }
    }
}