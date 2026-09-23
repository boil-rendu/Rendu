using System;
using RD.Core;

namespace RD.Client
{
    public interface IUICloseTween
    {
    }

    public interface IUICloseTweenSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    public abstract class UICloseTweenSystem<T> : SystemObject, IUICloseTweenSystem where T : Entity, IUICloseTween
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUICloseTweenSystem);
        }

        async Promise IUICloseTweenSystem.Run(Entity o)
        {
            await this.UICloseTween((T)o);
        }

        protected abstract Promise UICloseTween(T self);
    }
}
