using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「打开补间结束」能力</summary>
    public interface IUIOpenTweenEnd
    {
    }

    /// <summary>打开补间结束 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIOpenTweenEndSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    /// <summary>打开补间结束 系统抽象基类（EntitySystem 自动注册，子类实现 UIOpenTweenEnd）</summary>
    public abstract class UIOpenTweenEndSystem<T> : SystemObject, IUIOpenTweenEndSystem where T : Entity, IUIOpenTweenEnd
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIOpenTweenEndSystem);
        }

        void IUIOpenTweenEndSystem.Run(Entity o)
        {
            this.UIOpenTweenEnd((T)o);
        }

        protected abstract void UIOpenTweenEnd(T self);
    }
}
