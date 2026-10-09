using System;
using System.Collections.Generic;
using RD.Core;

namespace RD
{
	/// <summary>
	/// AI 处理器字典（<see cref="Singleton{T}"/>，<c>[CodeProcess]</c> 启动期初始化）：
	/// Awake 时通过 <see cref="CodeTypes"/> 反射收集所有标 <c>[AIHandler]</c> 的 <see cref="AAIHandler"/> 子类，
	/// 按 <c>type.Name</c> 索引为字典，业务侧 <see cref="Get(string)"/> 取出。
	/// </summary>
	[CodeProcess]
	public class AIDispatcherComponent: Singleton<AIDispatcherComponent>, ISingletonAwake
	{
		private readonly Dictionary<string, AAIHandler> aiHandlers = new();

		public void Awake()
		{
			var types = CodeTypes.Instance.GetTypes(typeof (AIHandlerAttribute));
			foreach (Type type in types)
			{
				AAIHandler aaiHandler = Activator.CreateInstance(type) as AAIHandler;
				if (aaiHandler == null)
				{
					Log.Error($"robot ai is not AAIHandler: {type.Name}");
					continue;
				}
				this.aiHandlers.Add(type.Name, aaiHandler);
			}
		}

		/// <summary>取 <paramref name="key"/>（一般 <c>type.Name</c>）对应的 AI handler；找不到返回 <c>null</c>（不抛）。</summary>
		public AAIHandler Get(string key)
		{
			this.aiHandlers.TryGetValue(key, out var aaiHandler);
			return aaiHandler;
		}
	}
}
