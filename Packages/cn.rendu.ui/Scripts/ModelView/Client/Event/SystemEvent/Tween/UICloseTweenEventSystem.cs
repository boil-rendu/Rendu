using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 关闭动画消息
    /// </summary>
    public static partial class UIEventSystem
    {
        public static async Promise CloseTween(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUICloseTweenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUICloseTweenSystem));
            if (iUICloseTweenSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUICloseTweenSystem aUICloseTweenSystem in iUICloseTweenSystems)
            {
                if (aUICloseTweenSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUICloseTweenSystem.Run(componentRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}