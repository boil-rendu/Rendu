using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="MailBoxType.UnOrderedMessage"/> 类型的消息处理器（<c>[Invoke(MailBoxType.UnOrderedMessage)]</c>）：
	/// 收到入箱的 MailBoxInvoker 后调 <see cref="MessageDispatcher"/> 按消息体 opcode 路由到具体 handler。
	/// </summary>
	[Invoke(MailBoxType.UnOrderedMessage)]
	public class MailBoxType_UnOrderedMessageHandler: AInvokeHandler<MailBoxInvoker>
	{
		/// <summary>Entry 入口：起 fire-and-forget <see cref="HandleAsync"/>（不 await）。</summary>
		public override void Handle(MailBoxInvoker args)
		{
			HandleAsync(args).NoContext();
		}

		/// <summary>实际处理：把 <see cref="MailBoxInvoker.MailBoxComponent"/>.Parent 当作目标 Entity，<see cref="FromAddress"/> 当作来源，调 <see cref="MessageDispatcher.Handle"/>。</summary>
		private static async Promise HandleAsync(MailBoxInvoker args)
		{
			MailBoxComponent mailBoxComponent = args.MailBoxComponent;

			MessageObject messageObject = args.MessageObject;

			await MessageDispatcher.Instance.Handle(mailBoxComponent.Parent, args.FromAddress, messageObject);
		}
	}
}
