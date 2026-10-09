using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace RD.Core
{
	/// <summary>
	/// <see cref="ComponentsCollection"/> 的 Bson 自定义序列化器：与 <see cref="BsonChildrenCollectionSerializer"/> 几乎相同，区别是 key 用 <see cref="Entity.GetLongHashCode"/>（组件按 long hash 索引）而非 <c>Id</c>。
	/// </summary>
	public class BsonComponentsCollectionSerializer: IBsonSerializer
	{
		/// <summary>反序列化：从 Bson 数组逐个出 <see cref="Entity"/>，按 <see cref="Entity.GetLongHashCode"/> 作 key 加入池化 <see cref="ComponentsCollection"/>。</summary>
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			ComponentsCollection componentsCollection = ComponentsCollection.Create(true);
			IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
			IBsonReader bsonReader = context.Reader;
			bsonReader.ReadStartArray();
			while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
			{
				Entity entity = bsonSerializer.Deserialize(context);
				entity.IsSerilizeWithParent = true;
				componentsCollection.Add(entity.GetLongHashCode(), entity);
			}
			bsonReader.ReadEndArray();

			return componentsCollection;
		}

		/// <summary>序列化：遍历 <see cref="ComponentsCollection"/>，仅写「<see cref="ISerializeToEntity"/> 标记」或「<see cref="Entity.IsSerilizeWithParent"/> = true」的组件。</summary>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			IBsonWriter bsonWriter = context.Writer;
			bsonWriter.WriteStartArray();
			ComponentsCollection componentsCollection = (ComponentsCollection)value;

			IBsonSerializer<Entity> bsonSerializer = BsonSerializer.LookupSerializer<Entity>();
			foreach ((long _, Entity entity) in componentsCollection)
			{
				if (entity is ISerializeToEntity || entity.IsSerilizeWithParent)
				{
					bsonSerializer.Serialize(context, entity);
				}
			}
			bsonWriter.WriteEndArray();
		}

		/// <inheritdoc />
		public System.Type ValueType => typeof(ComponentsCollection);
	}
}
