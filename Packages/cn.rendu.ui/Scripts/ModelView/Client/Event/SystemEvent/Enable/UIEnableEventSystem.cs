using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 激活可用事件
    /// </summary>
    public static partial class UIEventSystem
    {
        public static void Enable(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIEnableSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIEnableSystem));
            if (iUIEnableSystems == null)
            {
                return;
            }

            foreach (IUIEnableSystem aUIEnableSystem in iUIEnableSystems)
            {
                if (aUIEnableSystem == null)
                {
                    continue;
                }

                try
                {
                    aUIEnableSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
