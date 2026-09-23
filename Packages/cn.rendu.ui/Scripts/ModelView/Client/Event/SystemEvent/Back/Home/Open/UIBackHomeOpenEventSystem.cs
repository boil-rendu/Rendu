using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        /// <summary>
        /// 触发堆栈时
        /// HomeOpen触发 (当前界面home打开 其他全部关闭)
        /// 自己被打开
        /// </summary>
        public static async Promise BackHomeOpen(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIBackHomeOpenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIBackHomeOpenSystem));
            if (iUIBackHomeOpenSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIBackHomeOpenSystem aUIBackHomeOpenSystem in iUIBackHomeOpenSystems)
            {
                if (aUIBackHomeOpenSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUIBackHomeOpenSystem.Run(componentRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}