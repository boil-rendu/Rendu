using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIBackHomeClose
    {
    }

    public interface IUIBackHomeCloseSystem : ISystemType
    {
        Promise Run(Entity o, UIEventPanelInfo HomeClosePanelInfo);
    }

    [EntitySystem]
    public abstract class UIBackHomeCloseSystem<T, _> : SystemObject, IUIBackHomeCloseSystem where T : Entity, IUIBackHomeClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBackHomeCloseSystem);
        }

        async Promise IUIBackHomeCloseSystem.Run(Entity o, UIEventPanelInfo HomeClosePanelInfo)
        {
            await this.UIBackHomeClose((T)o, HomeClosePanelInfo);
        }

        protected abstract Promise UIBackHomeClose(T self, UIEventPanelInfo HomeClosePanelInfo);
    }
}
