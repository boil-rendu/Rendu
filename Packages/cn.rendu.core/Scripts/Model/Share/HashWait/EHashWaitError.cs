using RD.Core;
namespace RD
{
	/// <summary>
	/// <see cref="HashWait"/>（及同类 hash 维度的等待结构）操作结果枚举。
	/// 由 <see cref="HashWait.m_HashWaitTasks"/> 字典的 <see cref="RD.Core.Promise{T}"/> 携带回调用方。
	/// </summary>
	public enum EHashWaitError
	{
		/// <summary>其他错误（业务自定义）。</summary>
		Error = -1, //其他
		/// <summary>成功。</summary>
		Success = 0, //成功
		/// <summary>等待结构被销毁（关联 Entity 释放/Scene 销毁等）。</summary>
		Destroy = 1, //摧毁
		/// <summary>主动取消。</summary>
		Cancel = 2, //取消
		/// <summary>超时。</summary>
		Timeout = 3, //超时
		/// <summary>重置（中途由其他流程接管）。</summary>
		Reset = 4, //重置
	}
}
