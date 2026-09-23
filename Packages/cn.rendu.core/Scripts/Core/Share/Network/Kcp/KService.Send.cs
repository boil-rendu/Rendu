using System;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
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
