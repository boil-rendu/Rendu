using System;
using RD.Core;

namespace RD.Client
{
    /// <summary>标记接口：实现它的 UI 实体具备「关闭（可被拦截，返回是否允许关闭）」能力</summary>
    public interface IUIClose
    {
    }

    /// <summary>关闭（可被拦截，返回是否允许关闭） 系统契约：由框架按实体类型统一调度</summary>
    public interface IUICloseSystem : ISystemType
    {
        Promise<bool> Run(Entity o);
    }

    [EntitySystem]
    /// <summary>关闭（可被拦截，返回是否允许关闭） 系统抽象基类（EntitySystem 自动注册，子类实现 UIClose）</summary>
    public abstract class UICloseSystem<T> : SystemObject, IUICloseSystem where T : Entity, IUIClose
    {
        Type ISystemType.Type()
        {
            return typeof(T);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IUICloseSystem);
        }

        async Promise<bool> IUICloseSystem.Run(Entity o)
        {
            return await this.UIClose((T)o);
        }

        protected abstract Promise<bool> UIClose(T self);
    }
}
