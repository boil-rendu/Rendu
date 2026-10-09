using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>Opcode 单例：在 <see cref="Awake"/> 阶段遍历 <see cref="CodeTypes"/> 收集所有 <c>[Message]</c> 类，建立 <c>Type ↔ Opcode</c> 双向映射 + Request → Response 索引。</summary>
    /// <remarks>初始化后只读，主线程/网络线程可并发读。</remarks>
    public class OpcodeType: Singleton<OpcodeType>, ISingletonAwake
    {
        // 初始化后不变，所以主线程，网络线程都可以读
        private readonly DoubleMap<Type, ushort> typeOpcode = new();

        private readonly Dictionary<Type, Type> requestResponse = new();

        /// <summary>扫描 <see cref="CodeTypes"/> 收集所有 <c>[Message]</c> 类，建立 <c>Type ↔ Opcode</c> 双向索引；<c>[ResponseType]</c> 或 <see cref="ILocationMessage"/> 派生类则建 Request → Response 映射。</summary>
        public void Awake()
        {
            using var _ = Profiler.Once("OpcodeType.Awake");
            HashSet<Type> types = CodeTypes.Instance.GetTypes(typeof (MessageAttribute));
            foreach (Type type in types)
            {
                object[] att = type.GetCustomAttributes(typeof (MessageAttribute), false);
                if (att.Length == 0)
                {
                    continue;
                }

                MessageAttribute messageAttribute = att[0] as MessageAttribute;
                if (messageAttribute == null)
                {
                    continue;
                }

                ushort opcode = messageAttribute.Opcode;
                if (opcode != 0)
                {
                    this.typeOpcode.Add(type, opcode);
                }

                // 检查request response
                if (typeof (IRequest).IsAssignableFrom(type))
                {
                    if (typeof (ILocationMessage).IsAssignableFrom(type))
                    {
                        this.requestResponse.Add(type, typeof (MessageResponse));
                        continue;
                    }

                    var attrs = type.GetCustomAttributes(typeof (ResponseTypeAttribute), false);
                    if (attrs.Length == 0)
                    {
                        Log.Error($"not found responseType: {type}");
                        continue;
                    }

                    ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                    this.requestResponse.Add(type, CodeTypes.Instance.GetType($"RD.{responseTypeAttribute.Type}"));
                }
            }
        }

        /// <summary>按消息 <see cref="Type"/> 查 <c>Opcode</c>（找不到返回 <c>0</c>，由调用方决定是否抛异常）。</summary>
        public ushort GetOpcode(Type type)
        {
            return this.typeOpcode.GetValueByKey(type);
        }

        /// <summary>按 <c>Opcode</c> 查消息 <see cref="Type"/>；找不到抛 <see cref="Exception"/>。</summary>
        /// <param name="opcode">消息操作码（包头 2 字节）。</param>
        /// <exception cref="Exception">Opcode 未注册时抛异常。</exception>
        public Type GetType(ushort opcode)
        {
            Type type = this.typeOpcode.GetKeyByValue(opcode);
            if (type == null)
            {
                throw new Exception($"OpcodeType not found type: {opcode}");
            }
            return type;
        }

        /// <summary>按 Request <see cref="Type"/> 取对应的 Response <see cref="Type"/>；<see cref="ILocationMessage"/> 类型的请求统一返回 <see cref="MessageResponse"/>。</summary>
        /// <param name="request">请求类型（实现 <see cref="IRequest"/>）。</param>
        /// <exception cref="Exception">未注册 Request → Response 映射时抛异常。</exception>
        public Type GetResponseType(Type request)
        {
            if (!this.requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.FullName}");
            }

            return response;
        }
    }
}
