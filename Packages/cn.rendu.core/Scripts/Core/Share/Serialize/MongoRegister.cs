using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace RD.Core
{
    public static class MongoRegister
    {
        public static void RegisterStruct<T>() where T : struct
        {
            BsonSerializer.RegisterSerializer(typeof (T), new StructBsonSerialize<T>());
        }
        
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