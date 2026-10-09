using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「返回主页时关闭」能力</summary>
    public interface IUIBackHomeClose
    {
    }

    /// <summary>返回主页时关闭 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIBackHomeCloseSystem : ISystemType
    {
        Promise Run(Entity o, UIEventPanelInfo HomeClosePanelInfo);
    }

    [EntitySystem]
    /// <summary>返回主页时关闭 系统抽象基类（EntitySystem 自动注册，子类实现 UIBackHomeClose）</summary>
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
