using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「关闭补间」能力</summary>
    public interface IUICloseTween
    {
    }

    /// <summary>关闭补间 系统契约：由框架按实体类型统一调度</summary>
    public interface IUICloseTweenSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    /// <summary>关闭补间 系统抽象基类（EntitySystem 自动注册，子类实现 UICloseTween）</summary>
    public abstract class UICloseTweenSystem<T> : SystemObject, IUICloseTweenSystem where T : Entity, IUICloseTween
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUICloseTweenSystem);
        }

        async Promise IUICloseTweenSystem.Run(Entity o)
        {
            await this.UICloseTween((T)o);
        }

        protected abstract Promise UICloseTween(T self);
    }
}
