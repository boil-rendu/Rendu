using System.IO;
using System.Security.Cryptography;

namespace RD.Core
{
	/// <summary>MD5 工具：把文件哈希成小写 hex 字符串（用 <see cref="ByteHelper.ToHex(byte[],string)"/> "x2" 格式）。</summary>
	public static class MD5Helper
	{
		/// <summary>计算 <paramref name="filePath"/> 文件的 MD5，返回小写 hex（32 字符）。文件不存在或读取失败会抛 <see cref="IOException"/>。</summary>
		public static string FileMD5(string filePath)
		{
			byte[] retVal;
			using (FileStream file = new FileStream(filePath, FileMode.Open))
			{
				MD5 md5 = MD5.Create();
				retVal = md5.ComputeHash(file);
			}
			return retVal.ToHex("x2");
		}
	}
}
