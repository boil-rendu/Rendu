using System;
using System.Collections.Generic;
using System.IO;

namespace RD.Core
{
	/// <summary>文件/目录工具：递归遍历、清理、复制、扩展名批量替换。</summary>
	public static class FileHelper
	{
		/// <summary>递归收集 <paramref name="dir"/> 下所有匹配 <paramref name="searchPattern"/> 的文件路径（默认 <c>*</c>）。</summary>
		public static List<string> GetAllFiles(string dir, string searchPattern = "*")
		{
			List<string> list = new List<string>();
			GetAllFiles(list, dir, searchPattern);
			return list;
		}

		/// <summary>递归收集，追加到外部 <paramref name="files"/> 列表（避免每次 <c>GetAllFiles</c> 都 new 列表）。</summary>
		public static void GetAllFiles(List<string> files, string dir, string searchPattern = "*")
		{
			string[] fls = Directory.GetFiles(dir, searchPattern);
			foreach (string fl in fls)
			{
				files.Add(fl);
			}

			string[] subDirs = Directory.GetDirectories(dir);
			foreach (string subDir in subDirs)
			{
				GetAllFiles(files, subDir, searchPattern);
			}
		}

		/// <summary>清空 <paramref name="dir"/>：目录不存在静默返回；先递归删子目录再删子文件（保留 <paramref name="dir"/> 本身）。</summary>
		public static void CleanDirectory(string dir)
		{
			if (!Directory.Exists(dir))
			{
				return;
			}
			foreach (string subdir in Directory.GetDirectories(dir))
			{
				Directory.Delete(subdir, true);
			}

			foreach (string subFile in Directory.GetFiles(dir))
			{
				File.Delete(subFile);
			}
		}

		/// <summary>递归复制 <paramref name="srcDir"/> 到 <paramref name="tgtDir"/>：源不存在静默返回；目标不存在自动创建；含「父→子」循环拷贝检测。</summary>
		public static void CopyDirectory(string srcDir, string tgtDir)
		{
			DirectoryInfo source = new DirectoryInfo(srcDir);
			DirectoryInfo target = new DirectoryInfo(tgtDir);

			if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				throw new Exception("父目录不能拷贝到子目录！");
			}

			if (!source.Exists)
			{
				return;
			}

			if (!target.Exists)
			{
				target.Create();
			}

			FileInfo[] files = source.GetFiles();

			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
			}

			DirectoryInfo[] dirs = source.GetDirectories();

			for (int j = 0; j < dirs.Length; j++)
			{
				CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
			}
		}

		/// <summary>递归把 <paramref name="srcDir"/> 下扩展名 <paramref name="extensionName"/> 改为 <paramref name="newExtensionName"/>：用于热更 dll 出包后改后缀规避 Unity 识别。</summary>
		public static void ReplaceExtensionName(string srcDir, string extensionName, string newExtensionName)
		{
			if (Directory.Exists(srcDir))
			{
				string[] fls = Directory.GetFiles(srcDir);

				foreach (string fl in fls)
				{
					if (fl.EndsWith(extensionName))
					{
						File.Move(fl, fl.Substring(0, fl.IndexOf(extensionName)) + newExtensionName);
						File.Delete(fl);
					}
				}

				string[] subDirs = Directory.GetDirectories(srcDir);

				foreach (string subDir in subDirs)
				{
					ReplaceExtensionName(subDir, extensionName, newExtensionName);
				}
			}
		}
	}
}
