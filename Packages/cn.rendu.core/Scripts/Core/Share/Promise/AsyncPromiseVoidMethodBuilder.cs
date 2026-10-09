using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace RD.Core
{
    /// <summary>
    /// <c>async void</c> 等价 Task-Like 方法的 <see cref="AsyncVoidMethodBuilder"/> 替代品。
    /// 与 <see cref="AsyncPromiseMethodBuilder"/> 区别：没有 <see cref="Promise"/> 状态对象（Task = <see cref="PromiseVoid"/> 的空壳），异常通过 <see cref="Promise.ExceptionHandler"/> 派发（不能 await catch）。
    /// </summary>
    internal struct AsyncPromiseVoidMethodBuilder
    {
        private IStateMachineWrap iStateMachineWrap;

        // 1. Static Create method.
        /// <summary>编译器调用：开始一个 <c>async void</c> 方法体（无 Promise 状态对象）。</summary>
        [DebuggerHidden]
        public static AsyncPromiseVoidMethodBuilder Create()
        {
            AsyncPromiseVoidMethodBuilder builder = new();
            return builder;
        }

        // 2. TaskLike Task property(void)
        /// <summary>当前方法对应的 Task-Like 对象：始终是 <c>default(PromiseVoid)</c>（<see cref="PromiseVoid.IsCompleted"/> = <c>true</c> 的空 awaiter）。</summary>
        [DebuggerHidden]
        public PromiseVoid Task => default;

        // 3. SetException
        /// <summary>方法体抛出 <paramref name="e"/> 时由编译器调用：回收状态机 + 通过 <see cref="Promise.ExceptionHandler"/> 派发异常（caller 无法 await catch）。</summary>
        [DebuggerHidden]
        public void SetException(Exception e)
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
            Promise.ExceptionHandler.Invoke(e);
        }

        // 4. SetResult
        /// <summary>方法体正常完成时由编译器调用：回收状态机。</summary>
        [DebuggerHidden]
        public void SetResult()
        {
            if (this.iStateMachineWrap != null)
            {
                this.iStateMachineWrap.Recycle();
                this.iStateMachineWrap = null;
            }
        }

        // 5. AwaitOnCompleted
        /// <summary>安全版 await 钩子：要求 awaiter 实现 <see cref="IPromise"/>（不接 BCL Task），把状态机包装为池化对象并把 <c>MoveNext</c> 注册为续作。</summary>
        [DebuggerHidden]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : IPromise, INotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.OnCompleted(this.iStateMachineWrap.MoveNext);
        }

        // 6. AwaitUnsafeOnCompleted
        /// <summary>不安全版 await 钩子（<see cref="SecuritySafeCritical"/>）。</summary>
        [DebuggerHidden]
        [SecuritySafeCritical]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : IPromise, ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
        {
            this.iStateMachineWrap ??= StateMachineWrap<TStateMachine>.Fetch(ref stateMachine);
            awaiter.UnsafeOnCompleted(this.iStateMachineWrap.MoveNext);
        }

        // 7. Start
        /// <summary>启动状态机：<c>stateMachine.MoveNext()</c>。</summary>
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        /// <summary>无操作。</summary>
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}
