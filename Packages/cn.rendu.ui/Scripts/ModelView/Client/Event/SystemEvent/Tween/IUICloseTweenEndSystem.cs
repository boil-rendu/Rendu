using System;
using RD.Core;

namespace RD.Client
{
    public interface IUICloseTweenEnd
    {
    }

    public interface IUICloseTweenEndSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class UICloseTweenEndSystem<T> : SystemObject, IUICloseTweenEndSystem where T : Entity, IUICloseTweenEnd
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUICloseTweenEndSystem);
        }

        void IUICloseTweenEndSystem.Run(Entity o)
        {
            this.UICloseTweenEnd((T)o);
        }

        protected abstract void UICloseTweenEnd(T self);
    }
}