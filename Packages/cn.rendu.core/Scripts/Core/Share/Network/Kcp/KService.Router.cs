using System;
using System.Collections.Generic;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        // RouterSYN/ACK 重连的异步回调表：channelId -> callback
        private readonly Dictionary<long, Action<byte>> routerAckCallback = new();

        public void AddRouterAckCallback(long id, Action<byte> action)
        {
            this.routerAckCallback.Add(id, action);
        }
        
        public void RemoveRouterAckCallback(long id)
        {
            this.routerAckCallback.Remove(id);
        }
    }
}
