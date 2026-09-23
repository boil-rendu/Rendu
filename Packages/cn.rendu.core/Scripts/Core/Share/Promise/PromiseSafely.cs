namespace RD.Core
{
    /// <summary>
    /// 对于已知task可能会null的情况，提供一个安全的await方法
    /// 减少自己判断null的麻烦
    /// </summary>
    public static class PromiseSafely
    {
        public static Promise Await(Promise task, bool logError = false)
        {
            if (task == null)
            {
                if (logError)
                {
                    Log.Error($"PromiseSafely.Await: 这个异步是空的无法等待 请检查");
                }

                return Promise.CompletedTask;
            }

            return task;
        }

        public static async Promise<T> Await<T>(Promise<T> task, T defaultValue = default, bool logError = false)
        {
            if (task == null)
            {
                if (logError)
                {
                    Log.Error($"PromiseSafely.Await: 这个异步是空的无法等待 请检查");
                }

                return defaultValue;
            }

            return await task;
        }
    }
}