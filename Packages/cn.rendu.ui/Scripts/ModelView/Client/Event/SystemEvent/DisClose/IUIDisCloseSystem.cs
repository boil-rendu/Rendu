using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIDisClose
    {
    }

    public interface IUIDisCloseSystem : ISystemType
    {
        Promise<bool> Run(Entity o);
    }

    [EntitySystem]
    public abstract class UIDisCloseSystem<T> : SystemObject, IUIDisCloseSystem where T : Entity, IUIDisClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIDisCloseSystem);
        }

        async Promise<bool> IUIDisCloseSystem.Run(Entity o)
        {
            return await this.UIDisClose((T)o);
        }

        protected abstract Promise<bool> UIDisClose(T self);
    }
}
