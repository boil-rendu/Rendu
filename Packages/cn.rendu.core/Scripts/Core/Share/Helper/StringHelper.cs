using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RD.Core
{
	/// <summary>字符串工具：首字母大写、字节/字符串互转、hex 解码、<c>string.Format</c> 简写、列表/数组转 <c>ToString</c> 调试串。</summary>
	public static class StringHelper
	{
		/// <summary>把首字符转大写（<c>ToUpper</c> 第一个字符），空字符串安全。用于「按命名生成属性名/方法名」等场景。</summary>
		public static string CapitalizeFirstLetter(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			string s1 = input[..1];
			string s2 = input[1..];
			return s1.ToUpper() + s2;
		}

		/// <summary>用系统默认编码（GBK）把字符串编码为字节序列（返回 <see cref="IEnumerable{T}"/>）。</summary>
		public static IEnumerable<byte> ToBytes(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}

		/// <summary>用系统默认编码编码为字节数组（<c>byte[]</c> 形式）。</summary>
		public static byte[] ToByteArray(this string str)
		{
			byte[] byteArray = Encoding.Default.GetBytes(str);
			return byteArray;
		}

		/// <summary>UTF-8 编码为字节数组。</summary>
		public static byte[] ToUtf8(this string str)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(str);
			return byteArray;
		}

		/// <summary>把 hex 字符串解码为字节数组：长度必须为偶数（奇数抛 <see cref="ArgumentException"/>），用 <see cref="CultureInfo.InvariantCulture"/> 避免本地化数字格式问题。</summary>
		public static byte[] HexToBytes(this string hexString)
		{
			if (hexString.Length % 2 != 0)
			{
				throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
			}

			var hexAsBytes = new byte[hexString.Length / 2];
			for (int index = 0; index < hexAsBytes.Length; index++)
			{
				string byteValue = "";
				byteValue += hexString[index * 2];
				byteValue += hexString[index * 2 + 1];
				hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return hexAsBytes;
		}

		/// <summary><c>string.Format</c> 简写：<c>"x={0}".Fmt(42)</c>。</summary>
		public static string Fmt(this string text, params object[] args)
		{
			return string.Format(text, args);
		}

		/// <summary><see cref="List{T}"/> 转调试字符串：元素间逗号分隔，<c>ToString</c> 形式（不递归引号包裹）。</summary>
		public static string ListToString<T>(this List<T> list)
		{
			StringBuilder sb = new StringBuilder();
			foreach (T t in list)
			{
				sb.Append(t);
				sb.Append(",");
			}
			return sb.ToString();
		}

		/// <summary>数组转调试字符串：<c>" [a, b, c]"</c> 格式（首尾方括号，逗号+空格分隔）。<c>null</c> 输入返回空字符串。</summary>
		public static string ArrayToString<T>(this T[] args)
		{
			if (args == null)
			{
				return "";
			}

			string argStr = " [";
			for (int arrIndex = 0; arrIndex < args.Length; arrIndex++)
			{
				argStr += args[arrIndex];
				if (arrIndex != args.Length - 1)
				{
					argStr += ", ";
				}
			}

			argStr += "]";
			return argStr;
		}

		/// <summary>数组子段 <c>[index, index+count)</c> 转调试字符串（与无子段版本格式一致）。</summary>
		public static string ArrayToString<T>(this T[] args, int index, int count)
		{
			if (args == null)
			{
				return "";
			}

			string argStr = " [";
			for (int arrIndex = index; arrIndex < count + index; arrIndex++)
			{
				argStr += args[arrIndex];
				if (arrIndex != args.Length - 1)
				{
					argStr += ", ";
				}
			}

			argStr += "]";
			return argStr;
		}
	}
}
