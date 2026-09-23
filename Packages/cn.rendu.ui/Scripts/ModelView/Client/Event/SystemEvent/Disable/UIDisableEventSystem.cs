using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 被隐藏时事件
    /// </summary>
    public static partial class UIEventSystem
    {
        public static void Disable(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIDisableSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIDisableSystem));
            if (iUIDisableSystems == null)
            {
                return;
            }

            foreach (IUIDisableSystem aUIDisableSystem in iUIDisableSystems)
            {
                if (aUIDisableSystem == null)
                {
                    continue;
                }

                try
                {
                    aUIDisableSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
