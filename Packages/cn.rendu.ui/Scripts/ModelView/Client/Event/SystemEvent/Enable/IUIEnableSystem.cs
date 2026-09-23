using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIEnable
    {
    }

    public interface IUIEnableSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIEnableSystem<T> : SystemObject, IUIEnableSystem where T : Entity, IUIEnable
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIEnableSystem);
        }

        void IUIEnableSystem.Run(Entity o)
        {
            this.UIEnable((T)o);
        }

        protected abstract void UIEnable(T self);
    }
}
