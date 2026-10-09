using System;
using System.ComponentModel;
using MemoryPack;

namespace RD.Core
{
	/// <summary>
	/// MemoryPack 序列化门面：薄封装 <see cref="MemoryPackSerializer"/>，对 <see cref="ISupportInitialize"/> 实现类自动调 <c>BeginInit/EndInit</c> 钩子。
	/// 与 <see cref="MongoHelper"/> 并列使用：网络层优先 MemoryPack（快），存档/配置层优先 Bson（可读）。
	/// </summary>
	public static class MemoryPackHelper
	{
		/// <summary>把 <paramref name="message"/> 序列化为 MemoryPack 字节：自动按 runtime type 注册 code path 调 <see cref="MemoryPackSerializer.Serialize(Type, object)"/>，并触发 <see cref="ISupportInitialize.BeginInit"/>。</summary>
		public static byte[] Serialize(object message)
		{
			if (message is ISupportInitialize supportInitialize)
			{
				supportInitialize.BeginInit();
			}
			return MemoryPackSerializer.Serialize(message.GetType(), message);
		}

		/// <summary>直接写到 <see cref="MemoryBuffer"/>（网络包路径，避免 <c>byte[]</c> 中转分配）。</summary>
		public static void Serialize(object message, MemoryBuffer stream)
		{
			if (message is ISupportInitialize supportInitialize)
			{
				supportInitialize.BeginInit();
			}
			MemoryPackSerializer.Serialize(message.GetType(), stream, message);
		}

		/// <summary>从 <c>bytes[index..index+count)</c> 反序列化为 <paramref name="type"/>：<see cref="MemoryPackSerializer.Deserialize(Type, ReadOnlySpan{byte})"/> 后触发 <see cref="ISupportInitialize.EndInit"/>。</summary>
		public static object Deserialize(Type type, byte[] bytes, int index, int count)
		{
			object o = MemoryPackSerializer.Deserialize(type, bytes.AsSpan(index, count));
			if (o is ISupportInitialize supportInitialize)
			{
				supportInitialize.EndInit();
			}
			return o;
		}

		/// <summary>复用传入的 <paramref name="o"/> 实例（避免重新分配）：MemoryPack 的 <c>Deserialize&lt;Type&gt;(ref object)</c> 会用 runtime type 填字段，调用方需保证 <paramref name="o"/> 是正确类型。</summary>
		public static object Deserialize(Type type, byte[] bytes, int index, int count, ref object o)
		{
			MemoryPackSerializer.Deserialize(type, bytes.AsSpan(index, count), ref o);
			if (o is ISupportInitialize supportInitialize)
			{
				supportInitialize.EndInit();
			}
			return o;
		}

		/// <summary>从 <see cref="MemoryBuffer"/> 反序列化（网络收包路径）。</summary>
		public static object Deserialize(Type type, MemoryBuffer stream)
		{
			object o = MemoryPackSerializer.Deserialize(type, stream.GetSpan());
			if (o is ISupportInitialize supportInitialize)
			{
				supportInitialize.EndInit();
			}
			return o;
		}

		/// <summary>复用实例版的 <see cref="Deserialize(Type, MemoryBuffer)"/>。</summary>
		public static object Deserialize(Type type, MemoryBuffer stream, ref object o)
		{
			MemoryPackSerializer.Deserialize(type, stream.GetSpan(), ref o);
			if (o is ISupportInitialize supportInitialize)
			{
				supportInitialize.EndInit();
			}
			return o;
		}
	}
}
