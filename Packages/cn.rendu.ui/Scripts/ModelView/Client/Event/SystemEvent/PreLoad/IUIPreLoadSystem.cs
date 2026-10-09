using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「预加载」能力</summary>
    public interface IUIPreLoad
    {
    }

    /// <summary>预加载 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIPreLoadSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    /// <summary>预加载 系统抽象基类（EntitySystem 自动注册，子类实现 UIPreLoad）</summary>
    public abstract class UIPreLoadSystem<T> : SystemObject, IUIPreLoadSystem where T : Entity, IUIPreLoad
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIPreLoadSystem);
        }

        async Promise IUIPreLoadSystem.Run(Entity o)
        {
            await this.UIPreLoad((T)o);
        }

        protected abstract Promise UIPreLoad(T self);
    }
}