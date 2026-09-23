using System;
using System.Collections.Generic;

namespace RD.Core
{
    public static partial class PromiseHelper
    {
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
    
        public static bool IsCancel(this PromiseCancellationToken self)
        {
            if (self == null)
            {
                return false;
            }
            return self.IsDispose();
        }
        
        private class CoroutineBlocker
        {
            private int count;

            private Promise tcs;

            public CoroutineBlocker(int count)
            {
                this.count = count;
            }
            
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