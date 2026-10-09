using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RD.Core
{
    /// <summary>
    /// 空 awaiter（Task-Like 占位）：<see cref="IsCompleted"/> 永远为 <c>true</c>，<see cref="OnCompleted"/>/<see cref="UnsafeOnCompleted"/> 不做任何事。
    /// 作为 <see cref="AsyncPromiseVoidMethodBuilder.Task"/> 的返回值，让 <c>async void</c> 方法编译期满足「需要 awaitable 返回类型」的要求。
    /// </summary>
    [AsyncMethodBuilder(typeof (AsyncPromiseVoidMethodBuilder))]
    internal struct PromiseVoid: ICriticalNotifyCompletion
    {
        [DebuggerHidden]
        public void Coroutine()
        {
        }

        /// <summary>永远 <c>true</c>：await 立即完成。</summary>
        [DebuggerHidden]
        public bool IsCompleted => true;

        /// <summary>空操作：<c>continuation</c> 永远不会被调用（已 IsCompleted）。</summary>
        [DebuggerHidden]
        public void OnCompleted(Action continuation)
        {
        }

        /// <summary>空操作：同 <see cref="OnCompleted"/>。</summary>
        [DebuggerHidden]
        public void UnsafeOnCompleted(Action continuation)
        {
        }
    }
}
