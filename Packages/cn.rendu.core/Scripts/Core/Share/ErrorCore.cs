namespace RD.Core
{
    /// <summary>
    /// 框架内置错误码。
    ///
    /// 数值区间约定（各区间之间留有空档，便于后续追加）：
    ///   0              —— 成功
    ///   100000–109999  —— 网络层：KCP / Socket / TChannel
    ///   110000 及以上   —— 业务层：供使用方自定义，框架不再占用
    ///
    /// 本类标注了 [UniqueId]，由源码生成器在编译期校验错误码唯一性：
    /// 重复定义会直接报错，无需人工核对。
    ///
    /// 使用者如需扩展业务错误码，建议在本项目之外另建 partial 类：
    /// <code>
    /// namespace RD.Core
    /// {
    ///     public static partial class ErrorCore
    ///     {
    ///         public const int ERR_MyLoginFailed = 110001;
    ///     }
    /// }
    /// </code>
    /// 这样可在不修改本文件的前提下扩展，避免升级框架时产生冲突。
    /// </summary>
    [UniqueId]
    public static partial class ErrorCore
    {
        /// <summary>操作成功。</summary>
        public const int ERR_Success = 0;

        #region 网络层 — KCP / Socket

        /// <summary>KCP 连接超时：连接握手在限定时间内未完成。通常可重试。</summary>
        public const int ERR_KcpConnectTimeout = 100205;

        /// <summary>KCP 接受连接超时：服务端等待客户端握手超时。连接应关闭。</summary>
        public const int ERR_KcpAcceptTimeout = 100206;

        /// <summary>KCP 读写超时：底层 Socket 在限定时间内无数据收发。连接已不可用。</summary>
        public const int ERR_KcpReadWriteTimeout = 100207;

        /// <summary>对端主动断开连接。属正常的连接终止，通常无需报错。</summary>
        public const int ERR_PeerDisconnect = 100208;

        /// <summary>Socket 无法发送：发送缓冲区满或连接已关闭。可重试或断开。</summary>
        public const int ERR_SocketCantSend = 100209;

        /// <summary>Socket 底层错误：具体原因见 System.Net.Sockets.SocketError。连接应重建。</summary>
        public const int ERR_SocketError = 100210;

        /// <summary>KCP 待发送数据超过上限。发送方应分片或降频，否则丢弃。</summary>
        public const int ERR_KcpWaitSendSizeTooLarge = 100211;

        /// <summary>KCP 创建失败。属初始化错误，非运行时可恢复错误。</summary>
        public const int ERR_KcpCreateError = 100212;

        /// <summary>发送消息时找不到对应的 TChannel：目标连接不存在或已销毁。</summary>
        public const int ERR_SendMessageNotFoundTChannel = 100213;

        /// <summary>TChannel 接收数据出错：底层读取失败，连接应关闭。</summary>
        public const int ERR_TChannelRecvError = 100214;

        /// <summary>消息包解析失败：数据不完整或格式错误。连接状态可能已错乱，建议断开重连。</summary>
        public const int ERR_MessageSocketParserError = 100215;

        /// <summary>按 ChannelId 找不到 KCP Channel：连接可能已被回收。</summary>
        public const int ERR_KcpNotFoundChannel = 100216;

        /// <summary>WebSocket 发送失败。当前仓库未包含 WebSocket 实现，为预留错误码。</summary>
        public const int ERR_WebsocketSendError = 100217;

        /// <summary>WebSocket 连接被对端重置。当前仓库未包含 WebSocket 实现，为预留错误码。</summary>
        public const int ERR_WebsocketPeerReset = 100218;

        /// <summary>WebSocket 消息超出大小上限。当前仓库未包含 WebSocket 实现，为预留错误码。</summary>
        public const int ERR_WebsocketMessageTooBig = 100219;

        /// <summary>WebSocket 接收出错。当前仓库未包含 WebSocket 实现，为预留错误码。</summary>
        public const int ERR_WebsocketRecvError = 100220;

        /// <summary>KCP 分包读取到不一致的数据：拆包长度与实际不符。数据可能已损坏。</summary>
        public const int ERR_KcpReadNotSame = 100230;

        /// <summary>KCP 拆包失败：无法从缓冲区还原完整消息。</summary>
        public const int ERR_KcpSplitError = 100231;

        /// <summary>KCP 拆包数量异常：分包计数与预期不符。</summary>
        public const int ERR_KcpSplitCountError = 100232;

        #endregion

        #region 序列化 / 协议

        /// <summary>网络包解析失败：协议格式不匹配或数据损坏。连接建议断开。</summary>
        public const int ERR_PacketParserError = 110005;

        /// <summary>WebSocket 连接失败。当前仓库未包含 WebSocket 实现，为预留错误码。</summary>
        public const int ERR_WebsocketConnectError = 110304;

        #endregion
    }
}
