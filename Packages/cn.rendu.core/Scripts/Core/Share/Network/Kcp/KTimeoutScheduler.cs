using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// KService 的超时调度器（从 KService 抽取的独立类型）。
    /// 管理“定时需要 Update 的 channel”：timeId(time -> ids) + minTime 驱动
    /// <see cref="TimerOut"/> 把到期项移入 updateIds，由 KService 每帧取出推进。
    /// 纯逻辑、不依赖网络，便于单元测试。
    /// </summary>
    public sealed class KTimeoutScheduler
    {
        // 下帧要立即 update 的 channel
        private readonly HashSet<long> updateIds = new();
        // 定时需要 update 的 channel：time -> ids
        private readonly MultiMap<long, long> timeId = new();
        private readonly List<long> timeOutTime = new();
        // 记录最小时间，避免每次都去 MultiMap 取第一个值
        private long minTime;

        /// <summary>注册下帧(time==0)或定时(time>0)需要 update 的 channel。</summary>
        public void AddToUpdate(long time, long id)
        {
            if (time == 0)
            {
                this.updateIds.Add(id);
                return;
            }
            if (time < this.minTime)
            {
                this.minTime = time;
            }
            this.timeId.Add(time, id);
        }

        /// <summary>把到期(time&lt;=timeNow)的定时项移入 updateIds，并从 timeId 中清除。</summary>
        public void TimerOut(uint timeNow)
        {
            if (this.timeId.Count == 0)
            {
                return;
            }

            if (timeNow < this.minTime)
            {
                return;
            }

            this.timeOutTime.Clear();

            foreach (var kv in this.timeId)
            {
                long k = kv.Key;
                if (k > timeNow)
                {
                    this.minTime = k;
                    break;
                }

                this.timeOutTime.Add(k);
            }

            foreach (long k in this.timeOutTime)
            {
                foreach (long v in this.timeId[k])
                {
                    this.updateIds.Add(v);
                }
                this.timeId.Remove(k);
            }
        }

        /// <summary>取出本帧需要 update 的 channel 列表并清空（立即 + 到期的合并）。</summary>
        public List<long> TakeDueUpdateIds()
        {
            var ids = new List<long>(this.updateIds);
            this.updateIds.Clear();
            return ids;
        }

        public int PendingTimedCount => this.timeId.Count;
        public int PendingImmediateCount => this.updateIds.Count;
    }
}
