using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIPanelComponent 扩展（按资源名打开子 View）
    /// 与按泛型类型的版本逻辑一致，只是定位 View 用 resName（可用自动生成的枚举转字符串）
    /// </summary>
    public static partial class UIPanelComponentSystem
    {
        //这里传入的是ResName 不要传错
        //可以用自动生成的枚举转字符串
        /// <summary>异步打开子 View（0 参，按 resName）</summary>
        public static async Promise<Entity> OpenViewAsync(this UIPanelComponent self, string resName)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }

        /// <summary>异步打开子 View（变长参数版，按 resName）</summary>
        public static async Promise<Entity> OpenViewParamAsync(this UIPanelComponent self, string resName, params object[] paramMore)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }

        /// <summary>异步打开子 View（1 参，按 resName）</summary>
        public static async Promise<Entity> OpenViewAsync<P1>(this UIPanelComponent self, string resName, P1 p1)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }

        /// <summary>异步打开子 View（2 参，按 resName）</summary>
        public static async Promise<Entity> OpenViewAsync<P1, P2>(this UIPanelComponent self, string resName, P1 p1, P2 p2)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }

        /// <summary>异步打开子 View（3 参，按 resName）</summary>
        public static async Promise<Entity> OpenViewAsync<P1, P2, P3>(this UIPanelComponent self, string resName, P1 p1, P2 p2, P3 p3)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }

        /// <summary>异步打开子 View（4 参，按 resName）</summary>
        public static async Promise<Entity> OpenViewAsync<P1, P2, P3, P4>(this UIPanelComponent self, string resName, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }

        /// <summary>异步打开子 View（5 参，按 resName）</summary>
        public static async Promise<Entity> OpenViewAsync<P1, P2, P3, P4, P5>(this UIPanelComponent self, string resName, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            EntityRef<UIPanelComponent> selfRef = self;
            EntityRef<Entity> view = await self.GetView(resName);
            if (view.Entity == null) return default;
            self = selfRef;
            var success = false;
            EntityRef<UIViewComponent> viewComponent = view.Entity.GetParent<UIChild>().GetComponent<UIViewComponent>();
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

            return view;
        }
    }
}