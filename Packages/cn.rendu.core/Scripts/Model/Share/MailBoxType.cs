namespace RD
{
	/// <summary>
	/// MailBox 类型常量集合（partial）：每个常量 = <c>PackageType.xxx * 1000 + 业务子码</c>。
	/// 业务侧 <c>MailBoxComponent.Awake(mailBoxType)</c> 的 <c>mailBoxType</c> 参数必须取自这里，决定 EventSystem 中哪组 Handler 处理消息。
	/// </summary>
	public static partial class MailBoxType
	{
		/// <summary>无序消息邮箱：到达顺序不必严格（性能优先）。</summary>
		public const int UnOrderedMessage = PackageType.Core * 1000 + 1;
	}
}
