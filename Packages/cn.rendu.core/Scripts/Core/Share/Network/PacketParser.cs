using System;
using System.IO;

namespace RD.Core
{
    /// <summary>包解析状态机：交替读「包长度」「包体」。</summary>
    public enum ParserState
    {
        /// <summary>正在读包长度（首 2 字节 = Outer / 首 4 字节 = Inner）。</summary>
        PacketSize,
        /// <summary>正在读包体（按上一阶段读到的长度）。</summary>
        PacketBody
    }

    /// <summary>包解析器（无 GC）：从 <see cref="CircularBuffer"/> 流式解出 <c>Opcode + Body</c>，Inner 4 字节长度头，Outer 2 字节长度头。</summary>
    /// <remarks>解析出一个完整包后回调 <see cref="AService.ReadCallback"/>；长度越界/过小会抛异常，调用方负责捕获并断开连接。</remarks>
    public class PacketParser
    {
        private readonly CircularBuffer buffer;
        private int packetSize;
        private ParserState state;
        private readonly AService service;
        private readonly byte[] cache = new byte[8];

        /// <summary>Inner 服务的包长度字段字节数（4 字节 int）。</summary>
        public const int InnerPacketSizeLength = 4;
        /// <summary>Outer 服务的包长度字段字节数（2 字节 ushort）。</summary>
        public const int OuterPacketSizeLength = 2;
        /// <summary>当前正在解析的包对应的 <see cref="MemoryBuffer"/>（在 <c>Parse</c> 返回 <c>true</c> 时有效）。</summary>
        public MemoryBuffer MemoryBuffer;

        public PacketParser(CircularBuffer buffer, AService service)
        {
            this.buffer = buffer;
            this.service = service;
        }

        /// <summary>从 <see cref="buffer"/> 解析一个完整包：成功返回 <c>true</c>，<see cref="MemoryBuffer"/> 承载包体；数据不足返回 <c>false</c>，下次继续。</summary>
        /// <param name="memoryBuffer">解析出的包体缓冲；返回 <c>false</c> 时为 <c>null</c>。</param>
        /// <exception cref="Exception">包长度越界（&lt; <see cref="Packet.MinPacketSize"/> 或 &gt; <c>ushort.MaxValue * 16</c>）抛异常，可能是外网探测端口。</exception>
        public bool Parse(out MemoryBuffer memoryBuffer)
        {
            while (true)
            {
                switch (this.state)
                {
                    case ParserState.PacketSize:
                    {
                        if (this.service.ServiceType == ServiceType.Inner)
                        {
                            if (this.buffer.Length < InnerPacketSizeLength)
                            {
                                memoryBuffer = null;
                                return false;
                            }

                            // 前面已校验 buffer.Length >= InnerPacketSizeLength，Read 必然读满
                        _ = this.buffer.Read(this.cache, 0, InnerPacketSizeLength);

                            this.packetSize = BitConverter.ToInt32(this.cache, 0);
                            if (this.packetSize > ushort.MaxValue * 16 || this.packetSize < Packet.MinPacketSize)
                            {
                                throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
                            }
                        }
                        else
                        {
                            if (this.buffer.Length < OuterPacketSizeLength)
                            {
                                memoryBuffer = null;
                                return false;
                            }

                            // 前面已校验 buffer.Length >= OuterPacketSizeLength，Read 必然读满
                        _ = this.buffer.Read(this.cache, 0, OuterPacketSizeLength);

                            this.packetSize = BitConverter.ToUInt16(this.cache, 0);
                            if (this.packetSize < Packet.MinPacketSize)
                            {
                                throw new Exception($"recv packet size error, 可能是外网探测端口: {this.packetSize}");
                            }
                        }

                        this.state = ParserState.PacketBody;
                        break;
                    }
                    case ParserState.PacketBody:
                    {
                        if (this.buffer.Length < this.packetSize)
                        {
                            memoryBuffer = null;
                            return false;
                        }

                        memoryBuffer = this.service.Fetch(this.packetSize);
                        this.buffer.Read(memoryBuffer, this.packetSize);
                        //memoryStream.SetLength(this.packetSize - Packet.MessageIndex);

                        memoryBuffer.Seek(0, SeekOrigin.Begin);

                        this.state = ParserState.PacketSize;
                        return true;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
