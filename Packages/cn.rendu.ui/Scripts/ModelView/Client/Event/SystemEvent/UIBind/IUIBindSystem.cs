using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIBind
    {
    }

    public interface IUIBindSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIBindSystem<T> : SystemObject, IUIBindSystem where T : Entity, IUIBind
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBindSystem);
        }

        void IUIBindSystem.Run(Entity o)
        {
            this.UIBind((T)o);
        }

        protected abstract void UIBind(T self);
    }
}
