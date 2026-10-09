using System;
using System.Collections.Generic;
using System.Net;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        // 连接表：活跃与等待 accept 的 channel（抽为独立类型 KChannelTable）
        private readonly KChannelTable channels = new();

        /// <summary>按 channel <paramref name="id"/> 取出 <see cref="KChannel"/>，找不到返回 <c>null</c>。</summary>
        public KChannel Get(long id) => this.channels.Get(id);

        /// <summary>connect 侧主动建立到 <paramref name="ipEndPoint"/> 的 KCP 连接：若已存在 <paramref name="id"/> 的 localConn 则直接返回，否则创建 <see cref="KChannel"/> 并入「活跃连接」表。</summary>
        /// <param name="id">业务侧 channel Id（= localConn）。</param>
        /// <param name="ipEndPoint">目标远端地址。</param>
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

        /// <summary>从连接表移除并 dispose 该 channel：触发 <see cref="KService.Transport"/>.<c>OnError</c> 回调。</summary>
        /// <param name="id">channel Id。</param>
        /// <param name="error">错误码（0 = 主动断开不触发 <c>ErrorCallback</c>）。</param>
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
