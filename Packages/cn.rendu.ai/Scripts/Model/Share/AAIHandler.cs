using System;
using RD.Core;

namespace RD
{
	/// <summary>AI 处理器 marker 特性：标在 <see cref="AAIHandler"/> 子类上，启动期 <see cref="AIDispatcherComponent"/> 反射收集到字典。</summary>
	public class AIHandlerAttribute: BaseAttribute
	{
	}

	/// <summary>
	/// AI 行为节点基类：每个 AI 行为（如巡逻/攻击/追击）继承 <see cref="AAIHandler"/>，实现 <see cref="Check"/> + <see cref="Execute"/>。
	/// 由 <see cref="AIDispatcherComponent"/> 按 <c>type.Name</c> 索引分发。
	/// </summary>
	[AIHandler]
	public abstract class AAIHandler: HandlerObject
	{
		/// <summary>检查该 AI 行为是否满足执行条件（参数 <c>aiComponent</c> 携带状态，<c>aiConfig</c> 携带数据）。返回非 0 表示不可执行。</summary>
		// 检查是否满足条件
		public abstract int Check(AIComponent aiComponent, AIConfig aiConfig);

		/// <summary>执行 AI 行为（通常是协程/异步）。必须可被 <see cref="AIComponent.CancellationToken"/> 取消。</summary>
		// 协程编写必须可以取消
		public abstract Promise Execute(AIComponent aiComponent, AIConfig aiConfig);
	}
}
