using System;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        /// <summary>按 channel <paramref name="channelId"/> 发送已包好的 <see cref="MemoryBuffer"/>：转发给对应 <see cref="KChannel.Send"/>，由其负责 KCP 帧封装 + 下层 socket 发送。</summary>
        /// <param name="channelId">channel Id（找不到对应 channel 则静默返回）。</param>
        /// <param name="memoryBuffer">包好的消息缓冲（包头已由 <see cref="MessageSerializeHelper"/> 写好）。</param>
        public override void Send(long channelId, MemoryBuffer memoryBuffer)
        {
            KChannel channel = this.Get(channelId);
            if (channel == null)
            {
                return;
            }
            channel.Send(memoryBuffer);
        }
    }
}
