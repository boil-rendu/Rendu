using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace RD.Core
{
    /// <summary>
    /// 编译器为 <c>async Promise</c> 方法生成的 <see cref="AsyncTaskMethodBuilder"/> 替代品（8 个标准钩子 + 状态机包装 + Context 透传）。
    /// 区别于 BCL：<see cref="Task"/> 返回 <see cref="Promise"/>（自定义可池化、零分配 Task-Like），并自动在 <c>AwaitOnCompleted</c>/<c>AwaitUnsafeOnCompleted</c> 时把外层 <see cref="Promise.Context"/> 透传给被 await 的任务，让跨 await 的 SynchronizationContext 行为可控。
    /// </summary>
    public struct AsyncPromiseMethodBuilder
    {
        private IStateMachineWrap iStateMachineWrap;

        private Promise tcs;

        // 1. Static Create method.
        /// <summary>编译器调用：开始一个 <c>async Promise</c> 方法体，构造一个空 <see cref="Promise"/> 承载最终结果。</summary>
        [DebuggerHidden]
        public static AsyncPromiseMethodBuilder Create()
        {
            AsyncPromiseMethodBuilder builder = new() { tcs = Promise.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        /// <summary>当前方法对应的 <see cref="Promise"/>（Task-Like 对象）。</summary>
        [DebuggerHidden]
        public Promise Task => this.tcs;

        // 3. SetException
        /// <summary>方法体抛出 <paramref name="exception"/> 时由编译器调用：回收状态机 + 把异常设置到 <see cref="Promise"/>。</summary>
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetException(exception);
        }

        // 4. SetResult
        /// <summary>方法体正常完成时由编译器调用：回收状态机 + <c>SetResult</c>。</summary>
        [DebuggerHidden]
        public void SetResult()
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetResult();
        }

        // 5. AwaitOnCompleted
        /// <summary>
        /// 安全版 await 钩子：把状态机包装成 <see cref="IStateMachineWrap"/> 池化对象，把 <c>MoveNext</c> 注册为 awaiter 续作，
        /// 并按 3 种策略透传 <see cref="Promise.Context"/>（<see cref="TaskType.WithContext"/> 强传 / 已存在则沿用 / 没有则把当前 awaiter 当作新 context）。
        /// </summary>
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : class, IPromise, INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(this.iStateMachineWrap.MoveNext);

            // 同样需要处理 Context 传递
            if (this.tcs.TaskType == TaskType.WithContext)
            {
                awaiter.SetContext(this.tcs.Context);
            }
            else if (this.tcs.Context != null)
            {
                awaiter.SetContext(this.tcs.Context);
            }
            else
            {
                this.tcs.Context = awaiter;
            }
        }

        // 6. AwaitUnsafeOnCompleted
        /// <summary>不安全版 await 钩子（跳过 <c>ExecutionContext</c> 捕获）：同样的状态机池化 + Context 透传，但只对 <see cref="IPromise"/> 类型的 awaiter 处理 context。</summary>
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(this.iStateMachineWrap.MoveNext);

            if (awaiter is not IPromise task)
            {
                return;
            }

            if (this.tcs.TaskType == TaskType.WithContext)
            {
                task.SetContext(this.tcs.Context);
                return;
            }

            this.tcs.Context = task;
        }

        // 7. Start
        /// <summary>启动状态机：<c>stateMachine.MoveNext()</c> 走第一个 await 之前的代码。</summary>
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        /// <summary>无操作（BCL 中用于 box 优化，本实现用 <see cref="StateMachineWrap{T}"/> 替代 box）。</summary>
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    /// <summary>带返回值的 <see cref="AsyncPromiseMethodBuilder{T}"/> 泛型版，<see cref="Task"/> 返回 <see cref="Promise{T}"/>。</summary>
    public struct AsyncPromiseMethodBuilder<T>
    {
        private IStateMachineWrap iStateMachineWrap;

        private Promise<T> tcs;

        // 1. Static Create method.
        /// <summary>编译器调用：开始一个 <c>async Promise&lt;T&gt;</c> 方法体，构造一个空 <see cref="Promise{T}"/>。</summary>
        [DebuggerHidden]
        public static AsyncPromiseMethodBuilder<T> Create()
        {
            AsyncPromiseMethodBuilder<T> builder = new AsyncPromiseMethodBuilder<T>() { tcs = Promise<T>.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        /// <summary>当前方法对应的 <see cref="Promise{T}"/>。</summary>
        [DebuggerHidden]
        public Promise<T> Task => this.tcs;

        // 3. SetException
        /// <summary>方法体抛出 <paramref name="exception"/> 时由编译器调用：回收状态机 + 异常设到 <see cref="Promise{T}"/>。</summary>
        [DebuggerHidden]
        public void SetException(Exception exception)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetException(exception);
        }

        // 4. SetResult
        /// <summary>方法体正常完成时由编译器调用：回收状态机 + 把返回值 <paramref name="ret"/> 设到 <see cref="Promise{T}"/>。</summary>
        [DebuggerHidden]
        public void SetResult(T ret)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            this.tcs.SetResult(ret);
        }

        // 5. AwaitOnCompleted
        /// <summary>安全版 await 钩子：仅当 <paramref name="awaiter"/> 实现 <see cref="IPromise"/> 时才做 Context 透传（其它 awaiter 不接入本 Promise 体系）。</summary>
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(this.iStateMachineWrap.MoveNext);

            // 如果 awaiter 是 IPromise，也需要处理 Context 传递
            if (awaiter is IPromise task)
            {
                if (this.tcs.TaskType == TaskType.WithContext)
                {
                    task.SetContext(this.tcs.Context);
                }
                else if (this.tcs.Context != null)
                {
                    task.SetContext(this.tcs.Context);
                }
                else
                {
                    this.tcs.Context = task;
                }
            }
        }

        // 6. AwaitUnsafeOnCompleted
        /// <summary>不安全版 await 钩子（<see cref="SecuritySafeCritical"/>）：仅对 <see cref="IPromise"/> awaiter 透传 Context。</summary>
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(this.iStateMachineWrap.MoveNext);

            if (awaiter is not IPromise task)
            {
                return;
            }

            if (this.tcs.TaskType == TaskType.WithContext)
            {
                task.SetContext(this.tcs.Context);
                return;
            }

            this.tcs.Context = task;
        }

        // 7. Start
        /// <summary>启动状态机：<c>stateMachine.MoveNext()</c>。</summary>
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        /// <summary>无操作（与 <see cref="AsyncPromiseMethodBuilder.SetStateMachine"/> 一致）。</summary>
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}
