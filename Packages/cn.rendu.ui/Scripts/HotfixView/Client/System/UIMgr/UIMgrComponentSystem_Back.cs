using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 界面回退功能 关闭 / 恢复 / Home
    /// </summary>
    public static partial class UIMgrComponentSystem
    {
        /// <summary>
        /// 打开一个同级UI时关闭其他UI 
        /// 只有Panel层才有这个逻辑
        /// </summary>
        internal static async Promise AddUICloseElse(this UIMgrComponent self, PanelInfo info)
        {
            if (info.UIPanel is not { Layer: EPanelLayer.Panel })
            {
                return;
            }

            if (info.UIPanel.PanelIgnoreBack)
            {
                return;
            }

            var layerList = self.GetLayerPanelInfoList(EPanelLayer.Panel);
            var skipTween = info.UIWindow.WindowSkipOtherCloseTween;

            EntityRef<UIMgrComponent> selfRef = self;
            for (var i = layerList.Count - 1; i >= 0; i--)
            {
                var child = layerList[i];

                if (child == null || child == info)
                {
                    continue;
                }

                var childPanel = child.UIPanel;
                if (childPanel == null)
                {
                    Log.Error($"错误,是否在异步过程中删除了对象");
                    continue;
                }

                //防止多级时多次触发
                switch (childPanel.StackOption)
                {
                    case EPanelStackOption.Visible:
                        if (!child.UIBase.ActiveSelf) continue;
                        break;
                    case EPanelStackOption.VisibleTween:
                        if (!child.UIBase.ActiveSelf) continue;
                        break;
                    case EPanelStackOption.None:
                        continue;
                    case EPanelStackOption.Omit: //此类型表示 都忽略判断
                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {childPanel.StackOption}");
                        if (!child.UIBase.ActiveSelf) continue;
                        break;
                }

                self = selfRef;

                await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelCloseBefore
                {
                    UIPkgName = child.PkgName,
                    UIResName = child.ResName,
                    UIComponentName = child.Name,
                    StackOption = true,
                    PanelLayer = child.PanelLayer,
                });

                if (child.OwnerUIEntity is IUIBackClose)
                {
                    await UIEventSystem.BackClose(child.OwnerUIEntity, new UIEventPanelInfo
                    {
                        UIPkgName = info.PkgName,
                        UIResName = info.ResName,
                        UIComponentName = info.Name,
                        PanelLayer = info.PanelLayer,
                    });
                }

                var uiPkgName = child.PkgName;
                var uiResName = child.ResName;
                var uiComponentName = child.Name;
                var panelLayer = child.PanelLayer;

                childPanel = child.UIPanel;
                if (childPanel == null)
                {
                    Log.Error($"错误,是否在异步过程中删除了对象");
                    continue;
                }

                switch (childPanel.StackOption)
                {
                    case EPanelStackOption.Omit:
                        await childPanel.CloseAsync(!skipTween, true, true);
                        break;
                    case EPanelStackOption.None:
                        break;
                    case EPanelStackOption.Visible:
                        child.UIBase.SetActive(false);
                        break;
                    case EPanelStackOption.VisibleTween:
                        if (!skipTween)
                        {
                            await childPanel.CloseAllViewTween();
                            var childWindow = child.UIWindow;
                            if (childWindow == null)
                            {
                                Log.Error($"错误,是否在异步过程中删除了对象");
                            }
                            else
                            {
                                await childWindow.InternalOnWindowCloseTween();
                            }
                        }

                        child.UIBase.SetActive(false);
                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {childPanel.StackOption}");
                        child.UIBase.SetActive(false);
                        break;
                }

                self = selfRef;

                await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelCloseAfter
                {
                    UIPkgName = uiPkgName,
                    UIResName = uiResName,
                    UIComponentName = uiComponentName,
                    StackOption = true,
                    PanelLayer = panelLayer,
                });
            }
        }

        internal static async Promise RemoveUIAddElse(this UIMgrComponent self, PanelInfo info)
        {
            if (info.UIPanel is not { Layer: EPanelLayer.Panel })
            {
                return;
            }

            if (info.UIPanel.PanelIgnoreBack)
            {
                return;
            }

            var layerList = self.GetLayerPanelInfoList(EPanelLayer.Panel);
            var skipTween = info.UIWindow.WindowSkipOtherOpenTween;

            EntityRef<UIMgrComponent> selfRef = self;

            for (var i = layerList.Count - 1; i >= 0; i--)
            {
                var child = layerList[i];

                if (child == null || child == info)
                {
                    continue;
                }

                self = selfRef;

                await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelOpenBefore
                {
                    UIPkgName = child.PkgName,
                    UIResName = child.ResName,
                    UIComponentName = child.Name,
                    StackOption = true,
                    PanelLayer = child.PanelLayer,
                });

                var isBreak = true;
                var childPanel = child.UIPanel;
                if (childPanel == null)
                {
                    Log.Error($"错误,是否在异步过程中删除了对象");
                    continue;
                }

                switch (childPanel.StackOption)
                {
                    case EPanelStackOption.Omit: //不可能进入这里因为他已经被关闭了 如果进入则跳过这个界面
                        isBreak = false;
                        break;
                    case EPanelStackOption.None:
                        break;
                    case EPanelStackOption.Visible:
                        child.UIBase.SetActive(true);
                        break;
                    case EPanelStackOption.VisibleTween:
                        if (!skipTween)
                        {
                            var childWindow = child.UIWindow;
                            if (childWindow == null)
                            {
                                Log.Error($"错误,是否在异步过程中删除了对象");
                            }
                            else
                            {
                                await childWindow.InternalOnWindowOpenTween();
                            }

                            childPanel = child.UIPanel;
                            if (childPanel == null)
                            {
                                Log.Error($"错误,是否在异步过程中删除了对象");
                            }
                            else
                            {
                                await childPanel.OpenAllViewTween();
                            }
                        }
                        else
                        {
                            child.UIBase.SetActive(true);
                            await childPanel.OpenAllViewTween(false);
                        }

                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {childPanel.StackOption}");
                        child.UIBase.SetActive(true);
                        break;
                }

                if (child.OwnerUIEntity is IUIBackOpen)
                {
                    await UIEventSystem.BackOpen(child.OwnerUIEntity, new UIEventPanelInfo
                    {
                        UIPkgName = info.PkgName,
                        UIResName = info.ResName,
                        UIComponentName = info.Name,
                        PanelLayer = info.PanelLayer,
                    });
                }

                self = selfRef;

                await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelOpenAfter
                {
                    UIPkgName = child.PkgName,
                    UIResName = child.ResName,
                    UIComponentName = child.Name,
                    StackOption = true,
                    PanelLayer = child.PanelLayer,
                });

                if (isBreak)
                {
                    break;
                }
            }
        }

        internal static async Promise<bool> RemoveUIToHome(this UIMgrComponent self, PanelInfo home, bool tween = true)
        {
            if (home.UIPanel is not { Layer: EPanelLayer.Panel })
            {
                return false; //home的UI必须在panel层
            }

            var layerList = self.GetLayerPanelInfoList(EPanelLayer.Panel);
            var skipOtherCloseTween = home.UIWindow.WindowSkipOtherCloseTween;
            var skipHomeOpenTween = home.UIWindow.WindowSkipHomeOpenTween;
            var skipHomeBack = home.UIWindow.WindowSkipHomeBack;

            EntityRef<UIMgrComponent> selfRef = self;

            for (var i = layerList.Count - 1; i >= 0; i--)
            {
                var child = layerList[i];
                if (child == null)
                {
                    continue;
                }

                if (child != home)
                {
                    self = selfRef;

                    await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelCloseBefore
                    {
                        UIPkgName = child.PkgName,
                        UIResName = child.ResName,
                        UIComponentName = child.Name,
                        StackOption = true,
                        PanelLayer = child.PanelLayer,
                    });

                    if (!skipHomeBack && child.OwnerUIEntity is IUIBackClose)
                    {
                        await UIEventSystem.BackClose(child.OwnerUIEntity, new UIEventPanelInfo
                        {
                            UIPkgName = home.PkgName,
                            UIResName = home.ResName,
                            UIComponentName = home.Name,
                            PanelLayer = home.PanelLayer,
                        });
                    }

                    if (child.OwnerUIEntity is IUIBackHomeClose)
                    {
                        await UIEventSystem.BackHomeClose(child.OwnerUIEntity, new UIEventPanelInfo
                        {
                            UIPkgName = home.PkgName,
                            UIResName = home.ResName,
                            UIComponentName = home.Name,
                            PanelLayer = home.PanelLayer,
                        });
                    }

                    self = selfRef;

                    var uiPkgName = child.PkgName;
                    var uiResName = child.ResName;
                    var uiComponentName = child.Name;
                    var panelLayer = child.PanelLayer;

                    var success = await self.ClosePanelAsync(child.Name, !skipOtherCloseTween, true, true);
                    if (!success)
                    {
                        return false;
                    }

                    self = selfRef;

                    await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelCloseAfter
                    {
                        UIPkgName = uiPkgName,
                        UIResName = uiResName,
                        UIComponentName = uiComponentName,
                        StackOption = true,
                        PanelLayer = panelLayer,
                    });

                    continue;
                }

                await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelOpenBefore
                {
                    UIPkgName = child.PkgName,
                    UIResName = child.ResName,
                    UIComponentName = child.Name,
                    StackOption = true,
                    PanelLayer = child.PanelLayer,
                });

                var childPanel = child.UIPanel;
                if (childPanel == null)
                {
                    Log.Error($"错误,是否在异步过程中删除了对象");
                    continue;
                }

                switch (childPanel.StackOption)
                {
                    case EPanelStackOption.Omit:
                    case EPanelStackOption.None:
                    case EPanelStackOption.Visible:
                        child.UIBase.SetActive(true);
                        break;
                    case EPanelStackOption.VisibleTween:
                        if (tween && !skipHomeOpenTween)
                        {
                            var childWindow = child.UIWindow;
                            if (childWindow == null)
                            {
                                Log.Error($"错误,是否在异步过程中删除了对象");
                            }
                            else
                            {
                                await childWindow.InternalOnWindowOpenTween();
                            }

                            childPanel = child.UIPanel;
                            if (childPanel == null)
                            {
                                Log.Error($"错误,是否在异步过程中删除了对象");
                            }
                            else
                            {
                                await childPanel.OpenAllViewTween();
                            }
                        }
                        else
                        {
                            child.UIBase.SetActive(true);
                            await childPanel.OpenAllViewTween(false);
                        }

                        break;
                    default:
                        Debug.LogError($"新增类型未实现 {childPanel.StackOption}");
                        child.UIBase.SetActive(true);
                        break;
                }

                if (!skipHomeBack && child.OwnerUIEntity is IUIBackOpen)
                {
                    await UIEventSystem.BackOpen(child.OwnerUIEntity, new UIEventPanelInfo
                    {
                        UIPkgName = child.PkgName,
                        UIResName = child.ResName,
                        UIComponentName = child.Name,
                        PanelLayer = child.PanelLayer,
                    });
                }

                if (child.OwnerUIEntity is IUIBackHomeOpen)
                {
                    await UIEventSystem.BackHomeOpen(child.OwnerUIEntity);
                }

                self = selfRef;

                await EventSystem.Instance?.PublishAsync(self.Root(), new UIEventPanelOpenAfter
                {
                    UIPkgName = child.PkgName,
                    UIResName = child.ResName,
                    UIComponentName = child.Name,
                    StackOption = true,
                    PanelLayer = child.PanelLayer,
                });
            }

            return true;
        }
    }
}