namespace RD.Core
{
    /// <summary>Unit 侧的传送组件标记：实现此接口的组件需要在 Unit 跨进程/跨服传送时序列化并恢复（用于跨进程转移）。</summary>
    // Unit的组件有这个接口说明需要传送
    public interface ITransfer
    {
    }
}
