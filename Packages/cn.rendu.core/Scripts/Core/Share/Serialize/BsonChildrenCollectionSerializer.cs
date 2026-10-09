using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace RD.Core
{
	/// <summary>
	/// <see cref="ChildrenCollection"/> 的 Bson 自定义序列化器：序列化时写为 <c>Entity</c> 数组（仅含 <see cref="ISerializeToEntity"/> 标记或 <see cref="Entity.IsSerilizeWithParent"/> = true 的元素），反序列化时回填到 <see cref="ChildrenCollection"/> 实例（用 <c>Create(true)</c> 池化）。
	/// </summary>
	public class BsonChildrenCollectionSerializer: IBsonSerializer
	{
		/// <summary>反序列化：从 Bson 数组逐个 <see cref="IBsonSerializer{T}.Deserialize"/> 出 <see cref="Entity"/>，置 <see cref="Entity.IsSerilizeWithParent"/> = true 后按 <c>Id</c> 加入池化 <see cref="ChildrenCollection"/>。</summary>
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			ChildrenCollection childrenCollection = ChildrenCollection.Create(true);
			IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
			var bsonReader = context.Reader;
			bsonReader.ReadStartArray();
			while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
			{
				Entity entity = bsonSerializer.Deserialize(context);
				entity.IsSerilizeWithParent = true;
				childrenCollection.Add(entity.Id, entity);
			}
			bsonReader.ReadEndArray();

			return childrenCollection;
		}

		/// <summary>序列化：遍历 <see cref="ChildrenCollection"/>，仅写「<see cref="ISerializeToEntity"/> 标记」或「<see cref="Entity.IsSerilizeWithParent"/> = true」的子实体（按 <c>Entity</c> 多态 serializer 写）。</summary>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			IBsonWriter bsonWriter = context.Writer;
			bsonWriter.WriteStartArray();
			ChildrenCollection childrenCollection = (ChildrenCollection)value;

			IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
			foreach ((long _, Entity entity) in childrenCollection)
			{
				if (entity is ISerializeToEntity || entity.IsSerilizeWithParent)
				{
					bsonSerializer.Serialize(context, entity);
				}
			}
			bsonWriter.WriteEndArray();
		}

		/// <inheritdoc />
		public System.Type ValueType => typeof(ChildrenCollection);
	}
}
