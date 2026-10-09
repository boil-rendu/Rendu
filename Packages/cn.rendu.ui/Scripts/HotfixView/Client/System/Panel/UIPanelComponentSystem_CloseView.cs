using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIPanelComponent 扩展（关闭 Panel 下的子 View）</summary>
    public static partial class UIPanelComponentSystem
    {
        /// <summary>关闭子 View（按泛型类型，发起后不等）</summary>
        public static void CloseView<T>(this UIPanelComponent self, bool tween = true) where T : Entity
        {
            self.CloseViewAsync<T>(tween).NoContext();
        }

        /// <summary>关闭子 View（按资源名，发起后不等）</summary>
        public static void CloseView(this UIPanelComponent self, string resName, bool tween = true)
        {
            self.CloseViewAsync(resName, tween).NoContext();
        }

        /// <summary>异步关闭子 View（按泛型类型）：不存在则直接返回 false</summary>
        public static async Promise<bool> CloseViewAsync<T>(this UIPanelComponent self, bool tween = true) where T : Entity
        {
            var (exist, entity) = self.ExistView<T>();
            if (!exist) return false;
            return await CloseViewAsync(entity, tween);
        }

        /// <summary>异步关闭子 View（按资源名）：不存在则直接返回 false</summary>
        public static async Promise<bool> CloseViewAsync(this UIPanelComponent self, string resName, bool tween = true)
        {
            var (exist, entity) = self.ExistView(resName);
            if (!exist) return false;
            return await CloseViewAsync(entity, tween);
        }

        /// <summary>异步关闭子 View 的实际实现：由实体向上取 UIViewComponent 后调 CloseAsync</summary>
        private static async Promise<bool> CloseViewAsync(Entity entity, bool tween)
        {
            var uiBase = entity.GetParent<UIChild>();
            if (uiBase == null) return false;

            var viewComponent = uiBase.GetComponent<UIViewComponent>(true);
            if (viewComponent == null) return false;

            return await viewComponent.CloseAsync(tween);
        }
    }
}