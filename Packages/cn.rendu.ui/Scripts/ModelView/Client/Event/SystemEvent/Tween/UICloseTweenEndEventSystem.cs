using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 关闭动画结束消息
    /// </summary>
    public static partial class UIEventSystem
    {
        public static void CloseTweenEnd(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUICloseTweenEndSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUICloseTweenEndSystem));
            if (iUICloseTweenEndSystems == null)
            {
                return;
            }

            foreach (IUICloseTweenEndSystem aUICloseTweenEndSystem in iUICloseTweenEndSystems)
            {
                if (aUICloseTweenEndSystem == null)
                {
                    continue;
                }

                try
                {
                    aUICloseTweenEndSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}