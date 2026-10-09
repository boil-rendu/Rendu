using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 配置表 category marker 特性（<c>[AttributeUsage(Class)]</c>）：标在 <c>XxxConfig : ACategory, IConfig</c> 子类上，
	/// <c>ConfigLoader</c> 启动期通过 <see cref="CodeTypes"/> 反射收集所有 <c>[Config]</c> 类型，依次初始化。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigAttribute: BaseAttribute
	{
	}
}
