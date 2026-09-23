using System;
using System.Collections.Generic;
using System.Net;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        // 连接表：活跃与等待 accept 的 channel（抽为独立类型 KChannelTable）
        private readonly KChannelTable channels = new();

        public KChannel Get(long id) => this.channels.Get(id);

        public override void Create(long id, IPEndPoint ipEndPoint)
        {
            if (this.channels.ContainsLocalConn(id))
            {
                return;
            }

            try
            {
                // 低32bit是localConn
                uint localConn = (uint)id;
                KChannel kChannel = new KChannel(localConn, ipEndPoint, this);
                this.channels.AddActive(kChannel);
            }
            catch (Exception e)
            {
                Log.Error($"kservice get error: {id}\n{e}");
            }
        }

        public override void Remove(long id, int error = 0)
        {
            KChannel kChannel = this.channels.Remove(id);
            if (kChannel == null)
            {
                return;
            }

            kChannel.Error = error;
            
            Log.Debug($"kservice remove channel: {id} {kChannel.LocalConn} {kChannel.RemoteConn} {error}");
            kChannel.Dispose();
            this.Transport.OnError(id, error);
        }
    }
}
