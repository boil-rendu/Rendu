using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIOpenTween
    {
    }

    public interface IUIOpenTweenSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIOpenTweenSystem<T> : SystemObject, IUIOpenTweenSystem where T : Entity, IUIOpenTween
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenTweenSystem);
        }

        async Promise IUIOpenTweenSystem.Run(Entity o)
        {
            await this.UIOpenTween((T)o);
        }

        protected abstract Promise UIOpenTween(T self);
    }
}
