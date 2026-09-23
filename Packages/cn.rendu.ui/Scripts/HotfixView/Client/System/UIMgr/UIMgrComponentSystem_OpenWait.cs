using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    public static partial class UIMgrComponentSystem
    {
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

        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T>(this UIMgrComponent self, Entity root)
                where T : Entity, IAwake, IUIOpen
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T>(root);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        internal static async Promise<EHashWaitError> OpenPanelWaitParamAsync<T>(this UIMgrComponent self, Entity root, params object[] paramMore)
                where T : Entity, IUIOpen<ParamVo>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelParamAsync<T>(root, paramMore);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1>(this UIMgrComponent self, Entity root, P1 p1)
                where T : Entity, IUIOpen<P1>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1>(root, p1);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2>(this UIMgrComponent self, Entity root, P1 p1, P2 p2)
                where T : Entity, IUIOpen<P1, P2>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2>(root, p1, p2);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3>(this UIMgrComponent self, Entity root, P1 p1, P2 p2, P3 p3)
                where T : Entity, IUIOpen<P1, P2, P3>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2, P3>(root, p1, p2, p3);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<T, P1, P2, P3, P4>(this UIMgrComponent self, Entity root, P1 p1, P2 p2, P3 p3, P4 p4)
                where T : Entity, IUIOpen<P1, P2, P3, P4>
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync<T, P1, P2, P3, P4>(root, p1, p2, p3, p4);
            self = selfRef;
            return await self.PanelWait(panel);
        }

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