using System;

namespace RD.Core
{
    /// <summary>
    /// Scene 级事件 handler 标记接口：实现 <see cref="Type"/> 暴露对应事件数据类型，<see cref="EventSystem.Awake"/> 据此建索引。
    /// 与 Entity 侧的 <c>cn.rendu.core</c> <see cref="IEvent{T}"/>（按泛型区分）不是同一层：本接口不泛型，handler 是 <see cref="AEvent{S,A}"/>，处理 Scene 上的事件分发。
    /// </summary>
    public interface IEvent
    {
        /// <summary>该 handler 处理的事件数据类型（通常 = <c>typeof(A)</c>）。</summary>
        Type Type { get; }
    }

    /// <summary>Scene 级事件 handler 抽象基类：泛型版本，<c>S</c> 是 <see cref="IScene"/>、<c>A</c> 是事件结构体（无 GC）。</summary>
    /// <remarks>派生类重写 <see cref="Run"/> 编写处理逻辑；外部应通过 <see cref="Handle"/> 调用（带异常捕获 + Promise 包装），不要直接 <c>Run</c>。</remarks>
    /// <typeparam name="S">所属 Scene 类型（实现 <see cref="IScene"/>）。</typeparam>
    /// <typeparam name="A">事件数据类型（struct 避免 GC）。</typeparam>
    public abstract class AEvent<S, A>: IEvent where S: class, IScene where A: struct
    {
        /// <inheritdoc />
        public Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        /// <summary>派生类实现：处理事件 <paramref name="a"/> 的具体业务逻辑。</summary>
        /// <param name="scene">当前 Scene 上下文。</param>
        /// <param name="a">事件数据。</param>
        protected abstract Promise Run(S scene, A a);

        /// <summary>供 <see cref="EventSystem.PublishAsync"/> / <see cref="EventSystem.Publish"/> 调用的入口：包 <c>try/catch</c> + Promise，异常由 <see cref="Log.Error"/> 记录而非向上抛。</summary>
        public async Promise Handle(S scene, A a)
        {
            try
            {
                await Run(scene, a);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
