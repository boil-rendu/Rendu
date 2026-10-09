using System.Text;

namespace RD.Core
{
	/// <summary>字节/字符串转换与定长写入工具：<c>ToHex</c>、<c>ToStr</c>、<c>Utf8ToStr</c>、<c>WriteTo</c>（小端写 <c>byte/short/ushort/int/uint/long</c>/<see cref="ActorId"/>），<c>Hash</c>（FNV-1a 64 位变体）。</summary>
	public static class ByteHelper
	{
		/// <summary>单字节转 2 位大写 hex（<c>X2</c>）。</summary>
		public static string ToHex(this byte b)
		{
			return b.ToString("X2");
		}

		/// <summary>整段字节转大写 hex 字符串（<c>StringBuilder</c> 拼接无分配）。</summary>
		public static string ToHex(this byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		/// <summary>整段字节按自定义 <paramref name="format"/>（如 <c>"x2"</c>、<c>"X"</c>）转 hex 字符串。</summary>
		public static string ToHex(this byte[] bytes, string format)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in bytes)
			{
				stringBuilder.Append(b.ToString(format));
			}
			return stringBuilder.ToString();
		}

		/// <summary>子段 <c>[offset, offset+count)</c> 转大写 hex 字符串。</summary>
		public static string ToHex(this byte[] bytes, int offset, int count)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = offset; i < offset + count; ++i)
			{
				stringBuilder.Append(bytes[i].ToString("X2"));
			}
			return stringBuilder.ToString();
		}

		/// <summary>用系统默认编码（<c>Encoding.Default</c>，中文 Windows 通常 GBK）解码整段字节。</summary>
		public static string ToStr(this byte[] bytes)
		{
			return Encoding.Default.GetString(bytes);
		}

		/// <summary>用系统默认编码解码 <c>[index, index+count)</c> 子段。</summary>
		public static string ToStr(this byte[] bytes, int index, int count)
		{
			return Encoding.Default.GetString(bytes, index, count);
		}

		/// <summary>UTF-8 解码整段字节。</summary>
		public static string Utf8ToStr(this byte[] bytes)
		{
			return Encoding.UTF8.GetString(bytes);
		}

		/// <summary>UTF-8 解码 <c>[index, index+count)</c> 子段。</summary>
		public static string Utf8ToStr(this byte[] bytes, int index, int count)
		{
			return Encoding.UTF8.GetString(bytes, index, count);
		}

		/// <summary>按小端序把 <c>uint</c> 写入 <c>bytes[offset..offset+3]</c>。</summary>
		public static void WriteTo(this byte[] bytes, int offset, uint num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
			bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
			bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
		}

		/// <summary>按 4+4+8 字节小端序写 <see cref="ActorId"/>（Process + Fiber + InstanceId）。</summary>
		public static void WriteTo(this byte[] bytes, int offset, ActorId num)
		{
			bytes.WriteTo(offset, num.Process);
			bytes.WriteTo(offset + 4, num.Fiber);
			bytes.WriteTo(offset + 8, num.InstanceId);
		}

		/// <summary>按小端序把 <c>int</c> 写入 <c>bytes[offset..offset+3]</c>。</summary>
		public static void WriteTo(this byte[] bytes, int offset, int num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
			bytes[offset + 2] = (byte)((num & 0xff0000) >> 16);
			bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
		}

		/// <summary>写入单字节。</summary>
		public static void WriteTo(this byte[] bytes, int offset, byte num)
		{
			bytes[offset] = num;
		}

		/// <summary>按小端序把 <c>short</c> 写入 <c>bytes[offset..offset+1]</c>。</summary>
		public static void WriteTo(this byte[] bytes, int offset, short num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
		}

		/// <summary>按小端序把 <c>ushort</c> 写入 <c>bytes[offset..offset+1]</c>。</summary>
		public static void WriteTo(this byte[] bytes, int offset, ushort num)
		{
			bytes[offset] = (byte)(num & 0xff);
			bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
		}

		/// <summary>按本机字节序（不安全指针）把 <c>long</c> 写入 <c>bytes[offset..offset+7]</c>。</summary>
		public static unsafe void WriteTo(this byte[] bytes, int offset, long num)
		{
			byte* bPoint  = (byte*)&num;
			for (int i = 0; i < sizeof(long); ++i)
			{
				bytes[offset + i] = bPoint[i];
			}
		}

		/// <summary>FNV-1a 变体哈希（64 位）：初始 offset = 2166136261，prime = 16777619，末尾做 Bob Jenkins 整数混淆（<c>hash &lt;&lt; 13</c> 等）提升分布。</summary>
		public static long Hash(this byte[] data, int index, int length)
		{
			const int p = 16777619;
			long hash = 2166136261L;

			for (int i = index; i < index + length; i++)
			{
				hash = (hash ^ data[i]) * p;
			}

			hash += hash << 13;
			hash ^= hash >> 7;
			hash += hash << 3;
			hash ^= hash >> 17;
			hash += hash << 5;
			return hash;
		}
	}
}
