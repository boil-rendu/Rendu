using RD.Core;
namespace RD.Client
{
    /// <summary>UIViewComponent 扩展（按标准层级结构向上取 Panel / Window 相关组件）</summary>
    public static partial class UIViewComponentSystem
    {
        //标准view(Panel下的view)(非独立View)
        //可快捷获取panel实例 (不推荐使用)!!!
        //这里只是告诉你有这么个方式
        /// <summary>向上取 Panel 实体本身（不推荐直接使用，仅提供此途径）</summary>
        public static T GetPanel<T>(this UIViewComponent self) where T : IUIOpen
        {
            if (self?.Parent?.Parent is T panel)
            {
                return panel;
            }

            Log.Error($"获取失败 {self.GetType().Name} 没有找到父级 {typeof(T).Name} 请检查结构");
            return default;
        }

        //同上 需要标准View
        //这里拿到的不是panel 而是panel同级的UIPanelComponent
        /// <summary>向上取与 Panel 同级的 UIPanelComponent</summary>
        public static UIPanelComponent GetPanelComponent(this UIViewComponent self)
        {
            if (self?.Parent?.Parent?.Parent is UIChild uiBase)
            {
                return uiBase.GetComponent<UIPanelComponent>();
            }

            Log.Error($"获取失败 {self.GetType().Name} 没有找到 UIPanelComponent 请检查结构");
            return default;
        }

        //同上 需要标准View
        //这里拿到的不是panel 而是panel同级的UIWindowComponent
        /// <summary>向上取与 Panel 同级的 UIWindowComponent</summary>
        public static UIWindowComponent GetWindowComponent(this UIViewComponent self)
        {
            if (self?.Parent?.Parent?.Parent is UIChild uiBase)
            {
                return uiBase.GetComponent<UIWindowComponent>();
            }

            Log.Error($"获取失败 {self.GetType().Name} 没有找到 UIWindowComponent 请检查结构");
            return default;
        }
    }
}