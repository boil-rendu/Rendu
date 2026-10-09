using System;

namespace RD.Core
{
    /// <summary>
    /// Invoke handler 标记接口：与 <see cref="IEvent"/> 平行，<see cref="Type"/> 暴露对应请求/参数结构体类型，
    /// <see cref="EventSystem.Awake"/> 据此建索引到 <c>allInvokers</c> 二级字典。
    /// </summary>
    public interface IInvoke
    {
        /// <summary>该 handler 处理的 Invoke 参数数据类型（通常 = <c>typeof(A)</c>）。</summary>
        Type Type { get; }
    }

    /// <summary>无返回值的 Invoke handler 抽象基类：泛型 <c>A</c> 为 Invoke 参数结构体（struct 无 GC）。</summary>
    /// <remarks>派生类重写 <see cref="Handle"/> 实现处理逻辑，外部由 <see cref="EventSystem.Invoke{A}(long,A)"/> 调用。</remarks>
    /// <typeparam name="A">Invoke 参数数据类型（struct）。</typeparam>
    public abstract class AInvokeHandler<A>: HandlerObject, IInvoke where A: struct
    {
        /// <inheritdoc />
        public Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        /// <summary>派生类实现：处理 Invoke <paramref name="args"/> 的具体业务逻辑。</summary>
        public abstract void Handle(A args);
    }

    /// <summary>带返回值的 Invoke handler 抽象基类：泛型 <c>A</c> 为参数，<c>T</c> 为返回类型。</summary>
    /// <typeparam name="A">Invoke 参数数据类型（struct）。</typeparam>
    /// <typeparam name="T">返回值类型。</typeparam>
    public abstract class AInvokeHandler<A, T>: HandlerObject, IInvoke where A: struct
    {
        /// <inheritdoc />
        public Type Type
        {
            get
            {
                return typeof (A);
            }
        }

        /// <summary>派生类实现：处理 Invoke <paramref name="args"/> 并返回 <typeparamref name="T"/>。</summary>
        public abstract T Handle(A args);
    }
}
