using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;

namespace RD.Core
{
    public struct AsyncPromiseMethodBuilder
    {
        private IStateMachineWrap iStateMachineWrap;
        
        private Promise tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncPromiseMethodBuilder Create()
        {
            AsyncPromiseMethodBuilder builder = new() { tcs = Promise.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public Promise Task => this.tcs;

        // 3. SetException
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
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    public struct AsyncPromiseMethodBuilder<T>
    {
        private IStateMachineWrap iStateMachineWrap;
        
        private Promise<T> tcs;

        // 1. Static Create method.
        [DebuggerHidden]
        public static AsyncPromiseMethodBuilder<T> Create()
        {
            AsyncPromiseMethodBuilder<T> builder = new AsyncPromiseMethodBuilder<T>() { tcs = Promise<T>.Create(true) };
            return builder;
        }

        // 2. TaskLike Task property.
        [DebuggerHidden]
        public Promise<T> Task => this.tcs;

        // 3. SetException
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
        [DebuggerHidden]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        // 8. SetStateMachine
        [DebuggerHidden]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }
}