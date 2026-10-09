using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIMgrComponent 扩展（打开面板并等待其自行通知结束等待）</summary>
    public static partial class UIMgrComponentSystem
    {
        /// <summary>
        /// 等待面板结束等待（loading 遮罩之类）
        /// 生成一个 guid 同时注册到 Root 的 HashWait 和窗口的 UIWaitComponent，由面板调用 NotifyWait 兑现
        /// </summary>
        private static async Promise<EHashWaitError> PanelWait(this UIMgrComponent self, Entity panel)
        {
            if (panel == null) return EHashWaitError.Error;
            var guid = IdGenerator.Instance.GenerateId();
            var hashWait = self.Root.GetComponent<HashWait>().Wait(guid);
            var windowComponent = panel.GetParent<UIChild>().GetComponent<UIWindowComponent>();
            var waitComponent = windowComponent.GetComponent<UIWaitComponent>();
            if (waitComponent == null)
            {
                waitComponent = windowComponent.AddComponent<UIWaitComponent, long>(guid);
            }
            else
            {
                waitComponent.Reset(guid);
            }

            return await hashWait;
        }

        /// <summary>按泛型类型打开面板并等待（0 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T>(this UIMgrComponent self, Entity root)
                where T : Entity, IAwake, IUIOpen
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T>(root);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按泛型类型打开面板并等待（变长参数版，params object[] → ParamVo）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitParamAsync<T>(this UIMgrComponent self, Entity root, params object[] paramMore)
                where T : Entity, IUIOpen<ParamVo>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelParamAsync<T>(root, paramMore);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按泛型类型打开面板并等待（1 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1>(this UIMgrComponent self, Entity root, P1 p1)
                where T : Entity, IUIOpen<P1>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1>(root, p1);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按泛型类型打开面板并等待（2 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2>(this UIMgrComponent self, Entity root, P1 p1, P2 p2)
                where T : Entity, IUIOpen<P1, P2>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2>(root, p1, p2);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按泛型类型打开面板并等待（3 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3>(this UIMgrComponent self, Entity root, P1 p1, P2 p2, P3 p3)
                where T : Entity, IUIOpen<P1, P2, P3>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2, P3>(root, p1, p2, p3);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按泛型类型打开面板并等待（4 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3, P4>(this UIMgrComponent self, Entity root, P1 p1, P2 p2, P3 p3, P4 p4)
                where T : Entity, IUIOpen<P1, P2, P3, P4>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2, P3, P4>(root, p1, p2, p3, p4);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按泛型类型打开面板并等待（5 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3, P4, P5>(this UIMgrComponent self, Entity root, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
                where T : Entity, IUIOpen<P1, P2, P3, P4, P5>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2, P3, P4, P5>(root, p1, p2, p3, p4, p5);
            self = selfRef;
            return await self.PanelWait(panel);
        }
    }
}