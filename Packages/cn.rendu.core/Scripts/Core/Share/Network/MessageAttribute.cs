namespace RD.Core
{
    /// <summary>
    /// 标记一个网络消息类（<see cref="IMessage"/> 派生）的 <c>Opcode</c>：在 <see cref="OpcodeType.Awake"/> 阶段被收集为
    /// <c>Type ↔ ushort Opcode</c> 双向索引，供 <see cref="MessageSerializeHelper.MessageToStream"/> 写包头、
    /// <see cref="MessageSerializeHelper.ToMessage"/> 解包头时定位消息类型用。
    /// </summary>
    public class MessageAttribute: BaseAttribute
    {
        /// <summary>消息操作码（<c>ushort</c>）；与 <see cref="OpcodeRangeDefine"/> 配合按内外网划分区间。</summary>
        public ushort Opcode
        {
            get;
        }

        public MessageAttribute(ushort opcode = 0)
        {
            this.Opcode = opcode;
        }
    }
}
