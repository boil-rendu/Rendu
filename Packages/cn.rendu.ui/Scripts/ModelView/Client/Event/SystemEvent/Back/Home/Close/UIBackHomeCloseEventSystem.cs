using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 触发堆栈时
        /// HomeClose触发 (有其他界面打开 当前界面被关闭)
        /// 自己被关闭
        /// panelInfo = 是哪个界面被打开了 那个界面的一些信息
        /// </summary>
        public static async Promise BackHomeClose(Entity component, UIEventPanelInfo HomeClosePanelInfo)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIBackHomeCloseSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIBackHomeCloseSystem));
            if (iUIBackHomeCloseSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIBackHomeCloseSystem aUIBackHomeCloseSystem in iUIBackHomeCloseSystems)
            {
                if (aUIBackHomeCloseSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUIBackHomeCloseSystem.Run(componentRef, HomeClosePanelInfo);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}