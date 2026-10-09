using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 配置加载后处理特性（<c>[AttributeUsage(Class)]</c>）：标在静态类上，提供一个无参的 <c>Process</c> 方法（SourceGenerator 收口），
	/// 所有 <c>[Config]</c> 表加载完成后统一调用，做合并（<see cref="IMerge"/>）/补缺等业务处理。
	/// </summary>

	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigProcessAttribute: BaseAttribute
	{

	}
}
