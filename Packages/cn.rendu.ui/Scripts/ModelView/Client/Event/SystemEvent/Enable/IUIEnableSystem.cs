using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「UI 启用」能力</summary>
    public interface IUIEnable
    {
    }

    /// <summary>UI 启用 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIEnableSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    /// <summary>UI 启用 系统抽象基类（EntitySystem 自动注册，子类实现 UIEnable）</summary>
    public abstract class UIEnableSystem<T> : SystemObject, IUIEnableSystem where T : Entity, IUIEnable
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIEnableSystem);
        }

        void IUIEnableSystem.Run(Entity o)
        {
            this.UIEnable((T)o);
        }

        protected abstract void UIEnable(T self);
    }
}
