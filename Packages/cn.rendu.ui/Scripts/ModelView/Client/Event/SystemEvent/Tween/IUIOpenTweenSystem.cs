using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「打开补间」能力</summary>
    public interface IUIOpenTween
    {
    }

    /// <summary>打开补间 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIOpenTweenSystem : ISystemType
    {
        Promise Run(Entity o);
    }

    [EntitySystem]
    /// <summary>打开补间 系统抽象基类（EntitySystem 自动注册，子类实现 UIOpenTween）</summary>
    public abstract class UIOpenTweenSystem<T> : SystemObject, IUIOpenTweenSystem where T : Entity, IUIOpenTween
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenTweenSystem);
        }

        async Promise IUIOpenTweenSystem.Run(Entity o)
        {
            await this.UIOpenTween((T)o);
        }

        protected abstract Promise UIOpenTween(T self);
    }
}
