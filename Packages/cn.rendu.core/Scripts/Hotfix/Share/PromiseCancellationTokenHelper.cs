using System;
using RD.Core;

namespace RD
{
    public static class PromiseCancellationTokenHelper
    {
        // Suppress RD0017: 此方法通过 NoContext() 启动,WaitAsync 自动从上下文获取 token 为 null 是预期行为
        // RD0017 检查的语义不适用于此场景(参数 self 是被取消的 token,不是上下文传递的 token)
        private static async Promise TimeoutAsync(this PromiseCancellationToken self, long afterTimeCancel)
        {
            if (afterTimeCancel <= 0)
            {
                return;
            }
            
            if (self.IsCancel())
            {
                return;
            }

            // 这里用了Fiber.Instance是因为我知道有什么后果, 你们千万不能乱用这个Fiber.Instance
            await Fiber.Instance.Root.GetComponent<TimerComponent>().WaitAsync(afterTimeCancel);
            if (self.IsCancel())
            {
                return;
            }
            self.Cancel();
        }

        // Suppress RD0017: GetContextAsync 自动从上下文获取原 token,参数 addCancelToken 是要添加的新 token,两者用途不同
        // Suppress RD0016: GetContextAsync 的返回值不需要检查 IsCancel,因为我们只是用它来添加取消回调
        /// <summary>
        /// 增加一个canceltoken，可以用新增的canceltoken取消协程，当然也可以用老的取消
        /// </summary>
        public static async Promise AddCancel(this Promise task, PromiseCancellationToken addCancelToken)
        {
            if (addCancelToken == null)
            {
                throw new Exception("add cancel token is null");
            }
            PromiseCancellationToken cancelToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();

            cancelToken?.Add(addCancelToken.Cancel);

            await task.NewContext(addCancelToken);
        }

        // Suppress RD0017: GetContextAsync 自动从上下文获取原 token,参数 addCancelToken 是要添加的新 token,两者用途不同
        // Suppress RD0016: GetContextAsync 的返回值不需要检查 IsCancel,因为我们只是用它来添加取消回调
        /// <summary>
        /// 增加一个canceltoken，可以用新增的canceltoken取消协程，当然也可以用老的取消
        /// </summary>
        public static async Promise<T> AddCancel<T>(this Promise<T> task, PromiseCancellationToken addCancelToken)
        {
            if (addCancelToken == null)
            {
                throw new Exception("add cancel token is null");
            }

            PromiseCancellationToken cancelToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();

            cancelToken?.Add(addCancelToken.Cancel);

            return await task.NewContext(addCancelToken);
        }
        
        public static async Promise TimeoutAsync(this Promise task, PromiseCancellationToken cancellationToken, long afterTimeCancel)
        {
            cancellationToken.TimeoutAsync(afterTimeCancel).NoContext();
            await AddCancel(task, cancellationToken);
        }
        
        public static async Promise<T> TimeoutAsync<T>(this Promise<T> task, PromiseCancellationToken cancellationToken, long afterTimeCancel)
        {
            cancellationToken.TimeoutAsync(afterTimeCancel).NoContext();
            return await AddCancel(task, cancellationToken);
        }
        
        public static async Promise TimeoutAsync(this Promise task, long afterTimeCancel)
        {
            PromiseCancellationToken cancellationToken = new();
            cancellationToken.TimeoutAsync(afterTimeCancel).NoContext();
            await AddCancel(task, cancellationToken);
        }
        
        public static async Promise<T> TimeoutAsync<T>(this Promise<T> task, long afterTimeCancel)
        {
            PromiseCancellationToken cancellationToken = new();
            cancellationToken.TimeoutAsync(afterTimeCancel).NoContext();
            return await AddCancel(task, cancellationToken);
        }
    }
}