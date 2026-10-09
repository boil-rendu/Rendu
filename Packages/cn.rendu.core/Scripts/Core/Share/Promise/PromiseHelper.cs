using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// <see cref="Promise"/> 工具方法集合：Context 切换、取消判定、WaitAny/WaitAll。
    /// <c>partial class</c>，另一部分在其他文件中按需扩展。
    /// </summary>
    public static partial class PromiseHelper
    {
        /// <summary>切回当前 <see cref="SynchronizationContext"/> 上下文，返回 <c>null</c> 或在上下文中注册的任意对象。</summary>
        /// <remarks>常用于「希望 await 后回到主线程」的桥接：把一个空 <see cref="Promise{T}"/> 注册到目标上下文，<c>await</c> 完就在那个上下文继续。</remarks>
        /// <typeparam name="T">期望从上下文取回的对象类型（无约束，业务自定语义）。</typeparam>
        public static async Promise<T> GetContextAsync<T>() where T: class
        {
            Promise<object> tcs = Promise<object>.Create(true);
            tcs.TaskType = TaskType.ContextTask;
            object ret = await tcs;
            if (ret == null)
            {
                return null;
            }
            return (T)ret;
        }

        /// <summary><see cref="PromiseCancellationToken"/> 的 null-safe 取消判定扩展：<c>null</c> token 一律视为「未取消」。</summary>
        public static bool IsCancel(this PromiseCancellationToken self)
        {
            if (self == null)
            {
                return false;
            }
            return self.IsDispose();
        }

        /// <summary>
        /// 协程计数门闩：<see cref="WaitAll(List{Promise})"/>/<see cref="WaitAny(List{Promise})"/> 内部使用。
        /// 每个子任务完成时 <c>--count</c>，到 0 才释放主等待协程；<c>WaitAny</c> 模式下 <c>count = 1</c>，任意一个完成就放行。
        /// </summary>
        private class CoroutineBlocker
        {
            private int count;

            private Promise tcs;

            public CoroutineBlocker(int count)
            {
                this.count = count;
            }

            /// <summary>包装一个子任务：完成时（无论异常/取消）<c>--count</c>，到 0 时 <c>SetResult</c> 唤醒 <see cref="WaitAsync"/>。</summary>
            public async Promise RunSubCoroutineAsync(Promise task)
            {
                try
                {
                    await task;
                }
                finally
                {
                    --this.count;

                    if (this.count <= 0 && this.tcs != null)
                    {
                        Promise t = this.tcs;
                        this.tcs = null;
                        t.SetResult();
                    }
                }
            }

            /// <summary>主协程挂起等待：<c>count</c> 已 ≤ 0 时立即返回，否则挂起到 <see cref="RunSubCoroutineAsync"/> 全部完成唤醒。</summary>
            public async Promise WaitAsync()
            {
                if (this.count <= 0)
                {
                    return;
                }
                this.tcs = Promise.Create(true);
                await tcs;
            }
        }

        /// <summary>等待 <paramref name="tasks"/> 中任意一个完成即可继续（<c>count = 1</c>，首个完成的协程把门闩归零）。</summary>
        public static async Promise WaitAny(List<Promise> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);

            foreach (Promise task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        /// <summary><see cref="Promise"/> 数组版本的 <see cref="WaitAny(List{Promise})"/>。</summary>
        public static async Promise WaitAny(Promise[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);

            foreach (Promise task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }



        /// <summary>等待 <paramref name="tasks"/> 全部完成（任意一个异常会立即通过 <see cref="Promise.ExceptionHandler"/> 记录，但 <c>WaitAll</c> 本身不抛）。</summary>
        public static async Promise WaitAll(Promise[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length);

            foreach (Promise task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        /// <summary><see cref="List{Promise}"/> 版本的 <see cref="WaitAll(Promise[])"/>。</summary>
        public static async Promise WaitAll(List<Promise> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count);

            foreach (Promise task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }
    }
}
