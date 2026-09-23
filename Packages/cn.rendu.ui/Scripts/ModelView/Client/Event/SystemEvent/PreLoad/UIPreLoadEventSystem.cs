using System;
using RD.Core;

namespace RD.Client
{
    public static partial class UIEventSystem
    {
        public static async Promise PreLoad(Entity component)
        {
            var iPreLoadSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof(IUIPreLoadSystem));
            if (iPreLoadSystems == null)
            {
                return;
            }

            EntityRef<Entity> componentRef = component;
            foreach (IUIPreLoadSystem aPreLoadSystem in iPreLoadSystems)
            {
                if (aPreLoadSystem == null)
                {
                    continue;
                }

                try
                {
                    await aPreLoadSystem.Run(componentRef);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}