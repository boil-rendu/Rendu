using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIBackHomeOpen
    {
    }

    public interface IUIBackHomeOpenSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIBackHomeOpenSystem<T> : SystemObject, IUIBackHomeOpenSystem where T : Entity, IUIBackHomeOpen
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBackHomeOpenSystem);
        }

        async Promise IUIBackHomeOpenSystem.Run(Entity o)
        {
            await this.UIBackHomeOpen((T)o);
        }

        protected abstract Promise UIBackHomeOpen(T self);
    }
}
