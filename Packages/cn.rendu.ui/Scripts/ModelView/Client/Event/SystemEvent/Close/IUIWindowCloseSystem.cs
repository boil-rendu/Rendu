using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「窗口关闭」能力</summary>
    public interface IUIWindowClose
    {
    }

    /// <summary>窗口关闭 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIWindowCloseSystem : ISystemType
    {
        Promise Run(Entity self, bool viewCloseResult);
    }

    [EntitySystem]
    /// <summary>窗口关闭 系统抽象基类（EntitySystem 自动注册，子类实现 UIWindowClose）</summary>
    public abstract class UIWindowCloseSystem<T, _> : SystemObject, IUIWindowCloseSystem
            where T : Entity, IUIWindowClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIWindowCloseSystem);
        }

        async Promise IUIWindowCloseSystem.Run(Entity self, bool viewCloseResult)
        {
            await this.UIWindowClose((T)self, viewCloseResult);
        }

        protected abstract Promise UIWindowClose(T self, bool viewCloseResult);
    }
}