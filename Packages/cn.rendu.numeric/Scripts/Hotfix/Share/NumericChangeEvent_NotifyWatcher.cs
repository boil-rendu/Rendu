using RD.Core;

namespace RD
{
	// 分发数值监听
	/// <summary>
	/// 数值变化事件 → 数值监听器分发器（<c>[Event(SceneType.All)]</c>）：所有 Scene 都生效。
	/// 收到 <see cref="NumericChange"/> 事件后转交给 <see cref="NumericWatcherComponent"/> 按 numericType 路由到具体 <see cref="INumericWatcher"/>。
	/// </summary>
	[Event(SceneType.All)]  // 服务端Map需要分发, 客户端CurrentScene也要分发
	public class NumericChangeEvent_NotifyWatcher: AEvent<Scene, NumericChange>
	{
		protected override async Promise Run(Scene scene, NumericChange args)
		{
			NumericWatcherComponent.Instance.Run(args.Unit, args);
			await Promise.CompletedTask;
		}
	}
}
