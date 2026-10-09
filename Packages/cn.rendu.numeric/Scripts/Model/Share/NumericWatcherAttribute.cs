using System;
using RD.Core;

namespace RD
{
	/// <summary>
	/// 数值监听器作用域特性（<c>[AttributeUsage(Class)]</c>）：标在 <see cref="INumericWatcher"/> 子类上，声明生效的 SceneType + 监听的 numericType。
	/// SourceGenerator 在生成时按 (SceneType, NumericType) 索引 watcher。
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NumericWatcherAttribute : BaseAttribute
	{
		/// <summary>watcher 生效的 SceneType。</summary>
		public int SceneType { get; }

		/// <summary>监听的 numericType（见 <see cref="NumericType"/> 常量）。</summary>
		public int NumericType { get; }

		public NumericWatcherAttribute(int sceneType, int type)
		{
			this.SceneType = sceneType;
			this.NumericType = type;
		}
	}
}
