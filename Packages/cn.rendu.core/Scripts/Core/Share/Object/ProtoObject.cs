using System;
using System.ComponentModel;

namespace RD.Core
{
    /// <summary>
    /// 可被 Bson 克隆的 <see cref="Object"/>：<see cref="Object"/> + <see cref="ISupportInitialize"/>，提供 <see cref="Clone"/>（走 Bson 字节中转）。
    /// 派生：<see cref="MessageObject"/>。
    /// </summary>
    public abstract class ProtoObject: Object, ISupportInitialize
    {
        /// <summary>深拷贝：把 <c>this</c> 序列化为 Bson 再反序列化为同类型新实例。开销较大（需序列化），用于「需要完全独立副本」的场景。</summary>
        public object Clone()
        {
            byte[] bytes = MongoHelper.Serialize(this);
            return MongoHelper.Deserialize(this.GetType(), bytes, 0, bytes.Length);
        }

        /// <summary><see cref="ISupportInitialize.BeginInit"/>：Bson 反序列化开始钩子，默认空实现。</summary>
        public virtual void BeginInit()
        {
        }


        /// <summary><see cref="ISupportInitialize.EndInit"/>：Bson 反序列化结束钩子，默认空实现。</summary>
        public virtual void EndInit()
        {
        }
    }
}
