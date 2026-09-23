using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 打开动画结束消息
    /// </summary>
    public static partial class UIEventSystem
    {
        public static void OpenTweenEnd(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIOpenTweenEndSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIOpenTweenEndSystem));
            if (iUIOpenTweenEndSystems == null)
            {
                return;
            }

            foreach (IUIOpenTweenEndSystem aUIOpenTweenEndSystem in iUIOpenTweenEndSystems)
            {
                if (aUIOpenTweenEndSystem == null)
                {
                    continue;
                }

                try
                {
                    aUIOpenTweenEndSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}