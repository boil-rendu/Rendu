using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 打开动画消息
    /// </summary>
    public static partial class UIEventSystem
    {
        public static async Promise OpenTween(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIOpenTweenSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIOpenTweenSystem));
            if (iUIOpenTweenSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIOpenTweenSystem aUIOpenTweenSystem in iUIOpenTweenSystems)
            {
                if (aUIOpenTweenSystem == null)
                {
                    continue;
                }

                try
                {
                    await aUIOpenTweenSystem.Run(componentRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}