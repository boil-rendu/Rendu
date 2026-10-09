using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RD.Core
{
    /// <summary>
    /// 状态机包装器抽象：暴露 <see cref="MoveNext"/> 作为 <see cref="Action"/> 续作，并支持 <see cref="Recycle"/> 归还到池。
    /// 目的：避免 async 状态机在 <c>OnCompleted</c> 时被 box 成 <see cref="Action"/> 引发堆分配。
    /// </summary>
    public interface IStateMachineWrap
    {
        /// <summary>当 await 完成时回调的续作（驱动状态机继续执行）。</summary>
        Action MoveNext { get; }
        /// <summary>状态机完成/异常后归还到对象池。</summary>
        void Recycle();
    }

    /// <summary>泛型 <see cref="IStateMachineWrap"/>：池化 <typeparamref name="T"/> 类型的状态机，<see cref="MoveNext"/> 在构造时一次性绑定。</summary>
    /// <remarks>池上限 100，避免长时间闲置占用内存；<see cref="Fetch"/> 取不到时新建，<see cref="Recycle"/> 满则丢弃。</remarks>
    /// <typeparam name="T">被包装的 <see cref="IAsyncStateMachine"/> 类型。</typeparam>
    public class StateMachineWrap<T>: IStateMachineWrap where T: IAsyncStateMachine
    {
        [StaticField]
        private static readonly ConcurrentQueue<StateMachineWrap<T>> queue = new();

        /// <summary>从池中取出（或新建）一个 wrapper，把外部 <paramref name="stateMachine"/> 拷贝进 <see cref="StateMachine"/> 字段后返回。</summary>
        /// <param name="stateMachine">要包装的状态机（按值传入再赋给字段）。</param>
        public static StateMachineWrap<T> Fetch(ref T stateMachine)
        {
            if (!queue.TryDequeue(out var stateMachineWrap))
            {
                stateMachineWrap = new StateMachineWrap<T>();
            }
            stateMachineWrap.StateMachine = stateMachine;
            return stateMachineWrap;
        }

        /// <summary>归还到池：池中数量 &gt; 100 时直接丢弃不归还；否则清空 <see cref="StateMachine"/> 后入队。</summary>
        public void Recycle()
        {
            if (queue.Count > 100)
            {
                return;
            }

            this.StateMachine = default;
            queue.Enqueue(this);
        }

        private readonly Action moveNext;

        /// <inheritdoc />
        public Action MoveNext
        {
            get
            {
                return this.moveNext;
            }
        }

        private T StateMachine;

        private StateMachineWrap()
        {
            this.moveNext = this.Run;
        }

        private void Run()
        {
            this.StateMachine.MoveNext();
        }
    }
}
