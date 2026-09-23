using System;
using System.Collections.Generic;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        // 超时调度：抽为独立类型 KTimeoutScheduler
        private readonly KTimeoutScheduler timeout = new();

        private readonly List<long> cacheIds = new();

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
