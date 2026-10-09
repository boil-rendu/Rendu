namespace RD
{
	/// <summary>
	/// 进程内 ErrorCode 顶层常量集合（<c>[UniqueId]</c> 自动注入排序）：所有子 dll 的 ErrorCode 都 partial 继承。
	/// <para>错误码区间约定（与网络层 <see cref="System.Net.Sockets.SocketError"/> 不冲突，从 1 起步）：
	/// <list type="bullet">
	///   <item>1 ~ 11004：<see cref="System.Net.Sockets.SocketError"/> 透传</item>
	///   <item>100000000（含）以下：抛异常的 RPC 错误（网络层/底层错误，统一前缀 <c>ERR_WithException</c>）</item>
	///   <item>100000000 ~ 200000000：业务逻辑错误（业务自定义）</item>
	///   <item>200000000（含）以上：业务自定义不抛异常的 RPC 错误（调用方需自行处理）</item>
	/// </list></para>
	/// </summary>
	[UniqueId]
	public static partial class ErrorCode
	{
		/// <summary>RPC 成功（error = 0 表示无错）。</summary>
		public const int ERR_Success = 0;

		// 1-11004 是SocketError请看SocketError定义
		//-----------------------------------
		// 100000-100000000是Core层的错误

		// 这里配置逻辑层的错误码
		// 100000000以上是逻辑层的错误
		// 200000000以上不抛异常  ErrorCore.IsRpcNeedThrowException

		//public const int ErrorExampleException = 100000000 + PackageType.Core * 1000 + 1;
		//public const int ErrorExampleNoException = 200000000 + PackageType.Core * 1000 + 1;
		/// <summary>异常错误码阈值（包含）：小于等于此值的错误码会由 RPC 框架自动抛 <see cref="System.Exception"/>。</summary>
		public const int ERR_WithException = 100000000;
		/// <summary>非异常错误码阈值（包含）：大于此值的错误码需要调用方自行判断处理，RPC 框架不会抛异常。</summary>
		// 小于这个Rpc会抛异常，大于这个异常的error需要自己判断处理，也就是说需要处理的错误应该要大于该值
		public const int ERR_WithoutException = 200000000;

		/// <summary>RPC 取消（业务主动取消等待）。</summary>
		public const int ERR_Cancel = 200000000 + PackageType.Core * 1000 + 1;
		/// <summary>RPC 超时。</summary>
		public const int ERR_Timeout = 200000000 + PackageType.Core * 1000 + 2;


		/// <summary>判断 <paramref name="error"/> 是否需要 RPC 框架自动抛异常（即 &lt;= <see cref="ERR_WithoutException"/> 且 != 0）。</summary>
		public static bool IsRpcNeedThrowException(int error)
		{
			if (error == 0)
			{
				return false;
			}
			if (error > ERR_WithoutException)
			{
				return false;
			}

			return true;
		}
	}
}
