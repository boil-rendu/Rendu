namespace RD.Core
{
    /// <summary>
    /// 标记一个组件可序列化进 Entity 存档。
    /// 实现此接口的组件在父 Entity 序列化时由 <c>ISerializeSystem</c> 处理并写入字节流。
    /// </summary>
    public interface ISerializeToEntity
    {
    }
}
