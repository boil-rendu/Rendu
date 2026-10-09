using System;
using System.Collections.Generic;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        // RouterSYN/ACK 重连的异步回调表：channelId -> callback
        private readonly Dictionary<long, Action<byte>> routerAckCallback = new();

        /// <summary>注册 Router 握手 ACK 回调（连接迁移场景：Server 切换后等 Router 回 ACK 再通知对端）。</summary>
        /// <param name="id">channel Id。</param>
        /// <param name="action">ACK 到达时回调（参数 = Router 返回的状态字节）。</param>
        public void AddRouterAckCallback(long id, Action<byte> action)
        {
            this.routerAckCallback.Add(id, action);
        }

        /// <summary>移除 Router 握手 ACK 回调（ACK 已收到或超时）。</summary>
        /// <param name="id">channel Id。</param>
        public void RemoveRouterAckCallback(long id)
        {
            this.routerAckCallback.Remove(id);
        }
    }
}
