using System;
using System.IO;

namespace RD.Core
{
    /// <summary>消息序列化/反序列化辅助：基于 <see cref="MemoryPackHelper"/>，按 Inner/Outer 两种 <see cref="ServiceType"/> 写入/读出包头（Opcode + ActorId）。</summary>
    public static class MessageSerializeHelper
    {
        /// <summary>把 <see cref="MessageObject"/> 序列化为字节数组（分配新缓冲）。</summary>
        public static byte[] Serialize(MessageObject message)
        {
            return MemoryPackHelper.Serialize(message);
        }

        /// <summary>把 <see cref="MessageObject"/> 序列化到已有 <see cref="MemoryBuffer"/>（<paramref name="stream"/> 当前位置之后）。</summary>
        public static void Serialize(MessageObject message, MemoryBuffer stream)
        {
            MemoryPackHelper.Serialize(message, stream);
        }

        /// <summary>从字节段反序列化为 <see cref="MessageObject"/>：从 <see cref="ObjectPool"/> 取出实例并填充，避免每次分配。</summary>
        /// <param name="type">目标消息类型（通常由 <see cref="OpcodeType.GetType"/> 解析得到）。</param>
        /// <param name="bytes">字节数组。</param>
        /// <param name="index">起始偏移。</param>
        /// <param name="count">长度。</param>
        public static MessageObject Deserialize(Type type, byte[] bytes, int index, int count)
        {
            object o = ObjectPool.Fetch(type);
            MemoryPackHelper.Deserialize(type, bytes, index, count, ref o);
            return o as MessageObject;
        }

        /// <summary>从 <see cref="MemoryBuffer"/> 反序列化为 <see cref="MessageObject"/>（池化实例）。</summary>
        public static MessageObject Deserialize(Type type, MemoryBuffer stream)
        {
            object o = ObjectPool.Fetch(type);
            MemoryPackHelper.Deserialize(type, stream, ref o);
            return o as MessageObject;
        }

        /// <summary>把 <paramref name="message"/> 序列化为 <see cref="MemoryBuffer"/> 内的完整包（含 Opcode 头）。</summary>
        /// <param name="stream">目标缓冲；写入位置 = <c>headOffset + OpcodeLength</c>。</param>
        /// <param name="message">要序列化的消息实例。</param>
        /// <param name="headOffset">包头偏移量（Inner 服务需为 <see cref="Packet.ActorIdLength"/> 给 ActorId 留位置；Outer = 0）。</param>
        /// <returns>写出的 Opcode，供调用方继续填充包头。</returns>
        public static ushort MessageToStream(MemoryBuffer stream, MessageObject message, int headOffset = 0)
        {
            ushort opcode = OpcodeType.Instance.GetOpcode(message.GetType());

            stream.Seek(headOffset + Packet.OpcodeLength, SeekOrigin.Begin);
            stream.SetLength(headOffset + Packet.OpcodeLength);

            stream.GetBuffer().WriteTo(headOffset, opcode);

            MessageSerializeHelper.Serialize(message, stream);

            stream.Seek(0, SeekOrigin.Begin);
            return opcode;
        }

        /// <summary>把 <paramref name="message"/> 按 <see cref="AService.ServiceType"/> 打包为可发送的 <see cref="MemoryBuffer"/>：Inner 写入 <c>ActorId + Opcode + Body</c>，Outer 写入 <c>Opcode + Body</c>。</summary>
        /// <returns>(opcode, memoryBuffer) 供 <see cref="AService.Send"/> 进一步包 <c>PacketSize</c> 头。</returns>
        public static (ushort, MemoryBuffer) ToMemoryBuffer(AService service, ActorId actorId, object message)
        {
            MemoryBuffer memoryBuffer = service.Fetch();
            ushort opcode = 0;
            switch (service.ServiceType)
            {
                case ServiceType.Inner:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message, Packet.ActorIdLength);
                    memoryBuffer.GetBuffer().WriteTo(0, actorId);
                    break;
                }
                case ServiceType.Outer:
                {
                    opcode = MessageToStream(memoryBuffer, (MessageObject)message);
                    break;
                }
            }

            return (opcode, memoryBuffer);
        }

        /// <summary>从已剥去 <c>PacketSize</c> 头的 <see cref="MemoryBuffer"/> 解析出 (ActorId, message)：按 <see cref="AService.ServiceType"/> 反序列化对应包头 + 消息体。</summary>
        public static (ActorId, object) ToMessage(AService service, MemoryBuffer memoryStream)
        {
            object message = null;
            ActorId actorId = default;
            switch (service.ServiceType)
            {
                case ServiceType.Outer:
                {
                    memoryStream.Seek(Packet.OpcodeLength, SeekOrigin.Begin);
                    ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), 0);
                    Type type = OpcodeType.Instance.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
                case ServiceType.Inner:
                {
                    memoryStream.Seek(Packet.ActorIdLength + Packet.OpcodeLength, SeekOrigin.Begin);
                    byte[] buffer = memoryStream.GetBuffer();
                    actorId.Process = BitConverter.ToInt32(buffer, Packet.ActorIdIndex);
                    actorId.Fiber = BitConverter.ToInt32(buffer, Packet.ActorIdIndex + 4);
                    actorId.InstanceId = BitConverter.ToInt64(buffer, Packet.ActorIdIndex + 8);
                    ushort opcode = BitConverter.ToUInt16(buffer, Packet.ActorIdLength);
                    Type type = OpcodeType.Instance.GetType(opcode);
                    message = Deserialize(type, memoryStream);
                    break;
                }
            }

            return (actorId, message);
        }
    }
}
