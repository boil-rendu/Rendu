using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「返回主页时打开」能力</summary>
    public interface IUIBackHomeOpen
    {
    }

    /// <summary>返回主页时打开 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIBackHomeOpenSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    /// <summary>返回主页时打开 系统抽象基类（EntitySystem 自动注册，子类实现 UIBackHomeOpen）</summary>
    public abstract class UIBackHomeOpenSystem<T> : SystemObject, IUIBackHomeOpenSystem where T : Entity, IUIBackHomeOpen
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBackHomeOpenSystem);
        }

        async Promise IUIBackHomeOpenSystem.Run(Entity o)
        {
            await this.UIBackHomeOpen((T)o);
        }

        protected abstract Promise UIBackHomeOpen(T self);
    }
}
