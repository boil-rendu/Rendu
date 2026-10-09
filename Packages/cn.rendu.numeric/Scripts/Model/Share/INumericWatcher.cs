namespace RD
{
	/// <summary>
	/// 数值监听器接口：业务侧实现 <see cref="Run(Unit, NumericChange)"/>，标 <c>[NumericWatcher(SceneType.Xxx, NumericType.Yyy)]</c>，
	/// SourceGenerator 在生成时把对应 numericType 的 <see cref="NumericChange"/> 事件派发到本 watcher。
	/// </summary>
	public interface INumericWatcher
	{
		/// <summary>数值变化时调：<paramref name="unit"/> 是数值所属 Unit，<paramref name="args"/> 携带 Old/New/NumericType。</summary>
		void Run(Unit unit, NumericChange args);
	}
}
