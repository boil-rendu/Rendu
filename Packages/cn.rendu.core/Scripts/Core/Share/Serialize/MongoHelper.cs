using System;
using System.ComponentModel;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace RD.Core
{
	/// <summary>
	/// Bson/Json 序列化门面：把 MongoDB.Driver 的 <see cref="BsonSerializer"/> 包成可重入的 <c>try/catch</c> 版本，并自动调 <see cref="ISupportInitialize.BeginInit"/> 钩子。
	/// 比直接调 <see cref="BsonSerializer"/> 多出：异常信息带上类型名/字节数/位置（便于排查），并对所有 <see cref="ISupportInitialize"/> 实现类触发 <c>BeginInit</c>。
	/// </summary>
	public static class MongoHelper
	{
		[StaticField]
		private static readonly JsonWriterSettings defaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };

		/// <summary>把 <paramref name="obj"/> 序列化为 JSON 字符串：默认输出 <see cref="JsonOutputMode.RelaxedExtendedJson"/>（人类可读、$oid/$date 等扩展）。</summary>
		public static string ToJson(object obj)
		{
			try
			{
				if (obj is ISupportInitialize supportInitialize)
				{
					supportInitialize.BeginInit();
				}
				return obj.ToJson(defaultSettings);
			}
			catch (Exception e)
			{
				throw new Exception($"to json error {obj.GetType().FullName}\n{e}");
			}
		}

		/// <summary>同 <see cref="ToJson(object)"/> 但用自定义 <paramref name="settings"/>（如 <c>OutputMode = JsonOutputMode.CanonicalExtendedJson</c>、缩进等）。</summary>
		public static string ToJson(object obj, JsonWriterSettings settings)
		{
			try
			{
				if (obj is ISupportInitialize supportInitialize)
				{
					supportInitialize.BeginInit();
				}
				return obj.ToJson(settings);
			}
			catch (Exception e)
			{
				throw new Exception($"to json error {obj.GetType().FullName}\n{e}");
			}
		}

		/// <summary>JSON 字符串反序列化为 <typeparamref name="T"/>：失败抛出带原始 JSON 的异常。</summary>
		/// <typeparam name="T">目标类型（必须已被 <see cref="MongoRegister.Init"/> 注册或为框架基类）。</typeparam>
		public static T FromJson<T>(string str)
		{
			try
			{
				return BsonSerializer.Deserialize<T>(str);
			}
			catch (Exception e)
			{
				throw new Exception($"from json error: {str}\n{e}");
			}
		}

		/// <summary>非泛型 JSON 反序列化（用于动态 <see cref="Type"/> 场景，如配置表按类型字段读表）。</summary>
		public static object FromJson(Type type, string str)
		{
			try
			{
				return BsonSerializer.Deserialize(str, type);
			}
			catch (Exception e)
			{
				throw new Exception($"from json error: {str}\n{e}");
			}
		}

		/// <summary>把 <paramref name="obj"/> 序列化为 Bson 字节数组：自动触发 <see cref="ISupportInitialize.BeginInit"/>。</summary>
		public static byte[] Serialize(object obj)
		{
			try
			{
				if (obj is ISupportInitialize supportInitialize)
				{
					supportInitialize.BeginInit();
				}
				return obj.ToBson();
			}
			catch (Exception e)
			{
				throw new Exception($"Serialize error {obj.GetType().FullName}\n{e}");
			}
		}

		/// <summary>把 <paramref name="message"/> 以 <see cref="object"/> 名义类型直接写到 <paramref name="stream"/>：跳过 <c>ToBson</c> 的多态 classmap 推断，<c>MongoRegister.Init</c> 已注册 <c>ObjectSerializer(AllAllowedTypes)</c> 才能正常序列化任意消息。</summary>
		public static void Serialize(object message, MemoryStream stream)
		{
			try
			{
				if (message is ISupportInitialize supportInitialize)
				{
					supportInitialize.BeginInit();
				}

				using BsonBinaryWriter bsonWriter = new(stream, BsonBinaryWriterSettings.Defaults);

				BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
				BsonSerializationArgs args = default;
				args.NominalType = typeof (object);
				IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
				serializer.Serialize(context, args, message);
			}
			catch (Exception e)
			{
				throw new Exception($"Serialize error {message.GetType().FullName}\n{e}");
			}
		}

		/// <summary>从 <paramref name="bytes"/> 全段反序列化为 <paramref name="type"/>。</summary>
		public static object Deserialize(Type type, byte[] bytes)
		{
			try
			{
				return BsonSerializer.Deserialize(bytes, type);
			}
			catch (Exception e)
			{
				throw new Exception($"from bson error: {type.FullName} {bytes.Length}", e);
			}
		}

		/// <summary>从 <c>bytes[index..index+count)</c> 子段反序列化（避免 MemoryStream 重新分配整段）。</summary>
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			try
			{
				using MemoryStream memoryStream = new(bytes, index, count);

				return BsonSerializer.Deserialize(memoryStream, type);
			}
			catch (Exception e)
			{
				throw new Exception($"from bson error: {type.FullName} {bytes.Length} {index} {count}", e);
			}
		}

		/// <summary>从 <see cref="Stream"/> 反序列化（流位置保持在序列化数据之后）。</summary>
		public static object Deserialize(Type type, Stream stream)
		{
			try
			{
				return BsonSerializer.Deserialize(stream, type);
			}
			catch (Exception e)
			{
				throw new Exception($"from bson error: {type.FullName} {stream.Position} {stream.Length}", e);
			}
		}

		/// <summary>泛型版全段反序列化（薄封装，包装异常信息）。</summary>
		public static T Deserialize<T>(byte[] bytes)
		{
			try
			{
				using MemoryStream memoryStream = new(bytes);

				return (T)BsonSerializer.Deserialize(memoryStream, typeof (T));
			}
			catch (Exception e)
			{
				throw new Exception($"from bson error: {typeof (T).FullName} {bytes.Length}", e);
			}
		}

		/// <summary>泛型版子段反序列化。</summary>
		public static T Deserialize<T>(byte[] bytes, int index, int count)
		{
			return (T)Deserialize(typeof (T), bytes, index, count);
		}

		/// <summary>克隆：<c>Serialize(t) → Deserialize&lt;T&gt;</c>，用于深拷贝（业务侧优先用 <see cref="ProtoObject.Clone"/>，性能更好）。</summary>
		public static T Clone<T>(T t)
		{
			return Deserialize<T>(Serialize(t));
		}
	}
}
