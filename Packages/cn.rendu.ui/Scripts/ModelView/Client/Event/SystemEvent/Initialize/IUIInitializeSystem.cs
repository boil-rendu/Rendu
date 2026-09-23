using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIInitialize
    {
    }

    public interface IUIInitializeSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIInitializeSystem<T> : SystemObject, IUIInitializeSystem where T : Entity, IUIInitialize
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIInitializeSystem);
        }

        void IUIInitializeSystem.Run(Entity o)
        {
            this.UIInitialize((T)o);
        }

        protected abstract void UIInitialize(T self);
    }
}
