using System;
using System.Net;

namespace RD.Core
{
    public sealed partial class KService : AService
    {
        private readonly byte[] cache = new byte[2048];
        
        private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// 收包主循环（接收阶段）：只要 Transport 仍有可读数据就持续拉取，
        /// 逐包交给 <see cref="ProcessPacket"/> 解析与分发。单包处理失败不会影响后续收包。
        /// </summary>
        private void Recv()
        {
            if (this.Transport == null)
            {
                return;
            }

            while (this.Transport != null && this.Transport.Available() > 0)
            {
                int messageLength = this.Transport.Recv(this.cache, ref this.ipEndPoint);
                // 长度小于1，不是正常的消息
                if (messageLength < 1)
                {
                    continue;
                }

                this.ProcessPacket(messageLength);
            }
        }

        /// <summary>
        /// 解析与分发单个数据报（解析+分发阶段）：按首字节协议类型 flag 路由到对应处理逻辑。
        /// 整个分发过程包在 try/catch 中，任何未预期异常都会被记录而不会中断上层收包循环。
        /// </summary>
        private void ProcessPacket(int messageLength)
        {
            // accept：首字节为协议类型
            byte flag = this.cache[0];
            try
            {
                switch (flag)
                {
                    case KcpProtocalType.RouterACK:
                    case KcpProtocalType.RouterReconnectACK:
                        this.HandleRouterAck();
                        break;
                    case KcpProtocalType.RouterReconnectSYN:
                        this.HandleRouterReconnectSyn(messageLength);
                        break;
                    case KcpProtocalType.SYN: // accept
                        this.HandleSyn(messageLength);
                        break;
                    case KcpProtocalType.ACK: // connect 返回
                        this.HandleAck(messageLength);
                        break;
                    case KcpProtocalType.FIN: // 断开
                        this.HandleFin(messageLength);
                        break;
                    case KcpProtocalType.MSG: // 业务消息
                        this.HandleMsg(messageLength);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error($"kservice process packet error, flag={flag}\n{e}");
            }
        }

        /// <summary>
        /// 处理 RouterACK / RouterReconnectACK：根据拼接的会话 id 触发预设回调。
        /// </summary>
        private void HandleRouterAck()
        {
            uint remoteConn = BitConverter.ToUInt32(this.cache, 1);
            uint localConn = BitConverter.ToUInt32(this.cache, 5);

            long id = (long)(((ulong)localConn << 32) | remoteConn);
            if (this.routerAckCallback.TryGetValue(id, out var action))
            {
                action.Invoke(this.cache[0]);
            }
        }

        /// <summary>
        /// 处理 RouterReconnectSYN：校验通道一致性后回发 RouterReconnectACK。
        /// </summary>
        private void HandleRouterReconnectSyn(int messageLength)
        {
            // 长度!=9，不是 RouterReconnectSYN 消息
            if (messageLength != 9)
            {
                return;
            }

            string realAddress = null;
            uint remoteConn = BitConverter.ToUInt32(this.cache, 1);
            uint localConn = BitConverter.ToUInt32(this.cache, 5);

            KChannel kChannel = this.channels.Get(localConn);
            if (kChannel == null)
            {
                Log.Warning($"kchannel reconnect not found channel: {localConn} {remoteConn} {realAddress}");
                return;
            }

            // 这里必须校验 localConn，客户端重连，localConn 一定是一样的
            if (localConn != kChannel.LocalConn)
            {
                Log.Warning($"kchannel reconnect localconn error: {localConn} {remoteConn} {realAddress} {kChannel.LocalConn}");
                return;
            }

            if (remoteConn != kChannel.RemoteConn)
            {
                Log.Warning($"kchannel reconnect remoteconn error: {localConn} {remoteConn} {realAddress} {kChannel.RemoteConn}");
                return;
            }

            // 重连的时候 router 地址变化，这个不能放到 msg 中，必须经过严格的验证才能切换
            if (!this.ipEndPoint.Equals(kChannel.RemoteAddress))
            {
                kChannel.RemoteAddress = this.ipEndPoint.Clone();
            }

            try
            {
                byte[] buffer = this.cache;
                buffer.WriteTo(0, KcpProtocalType.RouterReconnectACK);
                buffer.WriteTo(1, kChannel.LocalConn);
                buffer.WriteTo(5, kChannel.RemoteConn);
                this.Transport.Send(buffer, 0, 9, this.ipEndPoint, ChannelType.Accept);
            }
            catch (Exception e)
            {
                Log.Error(e);
                kChannel.OnError(ErrorCore.ERR_SocketCantSend);
            }
        }

        /// <summary>
        /// 处理 SYN（accept 侧）：创建/匹配等待中的通道并回发 ACK。
        /// </summary>
        private void HandleSyn(int messageLength)
        {
            // 长度<9，不是 SYN 消息
            if (messageLength < 9)
            {
                return;
            }

            string realAddress = null;
            if (messageLength > 9)
            {
                realAddress = this.cache.ToStr(9, messageLength - 9);
            }
            else
            {
                realAddress = this.ipEndPoint.ToString();    
            }

            uint remoteConn = BitConverter.ToUInt32(this.cache, 1);
            uint localConn = BitConverter.ToUInt32(this.cache, 5);

            this.channels.TryGetWaitAccept(remoteConn, out KChannel kChannel);
            if (kChannel == null)
            {
                // accept 的 localConn 不能与 connect 的 localConn 冲突，所以设置为一个大的数
                // localConn 被人猜出来问题不大，因为 remoteConn 是随机的，第三方并不知道
                localConn = NetServices.Instance.CreateAcceptChannelId();
                // 已存在同样的 localConn，则不处理，等待下次 sync
                if (this.channels.ContainsLocalConn(localConn))
                {
                    return;
                }

                kChannel = new KChannel(localConn, remoteConn, this.ipEndPoint.Clone(), this);
                this.channels.AddWaitAccept(kChannel); // 连接上了或者超时后会删除
                this.channels.AddActive(kChannel);
                
                kChannel.RealAddress = realAddress;

                IPEndPoint realEndPoint = NetworkHelper.ToIPEndPoint(kChannel.RealAddress);
                this.AcceptCallback(kChannel.Id, realEndPoint);
            }
            if (kChannel.RemoteConn != remoteConn)
            {
                return;
            }

            // 地址跟上次的不一致则跳过
            if (kChannel.RealAddress != realAddress)
            {
                Log.Error($"kchannel syn address diff: {kChannel.Id} {kChannel.RemoteAddress} {realAddress}");
                return;
            }

            try
            {
                byte[] buffer = this.cache;
                buffer.WriteTo(0, KcpProtocalType.ACK);
                buffer.WriteTo(1, kChannel.LocalConn);
                buffer.WriteTo(5, kChannel.RemoteConn);
                Log.Info($"kservice syn: {kChannel.Id} {remoteConn} {localConn} {kChannel.RemoteAddress}");
                
                this.Transport.Send(buffer, 0, 9, kChannel.RemoteAddress, ChannelType.Accept);
            }
            catch (Exception e)
            {
                Log.Error(e);
                kChannel.OnError(ErrorCore.ERR_SocketCantSend);
            }
        }

        /// <summary>
        /// 处理 ACK（connect 返回）：补全通道远端连接号并触发连接完成。
        /// </summary>
        private void HandleAck(int messageLength)
        {
            // 长度!=9，不是 connect 消息
            if (messageLength != 9)
            {
                return;
            }

            uint remoteConn = BitConverter.ToUInt32(this.cache, 1);
            uint localConn = BitConverter.ToUInt32(this.cache, 5);
            KChannel kChannel = this.Get(localConn);
            if (kChannel != null)
            {
                Log.Info($"kservice ack: {localConn} {remoteConn}");
                kChannel.RemoteConn = remoteConn;
                kChannel.HandleConnnect();
            }
        }

        /// <summary>
        /// 处理 FIN（断开）：校验远端连接号后通知通道对端断开。
        /// </summary>
        private void HandleFin(int messageLength)
        {
            // 长度!=13，不是 DisConnect 消息
            if (messageLength != 13)
            {
                return;
            }

            uint remoteConn = BitConverter.ToUInt32(this.cache, 1);
            uint localConn = BitConverter.ToUInt32(this.cache, 5);
            int error = BitConverter.ToInt32(this.cache, 9);

            // 处理 chanel
            KChannel kChannel = this.Get(localConn);
            if (kChannel == null)
            {
                return;
            }
            
            // 校验 remoteConn，防止第三方攻击
            if (kChannel.RemoteConn != remoteConn)
            {
                return;
            }
            
            Log.Info($"kservice recv fin: {localConn} {remoteConn} {error}");
            kChannel.OnError(ErrorCore.ERR_PeerDisconnect);
        }

        /// <summary>
        /// 处理 MSG（业务消息）：交还通道投递；通道不存在则回发断开通知。
        /// </summary>
        private void HandleMsg(int messageLength)
        {
            // 长度<9，不是 Msg 消息
            if (messageLength < 9)
            {
                return;
            }

            // 处理 chanel
            uint remoteConn = BitConverter.ToUInt32(this.cache, 1);
            uint localConn = BitConverter.ToUInt32(this.cache, 5);

            KChannel kChannel = this.Get(localConn);
            if (kChannel == null)
            {
                // 通知对方断开
                this.Disconnect(localConn, remoteConn, ErrorCore.ERR_KcpNotFoundChannel, this.ipEndPoint, 1);
                return;
            }

            // 校验 remoteConn，防止第三方攻击
            if (kChannel.RemoteConn != remoteConn)
            {
                return;
            }

            // 对方发来 msg，说明 kchannel 连接完成
            if (!kChannel.IsConnected)
            {
                kChannel.IsConnected = true;
                this.channels.RemoveWaitAccept(kChannel.RemoteConn);
            }

            kChannel.HandleRecv(this.cache, 5, messageLength - 5);
        }

        public void Disconnect(uint localConn, uint remoteConn, int error, EndPoint address, int times)
        {
            try
            {
                if (this.Transport == null)
                {
                    return;
                }

                byte[] buffer = this.cache;
                buffer.WriteTo(0, KcpProtocalType.FIN);
                buffer.WriteTo(1, localConn);
                buffer.WriteTo(5, remoteConn);
                buffer.WriteTo(9, (uint) error);
                for (int i = 0; i < times; ++i)
                {
                    this.Transport.Send(buffer, 0, 13, address, ChannelType.Accept);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Disconnect error {localConn} {remoteConn} {error} {address} {e}");
            }
            
            Log.Info($"channel send fin: {localConn} {remoteConn} {address} {error}");
        }
    }
}
