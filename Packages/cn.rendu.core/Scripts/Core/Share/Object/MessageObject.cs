using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
    /// <summary>
    /// 网络消息基类：<see cref="ProtoObject"/> + <see cref="IMessage"/> + <see cref="IPool"/>，<c>[DisableNew]</c> 禁止业务侧 <c>new</c>（必须从 <see cref="ObjectPool"/> 取）。
    /// 由 <see cref="MessageSerializeHelper.Deserialize"/> 反序列化时从池中取实例填充，避免每次收发消息都分配对象。
    /// </summary>
    [DisableNew]
    public abstract class MessageObject: ProtoObject, IMessage, IPool
    {
        /// <summary>释放：默认空实现（池化实例由 <see cref="ObjectPool.Recycle"/> 回收，不走真正的 <c>Dispose</c>）。</summary>
        public virtual void Dispose()
        {
        }

        /// <summary>是否来自 <see cref="ObjectPool"/>：<see cref="MessageSerializeHelper.Deserialize"/> 在反序列化前先 <c>ObjectPool.Fetch</c> 拿到实例再填充，反序列化后置 <c>true</c>。</summary>
        [BsonIgnore]
        public bool IsFromPool { get; set; }
    }
}
