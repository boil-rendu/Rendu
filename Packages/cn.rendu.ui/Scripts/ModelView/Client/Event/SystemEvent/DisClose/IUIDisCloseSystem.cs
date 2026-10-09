using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「关闭判定（返回是否允许关闭）」能力</summary>
    public interface IUIDisClose
    {
    }

    /// <summary>关闭判定（返回是否允许关闭） 系统契约：由框架按实体类型统一调度</summary>
    public interface IUIDisCloseSystem : ISystemType
    {
        Promise<bool> Run(Entity o);
    }

    [EntitySystem]
    /// <summary>关闭判定（返回是否允许关闭） 系统抽象基类（EntitySystem 自动注册，子类实现 UIDisClose）</summary>
    public abstract class UIDisCloseSystem<T> : SystemObject, IUIDisCloseSystem where T : Entity, IUIDisClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUIDisCloseSystem);
        }

        async Promise<bool> IUIDisCloseSystem.Run(Entity o)
        {
            return await this.UIDisClose((T)o);
        }

        protected abstract Promise<bool> UIDisClose(T self);
    }
}
