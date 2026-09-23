using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIBackOpen
    {
    }

    public interface IUIBackOpenSystem : ISystemType
    {
        Promise Run(Entity o, UIEventPanelInfo closePanelInfo);
    }

    [EntitySystem]
    public abstract class UIBackOpenSystem<T, _> : SystemObject, IUIBackOpenSystem where T : Entity, IUIBackOpen
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBackOpenSystem);
        }

        async Promise IUIBackOpenSystem.Run(Entity o, UIEventPanelInfo closePanelInfo)
        {
            await this.UIBackOpen((T)o, closePanelInfo);
        }

        protected abstract Promise UIBackOpen(T self, UIEventPanelInfo closePanelInfo);
    }
}
