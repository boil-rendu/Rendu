using System;
using UnityEngine;
using RD.Client;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// UIMgrComponent 扩展（按名称打开面板）
    /// 各重载结构一致：空名校验 → 查 PanelInfo → 协程锁（Panel 层用项目名哈希、其余用组件名哈希）
    /// → OpenPanelStartAsync 创建 → OpenPanelBefore → info.UIPanel.Open(参数)（异常吞掉记日志，success=false）
    /// → OpenPanelAfter，成功则返回面板实体、失败返回 null
    /// 全程 EntityRef 保活并在每次 await 后重置 self
    /// </summary>
    public static partial class UIMgrComponentSystem
    {
        /// <summary>按名称打开面板（0 参）</summary>
        internal static async Promise<Entity> OpenPanelAsync(this UIMgrComponent self, string componentName, Entity root)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);
            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open();
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            return success ? component : null;
        }

        /// <summary>按名称打开面板（变长参数版，params object[] → ParamVo，用后归还）</summary>
        internal static async Promise<Entity> OpenPanelParamAsync(this UIMgrComponent self, string componentName, Entity root, params object[] paramMore)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);

            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);
            var p = ParamVo.Get(paramMore);

            try
            {
                success = await info.UIPanel.Open(p);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            ParamVo.Put(p);
            return success ? component : null;
        }

        /// <summary>按名称打开面板（1 参）</summary>
        internal static async Promise<Entity> OpenPanelAsync<P1>(this UIMgrComponent self, string componentName, Entity root, P1 p1)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);

            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            return success ? component : null;
        }

        /// <summary>按名称打开面板（2 参）</summary>
        internal static async Promise<Entity> OpenPanelAsync<P1, P2>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);

            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            return success ? component : null;
        }

        /// <summary>按名称打开面板（3 参）</summary>
        internal static async Promise<Entity> OpenPanelAsync<P1, P2, P3>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2, P3 p3)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);

            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2, p3);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            return success ? component : null;
        }

        /// <summary>按名称打开面板（4 参）</summary>
        internal static async Promise<Entity> OpenPanelAsync<P1, P2, P3, P4>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2, P3 p3, P4 p4)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);

            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2, p3, p4);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            return success ? component : null;
        }

        /// <summary>按名称打开面板（5 参）</summary>
        internal static async Promise<Entity> OpenPanelAsync<P1, P2, P3, P4, P5>(this UIMgrComponent self, string componentName, Entity root, P1 p1, P2 p2, P3 p3, P4 p4, P5 p5)
        {
            if (string.IsNullOrEmpty(componentName))
            {
                Debug.LogError($"<color=red> 无法打开 这是一个空名称 </color>");
                return null;
            }

            var info = self.GetPanelInfo(componentName);
            if (info == null) return default;

            EntityRef<UIMgrComponent> selfRef = self;
            EntityRef<Entity> rootRef = EntityRefHelper.GetEntityRefSafety(root);

            var coroutineLockCode = info.PanelLayer == EPanelLayer.Panel ? UIConstHelper.Const.ProjectName.GetHashCode() : componentName.GetHashCode();

            using var _ = await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UIPanel, coroutineLockCode);

            self = selfRef;
            var panel = await self.OpenPanelStartAsync(componentName, rootRef.Entity ?? self);
            if (panel == null) return default;

            var success = false;
            EntityRef<Entity> component = info.OwnerUIEntity;
            self = selfRef;
            await self.OpenPanelBefore(info);

            try
            {
                success = await info.UIPanel.Open(p1, p2, p3, p4, p5);
            }
            catch (Exception e)
            {
                Debug.LogError($"panel={info.ResName}, err={e.Message}{e.StackTrace}");
            }

            self = selfRef;
            await self.OpenPanelAfter(info, success);
            return success ? component : null;
        }
    }
}