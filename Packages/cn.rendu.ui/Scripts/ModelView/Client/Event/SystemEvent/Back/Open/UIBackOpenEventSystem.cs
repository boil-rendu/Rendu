using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 触发堆栈时
        /// 被添加触发 (有其他界面关闭 当前界面被打开)
        /// 自己被打开
        /// panelInfo = 是哪个界面被关闭了 那个界面的一些信息
        /// </summary>
        public static async Promise BackOpen(Entity component, UIEventPanelInfo closePanelInfo)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIBackOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIBackOpenSystem));
            if (iUIBackOpenSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIBackOpenSystem aUIBackOpenSystem in iUIBackOpenSystems)
            {
                if (aUIBackOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUIBackOpenSystem.Run(componentRef, closePanelInfo);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}