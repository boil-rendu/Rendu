using RD.Core;

namespace RD
{
	/// <summary>
	/// Main Fiber 初始化器（<c>[Invoke(SceneType.Main)]</c>）：每个 Fiber 创建时由 <see cref="EventSystem"/> 自动调，
	/// 把 <see cref="Options.SceneName"/> 解析为 <see cref="int"/> SceneType 写到 <see cref="Scene.SceneType"/>，
	/// 并依次 <c>await</c> <see cref="InitShare"/>/<see cref="InitServer"/>/<see cref="InitClient"/> 三个空标记事件触发各子模块初始化。
	/// </summary>
	[Invoke(SceneType.Main)]
	public class MainFiberInit: AInvokeHandler<FiberInit, Promise>
	{
		public override async Promise Handle(FiberInit fiberInit)
		{
			Scene root = fiberInit.Fiber.Root;

			int sceneType = SceneTypeSingleton.Instance.GetSceneType(Options.Instance.SceneName);
			root.SceneType = sceneType;

			await EventSystem.Instance.PublishAsync(root, new InitShare());
			await EventSystem.Instance.PublishAsync(root, new InitServer());
			await EventSystem.Instance.PublishAsync(root, new InitClient());
		}
	}
}
