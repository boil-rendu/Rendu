using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD.Core
{
	/// <summary>程序集类型扫描工具：把多 <see cref="Assembly"/> 的所有 <see cref="Type"/> 合并成 <c>FullName → Type</c> 字典。</summary>
	public static class AssemblyHelper
	{
		/// <summary>遍历 <paramref name="args"/> 每个 <see cref="Assembly"/> 调 <c>GetTypes()</c>，合并为全名到类型的字典；被 <see cref="CodeTypes.Awake"/> 调用扫描 AOT/热更 dll。</summary>
		/// <param name="args">要扫描的程序集集合。</param>
		public static Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
		{
			Dictionary<string, Type> types = new Dictionary<string, Type>();

			foreach (Assembly ass in args)
			{
				var ts = ass.GetTypes();
				foreach (Type type in ts)
				{
					types[type.FullName] = type;
				}
			}

			return types;
		}
	}
}
