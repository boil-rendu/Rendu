namespace RD.Core
{
    /// <summary>位置消息标记接口：实现此接口的 <see cref="IRequest"/> 默认响应类型为 <see cref="MessageResponse"/>（由 <see cref="OpcodeType.Awake"/> 识别，不再需要 <c>[ResponseType]</c>）。</summary>
    /// <remarks>「位置消息」= 寻址到具体 <c>Location</c> 组件的消息（不关心 Entity Type），统一由 <see cref="MessageResponse"/> 回执。</remarks>
    public interface ILocationMessage: ILocationRequest
    {
    }

    /// <summary>位置请求接口：实现 <see cref="IRequest"/> 的位置消息专用版本，业务侧用 <see cref="ILocationMessage"/> 即可。</summary>
    public interface ILocationRequest: IRequest
    {
    }

    /// <summary>位置响应接口：通常由 <see cref="MessageResponse"/> 满足，业务侧极少自定义。</summary>
    public interface ILocationResponse: IResponse
    {
    }
}
