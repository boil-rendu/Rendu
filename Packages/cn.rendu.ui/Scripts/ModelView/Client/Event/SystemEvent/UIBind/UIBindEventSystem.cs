using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 绑定事件 也可以理解为UI的初始化前
    /// </summary>
    public static partial class UIEventSystem
    {
        public static void Bind(Entity component)
        {
            if (component == null || component.IsDisposed)
            {
                return;
            }

            var iUIBindSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIBindSystem));
            if (iUIBindSystems == null)
            {
                return;
            }

            foreach (IUIBindSystem aUIBindSystem in iUIBindSystems)
            {
                if (aUIBindSystem == null)
                {
                    continue;
                }

                try
                {
                    aUIBindSystem.Run(component);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}
