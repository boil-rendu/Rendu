using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「数据绑定」能力</summary>
    public interface IUIBind
    {
    }

    /// <summary>数据绑定 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIBindSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    /// <summary>数据绑定 系统抽象基类（EntitySystem 自动注册，子类实现 UIBind）</summary>
    public abstract class UIBindSystem<T> : SystemObject, IUIBindSystem where T : Entity, IUIBind
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIBindSystem);
        }

        void IUIBindSystem.Run(Entity o)
        {
            this.UIBind((T)o);
        }

        protected abstract void UIBind(T self);
    }
}
