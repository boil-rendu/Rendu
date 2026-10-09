using System;

namespace RD.Core
{
    // GetComponentSystem有巨大作用，比如每次保存Unit的数据不需要所有组件都保存，只需要保存Unit变化过的组件
    // 是否变化可以通过判断该组件是否GetComponent，Get了就记录该组件
    // 这样可以只保存Unit变化过的组件
    // 再比如传送也可以做此类优化
    /// <summary>实体侧 GetComponent 标记接口：实体实现此接口表示愿意在 <c>GetComponent</c> 被调用时收到回调。</summary>
    public interface IGetComponent
    {
    }

    /// <summary>GetComponent 的 System 处理侧接口：<see cref="Run"/> 派发入口。</summary>
    public interface IGetComponentSystem: ISystemType
    {
        void Run(Entity o, Type type);
    }

    /// <summary>
    /// GetComponent 的 System 抽象基类：实现 <see cref="IGetComponentSystem.Run"/> 强类型转发到 <see cref="GetComponent(T, Type)"/>。
    /// 派生类被 <see cref="EntitySystemAttribute"/> 标记，<see cref="EntitySystemSingleton.RunGetComponentSystems"/> 自动派发。
    /// </summary>
    /// <remarks>主要用途：跟踪被 GetComponent 访问过的组件，<c>Unit</c> 存档/传送时只保存变化过的组件。</remarks>
    /// <typeparam name="T">目标实体类型，需实现 <see cref="IGetComponent"/>。</typeparam>
    [EntitySystem]
    public abstract class GetComponentSystem<T> : SystemObject, IGetComponentSystem where T: Entity, IGetComponent
    {
        void IGetComponentSystem.Run(Entity o, Type type)
        {
            this.GetComponent((T)o, type);
        }

        Type ISystemType.SystemType()
        {
            return typeof(IGetComponentSystem);
        }

        Type ISystemType.Type()
        {
            return typeof(T);
        }

        /// <summary>派生类实现：被 GetComponent 时回调（<paramref name="type"/> 为被请求的组件类型）。</summary>
        protected abstract void GetComponent(T self, Type type);
    }
}
