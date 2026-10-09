using RD.Core;
namespace RD.Client
{
    /// <summary>UIMgrComponent 扩展（查询 Panel / View 是否存在且处于显示状态）</summary>
    public static partial class UIMgrComponentSystem
    {
        #region 判断Panel是否存在且显示

        /// <summary>指定名称的 Panel 是否存在且显示</summary>
        public static bool ActiveSelf(this UIMgrComponent self, string panelName)
        {
            var info = self.GetPanelInfo(panelName);
            return info?.ActiveSelf ?? false;
        }

        /// <summary>指定类型的 Panel 是否存在且显示</summary>
        public static bool ActiveSelf<T>(this UIMgrComponent self) where T : Entity
        {
            var info = self.GetPanelInfo<T>();
            return info?.ActiveSelf ?? false;
        }

        #endregion

        #region 判断指定Panel的指定View是否存在且显示

        /// <summary>指定 Panel 下指定名称的 View 是否存在且显示（双重：Panel 名 + View 名）</summary>
        public static bool ActiveSelfView(this UIMgrComponent self, string panelName, string viewName)
        {
            var info = self.GetPanelInfo(panelName);
            if (info == null) return false;
            if (info.UIPanel == null) return false;
            var (exist, entity) = info.UIPanel.ExistView(viewName);
            if (!exist) return false;
            return entity.GetParent<UIChild>()?.ActiveSelf ?? false;
        }

        /// <summary>指定 Panel 类型下的指定 View 类型是否存在且显示</summary>
        public static bool ActiveSelfView<TPanel, TView>(this UIMgrComponent self)
                where TPanel : Entity
                where TView : Entity
        {
            var info = self.GetPanelInfo<TPanel>();
            if (info == null) return false;
            if (info.UIPanel == null) return false;
            var (exist, entity) = info.UIPanel.ExistView<TView>();
            if (!exist) return false;
            return entity.GetParent<UIChild>()?.ActiveSelf ?? false;
        }

        /// <summary>指定 Panel 类型下的指定 View 名称是否存在且显示</summary>
        public static bool ActiveSelfViewByViewName<TPanel>(this UIMgrComponent self, string viewName)
                where TPanel : Entity
        {
            var info = self.GetPanelInfo<TPanel>();
            if (info == null) return false;
            if (info.UIPanel == null) return false;
            var (exist, entity) = info.UIPanel.ExistView(viewName);
            if (!exist) return false;
            return entity.GetParent<UIChild>()?.ActiveSelf ?? false;
        }

        /// <summary>指定 Panel 名称下的指定 View 类型是否存在且显示</summary>
        public static bool ActiveSelfViewByPanelName<TView>(this UIMgrComponent self, string panelName)
                where TView : Entity
        {
            var info = self.GetPanelInfo(panelName);
            if (info == null) return false;
            if (info.UIPanel == null) return false;
            var (exist, entity) = info.UIPanel.ExistView<TView>();
            if (!exist) return false;
            return entity.GetParent<UIChild>()?.ActiveSelf ?? false;
        }

        #endregion
    }
}