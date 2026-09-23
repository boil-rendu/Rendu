using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIPreLoad
    {
    }

    public interface IUIPreLoadSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIPreLoadSystem<T> : SystemObject, IUIPreLoadSystem where T : Entity, IUIPreLoad
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIPreLoadSystem);
        }

        async Promise IUIPreLoadSystem.Run(Entity o)
        {
            await this.UIPreLoad((T)o);
        }

        protected abstract Promise UIPreLoad(T self);
    }
}