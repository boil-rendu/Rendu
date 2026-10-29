using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// [OnStateUpdate]: 每帧执行成员上的状态表达式(如 $property.State.Expanded = true)。
    ///
    /// 单独成类是因为它**写状态**(折叠态), 而不是画东西:
    /// 渲染器与测试都走同一份实现, 语义不会分叉。
    /// </summary>
    internal sealed class StateUpdater
    {
        private readonly ValueAccessor m_Values;

        public StateUpdater(ValueAccessor values)
        {
            m_Values = values;
        }

        /// <summary>执行成员上的 [OnStateUpdate](表达式或普通方法名)。</summary>
        public void Run(object target, CachedMember m, HostSession session)
        {
            var updates = m.GetAttrs<OnStateUpdateAttribute>();
            if (updates.Length == 0) return;

            var state = new PropertyState
            {
                Expanded = session.GetToggle(target, m.RawName, true),
            };
            var value = m_Values.Read(m, target);

            foreach (var attr in updates)
            {
                if (string.IsNullOrEmpty(attr.Action)) continue;
                if (ExpressionEvaluator.IsExpression(attr.Action))
                {
                    ExpressionEvaluator.Evaluate(attr.Action, target, value, state);
                }
                else
                {
                    // 普通方法名支持: 直接调用
                    var method = m_Values.FindMethod(target, attr.Action);
                    if (method != null) m_Values.InvokeSafe(target, method);
                }
            }

            session.SetToggle(target, m.RawName, state.Expanded);
        }
    }
}
