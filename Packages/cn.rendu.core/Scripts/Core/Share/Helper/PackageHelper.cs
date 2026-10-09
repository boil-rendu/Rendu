using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace RD.Core
{
	/// <summary>Unity <c>packages-lock.json</c> 反序列化模型：只保留 <c>cn.rendu.*</c> 包，<c>[BsonIgnore]</c> 的 <c>unityDir/name/module/dir</c> 在加载后回填。</summary>
	public class PackagesLock
	{
		/// <summary>Unity 工程根目录（构造后由 <see cref="PackageHelper.LoadRDPackagesLock"/> 回填，序列化时忽略）。</summary>
		[BsonIgnore]
		public string unityDir;

		/// <summary>包名 → <see cref="PackageInfo"/> 映射（仅含 <c>cn.rendu.*</c>，其它包加载时被移除）。</summary>
		public Dictionary<string, PackageInfo> dependencies = new();
	}

	/// <summary>单个包信息：name/module 加载时回填，version/depth/source/hash/url/dir/dependencies 来自 <c>packages-lock.json</c>。</summary>
	public class PackageInfo
	{
		/// <summary>包全名（<c>cn.rendu.core</c> 等），加载时回填。</summary>
		[BsonIgnore]
		public string name;
		/// <summary>去掉 <c>cn.rendu.</c> 前缀的简称（<c>core</c>/<c>ui</c> 等），加载时回填。</summary>
		[BsonIgnore]
		public string module;

		public string version;
		public int depth;
		public string source;
		public string hash;
		public string url;
		public string dir;
		public Dictionary<string, string> dependencies = new();
	}

	/// <summary><c>packages-lock.json</c> 加载与路径解析：按 <c>source</c>（embedded/git/其它）回填 <see cref="PackageInfo.dir"/>，过滤非 <c>cn.rendu.*</c> 包。</summary>
	public static class PackageHelper
	{
		/// <summary>从 <c>{unityDir}/Packages/packages-lock.json</c> 反序列化，移除非 <c>cn.rendu.*</c> 依赖，按 <c>source</c> 填 <see cref="PackageInfo.dir"/>：<c>embedded</c> → <c>Packages/version</c>，<c>git</c> → <c>Library/PackageCache/...</c> 或 <c>Packages/...@hash</c>，其它 → <c>Library/PackageCache/name@version</c>。</summary>
		/// <param name="unityDir">Unity 工程根目录（含 <c>Packages/</c> 和 <c>Library/PackageCache/</c>）。</param>
		public static PackagesLock LoadRDPackagesLock(string unityDir)
		{
			string s = File.ReadAllText(Path.Combine(unityDir, "Packages/packages-lock.json"));

			PackagesLock packagesLock = MongoHelper.FromJson<PackagesLock>(s);
			packagesLock.unityDir = unityDir;

			foreach (string k in packagesLock.dependencies.Keys.ToArray())
			{
				if (!k.StartsWith("cn.rendu"))
				{
					packagesLock.dependencies.Remove(k);
					continue;
				}

				PackageInfo packageInfo = packagesLock.dependencies[k];

				packageInfo.name = k;
				packageInfo.module = k.Replace("cn.rendu.", "");

				if (packageInfo.source == "embedded")
				{
					packageInfo.dir = Path.Combine(unityDir, "Packages", packageInfo.version.Replace("file:", ""));
				}
				else if (packageInfo.source == "git")
				{
					string p1 = Path.Combine(unityDir, "Library/PackageCache", k + "@" + packageInfo.hash.Substring(0, 10));
					string p2 = Path.Combine(unityDir, "Packages", k + "@" + packageInfo.hash);
					if (Directory.Exists(p1))
					{
						packageInfo.dir = p1;
					}
					else if (Directory.Exists(p2))
					{
						packageInfo.dir = p2;
					}
					else
					{
						throw new Exception($"not found package: {p1} {p2}");
					}
				}
				else
				{
					packageInfo.dir = Path.Combine(unityDir, "Library/PackageCache", k + "@" + packageInfo.version);
				}
			}

			return packagesLock;
		}
	}
}
