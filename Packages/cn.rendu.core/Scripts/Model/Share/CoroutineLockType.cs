namespace RD
{
	/// <summary>
	/// 协程锁类型常量集合（partial，多个 dll 按需扩展）：每个常量 = <c>PackageType.xxx * 1000 + 业务子码</c>。
	/// 业务侧 <c>CoroutineLockComponent.Wait(type, level, key)</c> 的 <c>type</c> 参数必须取自这里。
	/// </summary>
	public static partial class CoroutineLockType
	{
		/// <summary>Mailbox 消息队列互斥锁：保证同一 Mailbox 同一时刻只处理一条消息。</summary>
		public const int Mailbox = PackageType.Core * 1000 + 1;                   // Mailbox中队列
	}
}
