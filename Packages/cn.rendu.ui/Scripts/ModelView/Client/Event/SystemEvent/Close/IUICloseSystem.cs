using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIClose
    {
    }

    public interface IUICloseSystem : ISystemType
    {
        Promise<bool> Run(Entity o);
    }

    [EntitySystem]
    public abstract class UICloseSystem<T> : SystemObject, IUICloseSystem where T : Entity, IUIClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUICloseSystem);
        }

        async Promise<bool> IUICloseSystem.Run(Entity o)
        {
            return await this.UIClose((T)o);
        }

        protected abstract Promise<bool> UIClose(T self);
    }
}
