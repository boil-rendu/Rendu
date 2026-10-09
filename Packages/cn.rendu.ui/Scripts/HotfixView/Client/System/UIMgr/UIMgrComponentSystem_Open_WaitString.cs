using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>UIMgrComponent 扩展（按名称打开面板并等待其通知结束等待）</summary>
    public static partial class UIMgrComponentSystem
    {
        /// <summary>按名称打开面板并等待（0 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync(this UIMgrComponent self, string componentName, Entity root)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync(componentName, root);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按名称打开面板并等待（变长参数版，params object[] → ParamVo）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitParamAsync(this UIMgrComponent self, string componentName, Entity root, params object[] paramMore)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelParamAsync(componentName, root, paramMore);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按名称打开面板并等待（1 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<P1>(this UIMgrComponent self, string componentName, Entity root, P1 p1)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync(componentName, root, p1);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按名称打开面板并等待（2 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync(componentName, root, p1, p2);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按名称打开面板并等待（3 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2, P3>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2, P3 p3)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync(componentName, root, p1, p2, p3);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按名称打开面板并等待（4 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2, P3, P4>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync(componentName, root, p1, p2, p3, p4);
            self = selfRef;
            return await self.PanelWait(panel);
        }

        /// <summary>按名称打开面板并等待（5 参）</summary>
        internal static async Promise<EHashWaitError> OpenPanelWaitAsync<P1, P2, P3, P4, P5>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            EntityRef<UIMgrComponent> selfRef = self;
            var panel = await self.OpenPanelAsync(componentName, root, p1, p2, p3, p4, p5);
            self = selfRef;
            return await self.PanelWait(panel);
        }
    }
}