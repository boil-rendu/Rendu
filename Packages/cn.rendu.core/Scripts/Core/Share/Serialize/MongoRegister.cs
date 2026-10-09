using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace RD.Core
{
	/// <summary>
	/// MongoDB.Driver Bson 序列化器注册门面：注册自定义集合 serializer、清理热更残留、注册全局 <c>IgnoreExtraElements</c> 约定、预热 <see cref="Object"/> 派生类的 classmap。
	/// 在 ET 框架启动早期（<see cref="CodeTypes.Awake"/> 之后）调一次。
	/// </summary>
	public static class MongoRegister
	{
		/// <summary>把 <typeparamref name="T"/>（struct）注册为 <see cref="StructBsonSerialize{T}"/>：用于 Bson 默认无法直接处理的 struct（如 Unity.Mathematics <c>float2</c>）。</summary>
		public static void RegisterStruct<T>() where T : struct
		{
			BsonSerializer.RegisterSerializer(typeof (T), new StructBsonSerialize<T>());
		}

		/// <summary>
		/// 一次性注册所有必要项：清 Bson 内部缓存 + 注册两个集合 serializer + 注册 <c>ObjectSerializer(AllAllowedTypes)</c> 允许任意对象 + 注册 <c>IgnoreExtraElements</c> 约定 + 预热 <see cref="Object"/> 派生 classmap。
		/// </summary>
		public static void Init()
		{
			// 清理老的数据: 重建序列化器注册表, 清掉热更前次注册残留
			typeof(BsonSerializer).GetMethod("CreateSerializerRegistry", BindingFlags.Static | BindingFlags.NonPublic)
				?.Invoke(null, Array.Empty<object>());
			typeof(BsonSerializer).GetMethod("RegisterIdGenerators", BindingFlags.Static | BindingFlags.NonPublic)
				?.Invoke(null, Array.Empty<object>());

			BsonSerializer.RegisterSerializer(typeof(ComponentsCollection), new BsonComponentsCollectionSerializer());
			BsonSerializer.RegisterSerializer(typeof(ChildrenCollection), new BsonChildrenCollectionSerializer());

			// MongoHelper.ToJson/Serialize 以 object 为名义类型序列化任意消息对象(如 HttpGetRouterResponse)。
			// MongoDB.Bson 3.x 起 ObjectSerializer 默认只放行框架安全类型, 必须显式放开全类型,
			// 否则运行期抛 "Type ... is not configured as a type that is allowed to be serialized"。
			BsonSerializer.RegisterSerializer(typeof(object), new ObjectSerializer(ObjectSerializer.AllAllowedTypes));


			// 自动注册IgnoreExtraElements
			ConventionPack conventionPack = new() { new IgnoreExtraElementsConvention(true) };

			ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

			//RegisterStruct<float2>();
			//RegisterStruct<float3>();
			//RegisterStruct<float4>();
			//RegisterStruct<quaternion>();
			//RegisterStruct<FP>();
			//RegisterStruct<TSVector>();
			//RegisterStruct<TSVector2>();
			//RegisterStruct<TSVector4>();
			//RegisterStruct<TSQuaternion>();
			//RegisterStruct<LSInput>();

			Dictionary<string, Type> types = CodeTypes.Instance.GetTypes();
			foreach (Type type in types.Values)
			{
				if (!type.IsSubclassOf(typeof (Object)))
				{
					continue;
				}

				if (type.IsGenericType)
				{
					continue;
				}

				BsonClassMap.LookupClassMap(type);
			}
		}
	}
}
