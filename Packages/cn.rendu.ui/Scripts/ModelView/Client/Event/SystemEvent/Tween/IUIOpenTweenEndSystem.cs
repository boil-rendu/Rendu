using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIOpenTweenEnd
    {
    }

    public interface IUIOpenTweenEndSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIOpenTweenEndSystem<T> : SystemObject, IUIOpenTweenEndSystem where T : Entity, IUIOpenTweenEnd
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenTweenEndSystem);
        }

        void IUIOpenTweenEndSystem.Run(Entity o)
        {
            this.UIOpenTweenEnd((T)o);
        }

        protected abstract void UIOpenTweenEnd(T self);
    }
}
