using MemoryPack;

namespace RD.Core
{
    /// <summary>通用响应消息：<see cref="ILocationMessage"/> 派生请求的默认响应类型（由 <see cref="OpcodeType.Awake"/> 自动关联）。</summary>
    /// <remarks>固定 <c>Opcode = ushort.MaxValue</c>，MemoryPack 序列化按 <see cref="MemoryPackOrder"/> 顺序。</remarks>
    [Message(ushort.MaxValue)]
    [MemoryPackable]
    public partial class MessageResponse: MessageObject, IResponse
    {
        /// <summary>RPC 调用序号，与对应请求的 <see cref="IRequest.RpcId"/> 一致。</summary>
        [MemoryPackOrder(1)]
        public int RpcId { get; set; }
        /// <summary>错误码，0 = 成功。</summary>
        [MemoryPackOrder(2)]
        public int Error { get; set; }
        /// <summary>错误描述（仅 Error 非 0 时有意义）。</summary>
        [MemoryPackOrder(3)]
        public string Message { get; set; }
    }
}
