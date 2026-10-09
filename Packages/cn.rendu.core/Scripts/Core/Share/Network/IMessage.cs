namespace RD.Core
{
    // 不需要返回消息
    /// <summary>网络消息的根标记接口：所有 <c>[Message]</c> 类必须实现（直接实现 = OneWay，间接实现 <see cref="IRequest"/> / <see cref="IResponse"/> = RPC 模式）。</summary>
    public interface IMessage
    {
    }

    /// <summary>带 RpcId 的请求消息：配合 <see cref="IResponse"/> 形成 RPC 语义，由 <see cref="OpcodeType"/> 建 Request → Response 索引。</summary>
    public interface IRequest: IMessage
    {
        /// <summary>RPC 调用序号：用于在大量并发 RPC 时把响应匹配回调用方（调用方发请求时自增，响应回填同一 RpcId）。</summary>
        int RpcId
        {
            get;
            set;
        }
    }

    /// <summary>RPC 响应消息：携带 <see cref="Error"/>（错误码 0 = 成功）、<see cref="Message"/>（错误描述）、<see cref="RpcId"/>（与请求配对）。</summary>
    public interface IResponse: IMessage
    {
        /// <summary>错误码：0 表示成功，非 0 查 <c>ErrorCore</c>。</summary>
        int Error
        {
            get;
            set;
        }

        /// <summary>错误描述（仅在 <see cref="Error"/> 非 0 时有意义，便于日志和客户端提示）。</summary>
        string Message
        {
            get;
            set;
        }

        /// <summary>回填对应请求的 <see cref="IRequest.RpcId"/>，由调用方匹配响应归属。</summary>
        int RpcId
        {
            get;
            set;
        }
    }
}
