using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>Invoke 部分（partial class 续接 <see cref="EventSystem"/>）：按 <c>type</c> 单点调用 <c>[Invoke]</c> handler。</summary>
    public partial class EventSystem
    {
        // Invoke跟Publish的区别(特别注意)
        // Invoke类似函数，必须有被调用方，否则异常，调用者跟被调用者属于同一模块，比如MoveComponent中的Timer计时器，调用跟被调用的代码均属于移动模块
        // 既然Invoke跟函数一样，那么为什么不使用函数呢? 因为有时候不方便直接调用，比如Config加载，在客户端跟服务端加载方式不一样。比如TimerComponent需要根据Id分发
        // 注意，不要把Invoke当函数使用，这样会造成代码可读性降低，能用函数不要用Invoke
        // publish是事件，抛出去可以没人订阅，调用者跟被调用者属于两个模块，比如任务系统需要知道道具使用的信息，则订阅道具使用事件
        /// <summary>按 <paramref name="type"/> 调 <c>[Invoke]</c> handler（无返回）：未注册则抛异常（Invoke 语义必须有人接）。</summary>
        /// <typeparam name="A">参数结构体类型。</typeparam>
        /// <param name="type">Invoke 调用键（<see cref="InvokeAttribute.Type"/> 或 <see cref="InvokeId.Of"/> 哈希）。</param>
        /// <param name="args">Invoke 参数。</param>
        /// <exception cref="Exception">未找到对应 InvokeId 的 handler 时抛异常。</exception>
        public void Invoke<A>(long type, A args) where A: struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error1: {type} {typeof(A).FullName}");
            }
            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error2: {type} {typeof(A).FullName}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error3, not AInvokeHandler: {type} {typeof(A).FullName}");
            }

            aInvokeHandler.Handle(args);
        }

        /// <summary>按 <paramref name="type"/> 调 <c>[Invoke]</c> handler（带返回 <typeparamref name="T"/>）。</summary>
        public T Invoke<A, T>(long type, A args) where A: struct
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                throw new Exception($"Invoke error4: {type} {typeof(A).FullName}");
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                throw new Exception($"Invoke error5: {type} {typeof(A).FullName}");
            }

            var aInvokeHandler = invokeHandler as AInvokeHandler<A, T>;
            if (aInvokeHandler == null)
            {
                throw new Exception($"Invoke error6, not AInvokeHandler: {type} {typeof(A).FullName} {typeof(T).FullName} ");
            }

            return aInvokeHandler.Handle(args);
        }

        /// <summary>无 <c>type</c> 的简写：调用键默认为 0（<c>[Invoke()]</c> 形式）。</summary>
        public void Invoke<A>(A args) where A: struct
        {
            Invoke(0, args);
        }

        /// <summary>无 <c>type</c> 的简写（带返回）。</summary>
        public T Invoke<A, T>(A args) where A: struct
        {
            return Invoke<A, T>(0, args);
        }


        /// <summary>按 <paramref name="type"/> 查 Invoke handler 实例（<c>null</c> 表示未注册），便于业务自己决定何时调，与 <c>Invoke</c> 不同的是不抛异常。</summary>
        /// <typeparam name="T">期望的 handler 类型（如 <c>AInvokeHandler&lt;LoginInvokeArgs&gt;</c>）。</typeparam>
        /// <typeparam name="A">参数结构体类型（仅用于索引查找）。</typeparam>
        public T GetInvoker<T, A>(long type) where A : struct where T : class
        {
            if (!this.allInvokers.TryGetValue(typeof(A), out var invokeHandlers))
            {
                return null;
            }

            if (!invokeHandlers.TryGetValue(type, out var invokeHandler))
            {
                return null;
            }

            return invokeHandler as T;
        }


    }
}
