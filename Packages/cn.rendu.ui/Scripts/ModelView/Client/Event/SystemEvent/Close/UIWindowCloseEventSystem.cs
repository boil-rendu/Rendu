using System;
using System.Collections.Generic;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 你可以理解为OOP中的 子类重写Close方法 先触发base.Close()
        /// 然后触发现在的IUIWindowClose事件
        /// 切记如果base.Close()中吧自己给摧毁了，那么这里就不会触发了
        /// 一个UIWindowComponent 可以重写多个ViewClose 但是无法保证先后顺序
        /// 实现方式为 在对应UIWindowComponent中 AddComponent你的重写组件 然后实现IUIWindowClose接口
        /// 参考 TipsViewComponent
        /// </summary>
        /// <param name="component">实体对应的那个具体UI</param>
        /// <param name="viewCloseResult">base.Close()的返回值</param>
        public static async Promise WindowClose(Entity component, bool viewCloseResult)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var windowComponent = component.GetParent<UIChild>()?.GetComponent<UIWindowComponent>();
            if (windowComponent != null)
            {
                foreach (var view in windowComponent.Components.Values)
                {
                    await WindowCloseSystem(view, viewCloseResult);
                }
            }
        }

        public static async Promise WindowClose(UIWindowComponent windowComponent, bool viewCloseResult)
        {
            if (windowComponent != null)
            {
                foreach (var view in windowComponent.Components.Values)
                {
                    await WindowCloseSystem(view, viewCloseResult);
                }
            }
        }

        private static async Promise WindowCloseSystem(Entity component, bool viewCloseResult)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            if (component is not IUIWindowClose)
            {
                return;
            }

            var iUICloseSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIWindowCloseSystem));
            if (iUICloseSystems is not { Count: > 0 }) return;

            EntityRef<Entity> componentRef = component;
            foreach (IUIWindowCloseSystem aUICloseSystem in iUICloseSystems)
            {
                if (aUICloseSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUICloseSystem.Run(componentRef, viewCloseResult);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}