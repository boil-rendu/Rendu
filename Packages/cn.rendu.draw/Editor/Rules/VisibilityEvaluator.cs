using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// 显隐 / 禁用判定。
    ///
    /// 这些都是纯规则(给定成员与目标对象, 得出"画不画""能不能编辑"),
    /// 不碰任何 UI, 因此可以脱离 Unity 运行环境被测试 —— 所以 isPlaying 由调用方传入。
    /// </summary>
    internal sealed class VisibilityEvaluator
    {
        private readonly ValueAccessor m_Values;

        public VisibilityEvaluator(ValueAccessor values)
        {
            m_Values = values;
        }

        /// <summary>成员是否可见([HideInPlayMode] / [HideInEditorMode] / [ShowIf] / [HideIf])。</summary>
        public bool IsVisible(CachedMember m, object target, bool isPlaying)
        {
            // HideInPlayMode / HideInEditorMode 按当前是否运行中隐藏
            if (isPlaying)
            {
                if (m.GetAttr<HideInPlayModeAttribute>() != null) return false;
            }
            else if (m.GetAttr<HideInEditorModeAttribute>() != null)
            {
                return false;
            }

            foreach (var attr in m.GetAttrs<ShowIfAttribute>())
            {
                var expect = attr.Value;
                var actual = ConditionValue(target, attr.MemberName);
                if (expect != null)
                {
                    if (ValuesEqual(actual, expect) == false) return false;
                }
                else if (Truthy(actual) == false)
                {
                    return false;
                }
            }

            foreach (var attr in m.GetAttrs<HideIfAttribute>())
            {
                var expect = attr.Value;
                var actual = ConditionValue(target, attr.MemberName);
                if (expect != null)
                {
                    if (ValuesEqual(actual, expect)) return false;
                }
                else if (Truthy(actual))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>成员是否被禁用([DisableIf] / [EnableIf])。</summary>
        public bool IsDisabled(CachedMember m, object target)
        {
            foreach (var attr in m.GetAttrs<DisableIfAttribute>())
            {
                var actual = ConditionValue(target, attr.MemberName);
                var disabled = attr.Value != null ? ValuesEqual(actual, attr.Value) : Truthy(actual);
                if (disabled) return true;
            }

            var enableIf = m.GetAttrs<EnableIfAttribute>();
            if (enableIf.Length > 0)
            {
                foreach (var attr in enableIf)
                {
                    var actual = ConditionValue(target, attr.MemberName);
                    var ok = attr.Value != null ? ValuesEqual(actual, attr.Value) : Truthy(actual);
                    if (ok == false) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 组是否应该绘制。
        /// VisibleIf 为假时整组消失, 这一条与 HideWhenChildrenAreInvisible 无关 ——
        /// 否则一个"子项齐全但条件不满足"的组会退化成一个空框。
        /// </summary>
        public bool IsGroupVisible(GroupNode node, object target, bool isPlaying)
        {
            if (string.IsNullOrEmpty(node.VisibleIf) == false &&
                Truthy(ConditionValue(target, node.VisibleIf)) == false)
            {
                return false;
            }

            return node.HideWhenChildrenAreInvisible == false || HasVisibleContent(node, target, isPlaying);
        }

        private bool HasVisibleContent(GroupNode node, object target, bool isPlaying)
        {
            foreach (var m in node.Members)
            {
                if (IsVisible(m, target, isPlaying)) return true;
            }

            foreach (var child in node.Children)
            {
                // 走 IsGroupVisible 而不是单看 HideWhenChildrenAreInvisible:
                // 子组自己的 VisibleIf 为假时它整体不画, 这里就不能算作"父组还有内容"。
                if (IsGroupVisible(child, target, isPlaying)) return true;
            }

            return false;
        }

        /// <summary>
        /// 标题/提示类批注的条件显隐。
        /// visibleIfMemberName 为空 = 无条件显示; 支持 @ 表达式与裸成员名(同 [ShowIf])。
        /// </summary>
        public bool IsAnnotationVisible(string visibleIfMemberName, object target)
        {
            if (string.IsNullOrEmpty(visibleIfMemberName)) return true;
            return Truthy(ConditionValue(target, visibleIfMemberName));
        }

        /// <summary>条件成员求值: 支持以 @ 开头的表达式与普通成员名。</summary>
        private object ConditionValue(object target, string memberName)
        {
            if (ExpressionEvaluator.IsExpression(memberName))
            {
                return ExpressionEvaluator.Evaluate(memberName, target, null, null);
            }

            return m_Values.GetMemberValue(target, memberName);
        }

        // 真值/相等语义与表达式求值器共用一套实现(Value), 否则同一个值在
        // [ShowIf("X", v)] 与 @X == v 两处会得到不同结论。
        private static bool Truthy(object value)
        {
            return ValueOps.Truthy(value);
        }

        private static bool ValuesEqual(object a, object b)
        {
            return ValueOps.Equal(a, b);
        }
    }
}
