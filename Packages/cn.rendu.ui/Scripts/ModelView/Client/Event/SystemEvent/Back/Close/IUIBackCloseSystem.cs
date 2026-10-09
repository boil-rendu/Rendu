using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「返回时关闭」能力</summary>
    public interface IUIBackClose
    {
    }

    /// <summary>返回时关闭 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIBackCloseSystem : ISystemType
    {
        Promise Run(Entity o, UIEventPanelInfo addPanelInfo);
    }

    [EntitySystem]
    /// <summary>返回时关闭 系统抽象基类（EntitySystem 自动注册，子类实现 UIBackClose）</summary>
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
