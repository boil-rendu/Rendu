using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIBackClose
    {
    }

    public interface IUIBackCloseSystem : ISystemType
    {
        Promise Run(Entity o, UIEventPanelInfo addPanelInfo);
    }

    [EntitySystem]
    public abstract class UIBackCloseSystem<T, _> : SystemObject, IUIBackCloseSystem where T : Entity, IUIBackClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBackCloseSystem);
        }

        async Promise IUIBackCloseSystem.Run(Entity o, UIEventPanelInfo addPanelInfo)
        {
            await this.UIBackClose((T)o, addPanelInfo);
        }

        protected abstract Promise UIBackClose(T self, UIEventPanelInfo addPanelInfo);
    }
}
