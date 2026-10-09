using RD.Core;

namespace RD
{
	/// <summary>
	/// Session 消息处理器作用域特性：把 <c>IMessageSessionHandler</c> 标在某个 SceneType 下，
	/// <see cref="MessageSessionDispatcher"/> 用 <see cref="SceneType"/> 过滤，跨 Scene 的消息会被忽略。
	/// </summary>
	public class MessageSessionHandlerAttribute: BaseAttribute
	{
		/// <summary>本 handler 所属的 SceneType。</summary>
		public int SceneType { get; }

		public MessageSessionHandlerAttribute(int sceneType)
		{
			this.SceneType = sceneType;
		}
	}
}
