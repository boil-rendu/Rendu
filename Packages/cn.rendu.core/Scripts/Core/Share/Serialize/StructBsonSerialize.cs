using System;
using System.Reflection;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
	/// <summary>
	/// struct 通用 Bson 序列化器：把任意 <c>struct</c> 的「带 <c>[BsonElement]</c> 特性」或「public」字段按字段名写 Bson 文档；反序列化时按字段名匹配 + 反射 <see cref="FieldInfo.SetValue(object, object)"/> 写回。
	/// 用于 Bson 默认无法处理的 struct（如 <c>float2</c>、<c>FP</c>、<c>TSVector</c>），通过 <see cref="MongoRegister.RegisterStruct{T}"/> 注册。
	/// </summary>
	public class StructBsonSerialize<TValue>: StructSerializerBase<TValue> where TValue : struct
	{
		/// <summary>序列化：枚举 <typeparamref name="TValue"/> 的所有实例字段（含非 public），跳过「非 public 且无 <c>[BsonElement]</c>」的字段，按字段名写 Bson 文档。</summary>
		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
		{
			Type nominalType = args.NominalType;

			IBsonWriter bsonWriter = context.Writer;

			bsonWriter.WriteStartDocument();

			FieldInfo[] fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				BsonElementAttribute bsonElement = field.GetCustomAttribute<BsonElementAttribute>();
				if (bsonElement == null && !field.IsPublic)
				{
					continue;
				}
				bsonWriter.WriteName(field.Name);
				BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
			}

			bsonWriter.WriteEndDocument();
		}

		/// <summary>反序列化：构造默认值 <c>new TValue()</c>（box 出来才能 <see cref="FieldInfo.SetValue(object, object)"/>），按 Bson 文档名查找字段并 SetValue；找不到的字段被 <see cref="IBsonReader.SkipValue"/> 跳过。</summary>
		public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			//boxing is required for SetValue to work
			object obj = new TValue();
			Type actualType = args.NominalType;
			IBsonReader bsonReader = context.Reader;

			bsonReader.ReadStartDocument();

			while (bsonReader.State != BsonReaderState.EndOfDocument)
			{
				switch (bsonReader.State)
				{
					case BsonReaderState.Name:
					{
						string name = bsonReader.ReadName(Utf8NameDecoder.Instance);
						FieldInfo field = actualType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (field != null)
						{
							object value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
							field.SetValue(obj, value);
						}

						break;
					}
					case BsonReaderState.Type:
					{
						bsonReader.ReadBsonType();
						break;
					}
					case BsonReaderState.Value:
					{
						bsonReader.SkipValue();
						break;
					}
				}
			}

			bsonReader.ReadEndDocument();

			return (TValue)obj;
		}
	}
}
