using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    public static partial class UIPanelComponentSystem
    {
        public static void CloseView<T>(this UIPanelComponent self, bool tween = true) where T : Entity
        {
            self.CloseViewAsync<T>(tween).NoContext();
        }

        public static void CloseView(this UIPanelComponent self, string resName, bool tween = true)
        {
            self.CloseViewAsync(resName, tween).NoContext();
        }

        public static async Promise<bool> CloseViewAsync<T>(this UIPanelComponent self, bool tween = true) where T : Entity
        {
            var (exist, entity) = self.ExistView<T>();
            if (!exist) return false;
            return await CloseViewAsync(entity, tween);
        }

        public static async Promise<bool> CloseViewAsync(this UIPanelComponent self, string resName, bool tween = true)
        {
            var (exist, entity) = self.ExistView(resName);
            if (!exist) return false;
            return await CloseViewAsync(entity, tween);
        }

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