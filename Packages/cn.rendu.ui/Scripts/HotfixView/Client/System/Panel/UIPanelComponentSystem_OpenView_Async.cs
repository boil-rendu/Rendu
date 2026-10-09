using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIPanelComponent 扩展（按泛型类型打开子 View）
    /// 各重载结构一致：取 View → OpenViewBefore 前置 → Open（按参数个数调对应的 IUIOpen 重载）
    /// → 再取一次 self（await 期间可能被销毁）→ OpenViewAfter 后置
    /// 用 EntityRef 保活，await 后重新赋值 self/viewComponent 以防访问已销毁对象
    /// </summary>
    public static partial class UIPanelComponentSystem
    {
        /// <summary>异步打开子 View（0 参）</summary>
        public static async Promise<T> OpenViewAsync<T>(this UIPanelComponent self) where T : Entity
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            try
            {
                success = await viewComponent.Entity.Open();
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            return (T)view.Entity;
        }

        /// <summary>异步打开子 View（变长参数版，params object[] → ParamVo）</summary>
        public static async Promise<T> OpenViewParamAsync<T>(this UIPanelComponent self, params object[] paramMore) where T : Entity, IUIOpen<ParamVo>
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            var p = ParamVo.Get(paramMore);

            try
            {
                success = await viewComponent.Entity.Open(p);
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            ParamVo.Put(p);

            return (T)view.Entity;
        }

        /// <summary>异步打开子 View（1 参）</summary>
        public static async Promise<T> OpenViewAsync<T, P1>(this UIPanelComponent self, P1 p1) where T : Entity, IUIOpen<P1>
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            try
            {
                success = await viewComponent.Entity.Open(p1);
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            return (T)view.Entity;
        }

        /// <summary>异步打开子 View（2 参）</summary>
        public static async Promise<T> OpenViewAsync<T, P1, P2>(this UIPanelComponent self, P1 p1, P2 p2) where T : Entity, IUIOpen<P1, P2>
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            try
            {
                success = await viewComponent.Entity.Open(p1, p2);
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            return (T)view.Entity;
        }

        /// <summary>异步打开子 View（3 参）</summary>
        public static async Promise<T> OpenViewAsync<T, P1, P2, P3>(this UIPanelComponent self, P1 p1, P2 p2, P3 p3) where T : Entity, IUIOpen<P1, P2, P3>
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            try
            {
                success = await viewComponent.Entity.Open(p1, p2, p3);
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            return (T)view.Entity;
        }

        /// <summary>异步打开子 View（4 参）</summary>
        public static async Promise<T> OpenViewAsync<T, P1, P2, P3, P4>(this UIPanelComponent self, P1 p1, P2 p2, P3 p3, P4 p4) where T : Entity, IUIOpen<P1, P2, P3, P4>
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            try
            {
                success = await viewComponent.Entity.Open(p1, p2, p3, p4);
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            return (T)view.Entity;
        }

        /// <summary>异步打开子 View（5 参）</summary>
        public static async Promise<T> OpenViewAsync<T, P1, P2, P3, P4, P5>(this UIPanelComponent self, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5) where T : Entity, IUIOpen<P1, P2, P3, P4, P5>
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView<T>();
            if (view.Entity == null) return default;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
            self = selfRef;
            await self.OpenViewBefore(view);

            try
            {
                success = await viewComponent.Entity.Open(p1, p2, p3, p4, p5);
            }
            catch (Exception e)
            {
                Debug.LogError($"err={e.Message}{e.StackTrace}");
            }

            self = selfRef;

            await self.OpenViewAfter(view, success);

            return (T)view.Entity;
        }
    }
}