using System;

namespace RD.Core
{
    /// <summary>
    /// 标记 <see cref="IRequest"/> 派生类的响应类型（<see cref="IResponse"/> 派生）：<see cref="OpcodeType.Awake"/> 阶段据
    /// <see cref="Type"/> 字符串（约定 <c>RD.</c> 前缀，从 <see cref="CodeTypes"/> 全名表查）建立 Request → Response 索引。
    /// 对于实现 <see cref="ILocationMessage"/> 的请求，默认指向 <see cref="MessageResponse"/> 而非此特性。
    /// </summary>
    public class ResponseTypeAttribute: BaseAttribute
    {
        /// <summary>响应类型的全名（约定 <c>RD.</c> 前缀，配合 <see cref="CodeTypes"/> 全名索引查表）。</summary>
        public string Type { get; }

        public ResponseTypeAttribute(string type)
        {
            this.Type = type;
        }
    }
}
