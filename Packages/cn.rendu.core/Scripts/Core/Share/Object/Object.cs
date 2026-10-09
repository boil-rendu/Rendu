using MongoDB.Bson;

namespace RD.Core
{
    /// <summary>
    /// 框架内所有领域对象（Entity、消息、单例基类等）的根类型：<c>[EnableClass]</c> 标记，供源生成器扫描时识别。
    /// 提供 BSON/JSON 序列化扩展 <see cref="ToJson"/>/<see cref="ToBson"/>；<see cref="ToString"/> 走 BSON ToString（不走 <c>MongoHelper</c> 避免单步调试时触发 <c>BeginInit</c>）。
    /// </summary>
    [EnableClass]
    public abstract class Object
    {
        /// <summary>
        /// 单步调试友好的 <c>ToString</c>：直接走 BSON 库的 <c>ToJson</c>，不调 <c>MongoHelper.ToJson</c>。
        /// 原因：<c>MongoHelper.ToJson</c> 内部会触发 <c>ISupportInitialize.BeginInit</c>，单步调试时 IDE 自动 <c>ToString</c> 会爆炸。
        /// </summary>
        public override string ToString()
        {
            // 这里不能用MongoHelper.ToJson，因为单步调试会调用ToString来显示数据
            // 如果MongoHelper.ToJson会调用BeginInit,就出大事了
            // return MongoHelper.ToJson(this);
            return ((object)this).ToJson();
        }

        /// <summary>序列化为 JSON 字符串（业务主动调用，非调试路径，可走 <c>MongoHelper.ToJson</c>）。</summary>
        public string ToJson()
        {
            return MongoHelper.ToJson(this);
        }

        /// <summary>序列化为 BSON 字节数组（用于 Mongo 存储 / 网络传输）。</summary>
        public byte[] ToBson()
        {
            return MongoHelper.Serialize(this);
        }
    }
}
