using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// KService 的 channel 连接表（从 KService 抽取的独立类型）。
    /// 维护两张表：localConnChannels(活跃) 与 waitAcceptChannels(等待 accept)。
    /// 仅负责存储、查找与移除；副作用(Dispose / Transport.OnError)由调用方负责，
    /// 以便对增删查与等待表关联做独立的单元测试。
    /// </summary>
    public sealed class KChannelTable
    {
        private readonly Dictionary<long, KChannel> localConnChannels = new();
        private readonly Dictionary<long, KChannel> waitAcceptChannels = new();

        public int ActiveCount => this.localConnChannels.Count;
        public int WaitAcceptCount => this.waitAcceptChannels.Count;

        public KChannel Get(long id)
        {
            this.localConnChannels.TryGetValue(id, out KChannel channel);
            return channel;
        }

        public bool ContainsLocalConn(long localConn)
        {
            return this.localConnChannels.ContainsKey(localConn);
        }

        public void AddActive(KChannel channel)
        {
            this.localConnChannels.Add(channel.LocalConn, channel);
        }

        public void AddWaitAccept(KChannel channel)
        {
            this.waitAcceptChannels.Add(channel.RemoteConn, channel);
        }

        public bool TryGetWaitAccept(long remoteConn, out KChannel channel)
        {
            return this.waitAcceptChannels.TryGetValue(remoteConn, out channel);
        }

        public void RemoveWaitAccept(long remoteConn)
        {
            this.waitAcceptChannels.Remove(remoteConn);
        }

        /// <summary>从两张表中移除 localConn 对应的 channel；若存在返回该 channel，否则 null。</summary>
        public KChannel Remove(long localConn)
        {
            if (!this.localConnChannels.TryGetValue(localConn, out KChannel channel))
            {
                return null;
            }

            this.localConnChannels.Remove(localConn);
            if (this.waitAcceptChannels.TryGetValue(channel.RemoteConn, out KChannel waitChannel))
            {
                if (waitChannel.LocalConn == channel.LocalConn)
                {
                    this.waitAcceptChannels.Remove(channel.RemoteConn);
                }
            }
            return channel;
        }

        public IEnumerable<long> GetActiveKeys()
        {
            return this.localConnChannels.Keys;
        }

        public IEnumerable<KChannel> GetActiveValues()
        {
            return this.localConnChannels.Values;
        }

        public IEnumerable<KChannel> GetWaitAcceptValues()
        {
            return this.waitAcceptChannels.Values;
        }
    }
}
