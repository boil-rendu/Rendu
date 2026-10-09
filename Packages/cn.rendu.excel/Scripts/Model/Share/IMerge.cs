namespace RD
{
	/// <summary>
	/// 配置合并接口：由 <c>[ConfigProcess]</c> 流程调 <see cref="Merge(object)"/> 把多个同类型 config 合并为一条。
	/// </summary>
	public interface IMerge
	{
		/// <summary>把 <paramref name="o"/> 合并进当前对象（业务实现，<paramref name="o"/> 一般是同类型或兼容类型）。</summary>
		void Merge(object o);
	}
}
