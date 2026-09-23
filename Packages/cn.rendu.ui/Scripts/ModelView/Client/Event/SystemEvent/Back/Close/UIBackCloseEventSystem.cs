using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 触发堆栈时
        /// 被关闭触发 (有界面打开 当前界面被关闭)
        /// 自己被关闭
        /// panelInfo = 是哪个界面被打开了 那个界面的一些信息
        /// </summary>
        public static async Promise BackClose(Entity component, UIEventPanelInfo addPanelInfo)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIBackCloseSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIBackCloseSystem));
            if (iUIBackCloseSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIBackCloseSystem aUIBackCloseSystem in iUIBackCloseSystems)
            {
                if (aUIBackCloseSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUIBackCloseSystem.Run(componentRef, addPanelInfo);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}