using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「初始化」能力</summary>
    public interface IUIInitialize
    {
    }

    /// <summary>初始化 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIInitializeSystem : ISystemType
    {
        void Run(Entity o);
    }

    [EntitySystem]
    /// <summary>初始化 系统抽象基类（EntitySystem 自动注册，子类实现 UIInitialize）</summary>
    public abstract class UIInitializeSystem<T> : SystemObject, IUIInitializeSystem where T : Entity, IUIInitialize
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIInitializeSystem);
        }

        void IUIInitializeSystem.Run(Entity o)
        {
            this.UIInitialize((T)o);
        }

        protected abstract void UIInitialize(T self);
    }
}
