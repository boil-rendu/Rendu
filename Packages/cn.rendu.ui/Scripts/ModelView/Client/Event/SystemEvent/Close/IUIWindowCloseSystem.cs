using System;
using RD.Core;

namespace RD.Client
{
    public interface IUIWindowClose
    {
    }

    public interface IUIWindowCloseSystem : ISystemType
    {
        Promise Run(Entity self, bool viewCloseResult);
    }

    [EntitySystem]
    public abstract class UIWindowCloseSystem<T, _> : SystemObject, IUIWindowCloseSystem
            where T : Entity, IUIWindowClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIWindowCloseSystem);
        }

        async Promise IUIWindowCloseSystem.Run(Entity self, bool viewCloseResult)
        {
            await this.UIWindowClose((T)self, viewCloseResult);
        }

        protected abstract Promise UIWindowClose(T self, bool viewCloseResult);
    }
}