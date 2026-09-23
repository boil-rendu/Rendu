using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIDisable
    {
    }

    public interface IUIDisableSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIDisableSystem<T> : SystemObject, IUIDisableSystem where T : Entity, IUIDisable
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIDisableSystem);
        }

        void IUIDisableSystem.Run(Entity o)
        {
            this.UIDisable((T)o);
        }

        protected abstract void UIDisable(T self);
    }
}
