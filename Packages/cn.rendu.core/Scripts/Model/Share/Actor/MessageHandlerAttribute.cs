using RD.Core;

namespace RD
{
	/// <summary>
	/// 消息处理器作用域特性：把 <c>IMHandler</c> 标在某个 SceneType 下，用于 <c>IMHandler</c> 自动注册阶段的 Scene 过滤。
	/// 由 SourceGenerator 在 <see cref="EventSystem"/> 注册 handler 时读取，<see cref="SceneType"/> 与 <see cref="Address.SceneType"/> 不匹配的消息会被忽略。
	/// </summary>
	public class MessageHandlerAttribute: BaseAttribute
	{
		/// <summary>本 handler 所属的 SceneType（<see cref="SceneType"/> 常量之一）。</summary>
		public int SceneType { get; }

		public MessageHandlerAttribute(int sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}
