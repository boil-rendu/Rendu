using System;
using System.Collections;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
    /// <summary>
    /// <see cref="TimerComponent"/> 的系统扩展（Hotfix 层），承载定时器的创建、取消与每帧调度逻辑：
    /// <list type="bullet">
    ///   <item><see cref="Update"/>：每帧推进，取出已到期定时器并按 <see cref="TimerClass"/> 分发给 EventSystem 或 Promise。</item>
    ///   <item>回调式（可热更）：NewOnceTimer / NewFrameTimer / NewRepeatedTimer。</item>
    ///   <item>await 式（逻辑连贯但不可热更）：WaitAsync / WaitTillAsync / WaitFrameAsync。</item>
    ///   <item><see cref="Remove"/>：按 timerId 取消尚未触发的定时器。</item>
    /// </list>
    /// </summary>
    [EntitySystemOf(typeof(TimerComponent))]
    public static partial class TimerComponentSystem
    {
        /// <summary>TimerComponent 无初始化逻辑，Awake 留空。</summary>
        [EntitySystem]
        private static void Awake(this TimerComponent self)
        {
        }
        
        /// <summary>
        /// 每帧调度核心。流程：
        /// 1) 无定时器或当前时间早于最早到期时间(<see cref="TimerComponent.minTime"/>)则直接返回；
        /// 2) 遍历 timeId，把 &lt;= 当前时间的时间点收集进 timeOutTime；
        /// 3) 把每个到期时间点对应的 timerId 收集进 timeOutTimerIds，并从 timeId 移除；
        /// 4) 逐个取出 TimerAction，按 <see cref="TimerClass"/> 调用 <see cref="Run"/> 触发。
        /// </summary>
        [EntitySystem]
        private static void Update(this TimerComponent self)
        {
            if (self.timeId.Count == 0)
            {
                return;
            }

            long timeNow = self.GetNow();

            if (timeNow < self.minTime)
            {
                return;
            }

            foreach (var kv in self.timeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    self.minTime = k;
                    break;
                }

                self.timeOutTime.Enqueue(k);
            }

            while (self.timeOutTime.Count > 0)
            {
                long time = self.timeOutTime.Dequeue();
                List<long> list = self.timeId[time];
                for (int i = 0; i < list.Count; ++i)
                {
                    long timerId = list[i];
                    self.timeOutTimerIds.Enqueue(timerId);
                }
                self.timeId.Remove(time);
            }

            if (self.timeId.Count == 0)
            {
                self.minTime = long.MaxValue;
            }

            while (self.timeOutTimerIds.Count > 0)
            {
                long timerId = self.timeOutTimerIds.Dequeue();

                if (!self.timerActions.Remove(timerId, out TimerAction timerAction))
                {
                    continue;
                }
                
                self.Run(timerId, ref timerAction);
            }
        }
        
        /// <summary>生成全局唯一的 timerId（自增 idGenerator）。</summary>
        private static long GetId(this TimerComponent self)
        {
            return ++self.idGenerator;
        }

        /// <summary>取当前服务器帧时间，作为定时器的时间基准。</summary>
        private static long GetNow(this TimerComponent self)
        {
            return TimeInfo.Instance.ServerFrameTime();
        }

        /// <summary>
        /// 按 <see cref="TimerClass"/> 触发单个定时器：
        /// <list type="bullet">
        ///   <item>OnceTimer：通过 EventSystem.Invoke(type, callback) 派发回调（可热更）。</item>
        ///   <item>OnceWaitTimer：对绑定的 Promise 调用 SetResult，唤醒 await 方。</item>
        ///   <item>RepeatedTimer：先按周期重新入队，再派发回调。</item>
        /// </list>
        /// </summary>
        private static void Run(this TimerComponent self, long timerId, ref TimerAction timerAction)
        {
            switch (timerAction.TimerClass)
            {
                case TimerClass.OnceTimer:
                {
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
                case TimerClass.OnceWaitTimer:
                {
                    Promise tcs = timerAction.Object as Promise;
                    tcs.SetResult();
                    break;
                }
                case TimerClass.RepeatedTimer:
                {                    
                    long timeNow = self.GetNow();
                    timerAction.StartTime = timeNow;
                    self.AddTimer(timerId, ref timerAction);
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
            }
        }

        /// <summary>
        /// 将定时器注册进内部索引：写入 timeId（到期时间 -&gt; timerId）与 timerActions（timerId -&gt; 参数），
        /// 并在更早到期时更新 minTime，供 <see cref="Update"/> 快速剪枝。
        /// </summary>
        private static void AddTimer(this TimerComponent self, long timerId, ref TimerAction timer)
        {
            long tillTime = timer.StartTime + timer.Time;
            self.timeId.Add(tillTime, timerId);
            self.timerActions.Add(timerId, timer);
            if (tillTime < self.minTime)
            {
                self.minTime = tillTime;
            }
        }

        /// <summary>取消定时器并把入参 id 归零，避免外部持有失效 id 而重复取消。</summary>
        public static bool Remove(this TimerComponent self, ref long id)
        {
            long i = id;
            id = 0;
            return self.Remove(i);
        }

        /// <summary>按 timerId 从 timerActions 中移除尚未触发的定时器；id 为 0 或不存在返回 false。</summary>
        private static bool Remove(this TimerComponent self, long id)
        {
            if (id == 0)
            {
                return false;
            }

            if (!self.timerActions.Remove(id, out TimerAction _))
            {
                return false;
            }
            return true;
        }

        /// <summary>await 式等待：挂起直到指定的绝对时间 <paramref name="tillTime"/>，可被 PromiseCancellationToken 取消。</summary>
        public static async Promise WaitTillAsync(this TimerComponent self, long tillTime)
        {
            long timeNow = self.GetNow();
            if (timeNow >= tillTime)
            {
                return;
            }

            Promise tcs = Promise.Create(true);
            long timerId = self.GetId();
            TimerAction timer = new(TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            self.AddTimer(timerId, ref timer);

            void CancelAction()
            {
                if (self.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            PromiseCancellationToken cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        /// <summary>await 式等待一帧（等价于 WaitAsync(1)）。</summary>
        public static async Promise WaitFrameAsync(this TimerComponent self)
        {
            await self.WaitAsync(1);
        }

        /// <summary>await 式等待相对时间 <paramref name="time"/> 毫秒，可被 PromiseCancellationToken 取消。</summary>
        public static async Promise WaitAsync(this TimerComponent self, long time)
        {
            if (time == 0)
            {
                return;
            }

            long timeNow = self.GetNow();

            Promise tcs = Promise.Create(true);
            long timerId = self.GetId();
            TimerAction timer = new (TimerClass.OnceWaitTimer, timeNow, time, 0, tcs);
            self.AddTimer(timerId, ref timer);

            void CancelAction()
            {
                if (self.Remove(timerId))
                {
                    tcs.SetResult();
                }
            }

            PromiseCancellationToken cancellationToken = await PromiseHelper.GetContextAsync<PromiseCancellationToken>();
            try
            {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelAction);
            }
        }

        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        /// <summary>回调式（可热更）一次性定时器：到 tillTime 时通过 EventSystem 派发 type 回调。</summary>
        public static long NewOnceTimer(this TimerComponent self, long tillTime, int type, object args)
        {
            long timeNow = self.GetNow();
            if (tillTime < timeNow)
            {
                Log.Error($"new once time too small: {tillTime}");
            }
            long timerId = self.GetId();
            TimerAction timer = new (TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, args);
            self.AddTimer(timerId, ref timer);
            return timerId;
        }

        /// <summary>回调式每帧定时器：DOTNET 环境下最小间隔 100ms，否则按帧执行。</summary>
        public static long NewFrameTimer(this TimerComponent self, int type, object args)
        {
#if DOTNET
            return self.NewRepeatedTimerInner(100, type, args);
#else
            return self.NewRepeatedTimerInner(0, type, args);
#endif
        }

        /// <summary>
        /// 创建一个重复定时器（内部工厂）：按周期反复触发 type 回调；每帧执行的(time=0/小间隔)不进入 timeId 遍历路径。
        /// </summary>
        private static long NewRepeatedTimerInner(this TimerComponent self, long time, int type, object args)
        {
#if DOTNET
            if (time < 100)
            {
                throw new Exception($"repeated timer < 100, timerType: time: {time}");
            }
#endif
            
            long timeNow = self.GetNow();
            long timerId = self.GetId();
            TimerAction timer = new (TimerClass.RepeatedTimer, timeNow, time, type, args);

            // 每帧执行的不用加到timerId中，防止遍历
            self.AddTimer(timerId, ref timer);
            return timerId;
        }

        /// <summary>回调式重复定时器：间隔须 &gt;= 100ms，否则记错误并返回 0（不创建）。</summary>
        public static long NewRepeatedTimer(this TimerComponent self, long time, int type, object args)
        {
            if (time < 100)
            {
                Log.Error($"time too small: {time}");
                return 0;
            }

            return self.NewRepeatedTimerInner(time, type, args);
        }
    }
}
