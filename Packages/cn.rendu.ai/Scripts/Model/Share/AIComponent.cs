using RD.Core;

namespace RD
{
	/// <summary>
	/// AI 行为驱动器：挂在 <see cref="Scene"/>（客户端或服务端 Unit）上，运行一棵由 <see cref="AIConfig"/> 描述的 AI 行为树。
	/// 启动期从 <see cref="AIDispatcherComponent"/> 取出当前 <see cref="AIConfigId"/> 对应的 handler 链，
	/// 按 <see cref="Check"/> 通过后 <c>await Execute(...)</c>，期间 <see cref="CancellationToken"/> 可被外部取消。
	/// </summary>
	// 客户端挂在ClientScene上，服务端挂在Unit上
	[ComponentOf(typeof(Scene))]
	public class AIComponent: Entity, IAwake<int>, IDestroy
	{
		/// <summary>AI 配置 Id（指向 <see cref="AIConfigCategory.AIConfigs"/> 中的某条子树）。</summary>
		public int AIConfigId;

		/// <summary>当前正在跑的 AI 行为的取消令牌（外部用 <c>Cancel()</c> 立即终止）。</summary>
		public PromiseCancellationToken CancellationToken;

		/// <summary>当前节点的计时器句柄（用于按时间切换到下一节点）。</summary>
		public long Timer;

		/// <summary>当前正在执行的 AI 行为索引（在 <see cref="AIConfig"/> 链中的位置）。</summary>
		public int Current;
	}
}
