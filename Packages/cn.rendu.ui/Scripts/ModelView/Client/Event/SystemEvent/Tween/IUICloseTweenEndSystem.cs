using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「关闭补间结束」能力</summary>
    public interface IUICloseTweenEnd
    {
    }

    /// <summary>关闭补间结束 系统契约：由框架按实体类型统一调度</summary>
    public interface IUICloseTweenEndSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    /// <summary>关闭补间结束 系统抽象基类（EntitySystem 自动注册，子类实现 UICloseTweenEnd）</summary>
    public abstract class UICloseTweenEndSystem<T> : SystemObject, IUICloseTweenEndSystem where T : Entity, IUICloseTweenEnd
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUICloseTweenEndSystem);
        }

        void IUICloseTweenEndSystem.Run(Entity o)
        {
            this.UICloseTweenEnd((T)o);
        }

        protected abstract void UICloseTweenEnd(T self);
    }
}