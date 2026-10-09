using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「UI 禁用」能力</summary>
    public interface IUIDisable
    {
    }

    /// <summary>UI 禁用 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIDisableSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    /// <summary>UI 禁用 系统抽象基类（EntitySystem 自动注册，子类实现 UIDisable）</summary>
    public abstract class UIDisableSystem<T> : SystemObject, IUIDisableSystem where T : Entity, IUIDisable
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIDisableSystem);
        }

        void IUIDisableSystem.Run(Entity o)
        {
            this.UIDisable((T)o);
        }

        protected abstract void UIDisable(T self);
    }
}
