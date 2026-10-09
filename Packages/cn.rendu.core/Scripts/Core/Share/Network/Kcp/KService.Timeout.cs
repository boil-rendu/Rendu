using System;
using System.Collections.Generic;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        // 超时调度：抽为独立类型 KTimeoutScheduler
        private readonly KTimeoutScheduler timeout = new();

        private readonly List<long> cacheIds = new();

        /// <summary>把 channel <paramref name="id"/> 安排在 <paramref name="time"/> 时刻被 <see cref="KChannel.Update"/> 调度一次。</summary>
        /// <param name="time">目标时间戳（毫秒，相对 <see cref="TimeInfo.FrameTime"/>）。</param>
        /// <param name="id">要调度的 channel Id。</param>
        public void AddToUpdate(long time, long id) => this.timeout.AddToUpdate(time, id);

        private void TimerOut(uint timeNow) => this.timeout.TimerOut(timeNow);

        // 检查 waitAccept 中超时未完成握手的 channel，触发 accept 超时错误
        private void CheckWaitAcceptChannel(uint timeNow)
        {
            cacheIds.Clear();
            foreach (KChannel kChannel in this.channels.GetWaitAcceptValues())
            {
                if (kChannel.IsDisposed)
                {
                    continue;
                }

                if (kChannel.IsConnected)
                {
                    continue;
                }

                if (timeNow < kChannel.CreateTime + ConnectTimeoutTime)
                {
                    continue;
                }

                cacheIds.Add(kChannel.Id);
            }

            foreach (long id in this.cacheIds)
            {
                if (!this.channels.TryGetWaitAccept(id, out KChannel kChannel))
                {
                    continue;
                }
                kChannel.OnError(ErrorCore.ERR_KcpAcceptTimeout);
            }
        }

        // 推进当前帧需要 update 的 channel
        private void UpdateChannel(uint timeNow)
        {
            foreach (long id in this.timeout.TakeDueUpdateIds())
            {
                KChannel kChannel = this.Get(id);
                if (kChannel == null)
                {
                    continue;
                }

                if (kChannel.Id == 0)
                {
                    continue;
                }

                kChannel.Update(timeNow);
            }
        }
    }
}
