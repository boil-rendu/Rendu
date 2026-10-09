using RD.Core;

namespace RD
{
	/// <summary>
	/// <see cref="MailBoxComponent"/> 的系统扩展（Model 层），管理邮箱组件的注册、注销与消息入队：
	/// <list type="bullet">
	///   <item><see cref="Awake"/>：设置邮箱类型并登记到所属 Fiber.Mailboxes，使其可接收消息。</item>
	///   <item><see cref="Destroy"/>：从 Fiber.Mailboxes 反查并移除自身。</item>
	///   <item><see cref="Add"/>：外部投递入口，按 MailBoxType 路由到对应的消息处理 Handler。</item>
	/// </list>
	/// </summary>
	[EntitySystemOf(typeof(MailBoxComponent))]
	public static partial class MailBoxComponentSystem
	{
		/// <summary>初始化邮箱：记录类型、缓存父实体 InstanceId，并注册到 Fiber.Mailboxes 使其进入可接收状态。</summary>
		[EntitySystem]
		private static void Awake(this MailBoxComponent self, int mailBoxType)
		{
			Fiber fiber = self.Fiber();
			self.MailBoxType = mailBoxType;
			self.ParentInstanceId = self.Parent.InstanceId;
			fiber.Mailboxes.Add(self);
		}

		/// <summary>销毁邮箱：按缓存的 ParentInstanceId 从 Fiber.Mailboxes 反查并移除自身。</summary>
		[EntitySystem]
		private static void Destroy(this MailBoxComponent self)
		{
			self.Fiber().Mailboxes.Remove(self.ParentInstanceId);
		}

		/// <summary>
		/// 外部投递消息的入口：把消息连同来源地址封装为 <see cref="MailBoxInvoker"/>，
		/// 通过 EventSystem.Invoke(MailBoxType, invoker) 按邮箱类型路由到对应的消息处理 Handler。
		/// </summary>
		public static void Add(this MailBoxComponent self, Address fromAddress, MessageObject messageObject)
		{
			// 根据mailboxType进行分发处理
			EventSystem.Instance.Invoke(self.MailBoxType, new MailBoxInvoker() {MailBoxComponent = self, MessageObject = messageObject, FromAddress = fromAddress});
		}
	}

	/// <summary>
	/// 邮件路由上下文：把「来源地址 + 目标 MailBox + 消息体」封装在一起，作为 <see cref="EventSystem.Invoke(int, object)"/> 的入参，
	/// 由具体的 <c>IMHandler</c> 处理器接收。
	/// </summary>
	public struct MailBoxInvoker
	{
		/// <summary>消息来源地址（用于回包）。</summary>
		public Address FromAddress;
		/// <summary>消息体（含 Opcode + body）。</summary>
		public MessageObject MessageObject;
		/// <summary>目标邮箱组件。</summary>
		public MailBoxComponent MailBoxComponent;
	}

	/// <summary>
	/// Actor 邮箱组件。挂上该组件的 Entity 即成为一个消息处理单元（Actor）：
	/// 外部通过 <see cref="MailBoxComponentSystem.Add"/> 投递的消息会按 <see cref="MailBoxType"/>
	/// 路由到对应的消息处理系统，实现"队列化、按类型分发"的 Actor 语义。
	/// <para>典型用法：Scene/Unit 等需要接收远程或跨纤程消息的实体挂此组件，
	/// 其 <see cref="MailBoxType"/> 决定 EventSystem 中哪一组 Handler 来处理消息。</para>
	/// </summary>
	[ComponentOf]
	public class MailBoxComponent: Entity, IAwake<int>, IDestroy
	{
		/// <summary>拥有该 MailBox 的父实体实例 Id。用于 Fiber.Mailboxes 字典的反查，销毁时据其移除自身。</summary>
		public long ParentInstanceId { get; set; }

		/// <summary>邮箱类型（消息分发路由键）。对应 EventSystem.Invoke 的 type 参数，决定消息交由哪类 Handler 处理。</summary>
		public int MailBoxType { get; set; }
	}
}
