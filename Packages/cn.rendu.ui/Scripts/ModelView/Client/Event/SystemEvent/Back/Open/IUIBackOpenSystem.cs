using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「返回时打开」能力</summary>
    public interface IUIBackOpen
    {
    }

    /// <summary>返回时打开 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIBackOpenSystem : ISystemType
    {
        Promise Run(Entity o, UIEventPanelInfo closePanelInfo);
    }

    [EntitySystem]
    /// <summary>返回时打开 系统抽象基类（EntitySystem 自动注册，子类实现 UIBackOpen）</summary>
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
