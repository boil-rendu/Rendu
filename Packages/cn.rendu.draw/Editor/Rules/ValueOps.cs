using System;
using System.Collections;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// 值的**语义**判定: 真假 / 相等 / 比大小 —— 表达式求值器与条件显隐共用同一套。
    ///
    /// 它之所以独立成一个"值的算子"类, 而不是散在解释器与可见性判定里各判一次:
    /// 同一个值在 [ShowIf] 里判真假、在 @表达式里判相等、在表格里又判一次, 三处若各写一份,
    /// 就会出现"表达式里相等、显隐里不相等"这种没法解释的行为。
    ///
    /// 注意这里的相等是**带容差的相等**, 与 <c>object.Equals</c> 不是一回事 ——
    /// 字段常是 float 而字面量是 double, 直接比会给出违反直觉的结果。
    /// </summary>
    internal static class ValueOps
    {
        /// <summary>
        /// 数值比较容差（相对）。
        /// 用相对容差而不是 == 或 <c>double.Epsilon</c>：字段常是 float，而特性参数与表达式里的
        /// 字面量是 double，<c>0.1f</c> 转 double 是 0.100000001490116…，与 0.1 差约 1.5e-9。
        /// 容差随量级放大也是必需的：1e9 附近的 float 相邻值间隔远大于任何固定绝对容差。
        /// </summary>
        internal const double NumericTolerance = 1e-6;

        /// <summary>布尔上下文：null / false / 空串 / 空集合 / 已销毁的 Unity 对象为假，其余为真（数值 0 也为真）。</summary>
        internal static bool Truthy(object value)
        {
            switch (value)
            {
                case null:
                    return false;
                case bool b:
                    return b;
                case string s:
                    return string.IsNullOrEmpty(s) == false;
                case UnityEngine.Object u:
                    return u != null;
                case ICollection c:
                    return c.Count > 0;
                default:
                    return true;
            }
        }

        /// <summary>是否为数值类型（枚举与 bool 不算）。</summary>
        internal static bool IsNumeric(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong ||
                   value is float || value is double || value is decimal;
        }

        /// <summary>相等判定：数值带容差比较，枚举可与字符串名同义比较。</summary>
        internal static bool Equal(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (Equals(a, b)) return true;

            if (IsNumeric(a) && IsNumeric(b))
            {
                var left  = Convert.ToDouble(a);
                var right = Convert.ToDouble(b);
                var scale = Math.Max(1d, Math.Max(Math.Abs(left), Math.Abs(right)));
                return Math.Abs(left - right) <= NumericTolerance * scale;
            }

            if (a is Enum && b is string) return a.ToString() == b.ToString();
            if (b is Enum && a is string) return b.ToString() == a.ToString();
            return false;
        }

        /// <summary>比大小, 供表达式的 &lt; &gt; &lt;= &gt;= 使用。</summary>
        /// <remarks>
        /// 数值按数值比(整型与浮点可以互相比较), 否则按字符串的序号比 ——
        /// 表达式里出现对象比较时这比抛异常有用。注意相等(==)不走这里, 见 <see cref="Equal"/>。
        /// </remarks>
        internal static int Compare(object a, object b)
        {
            if (IsNumeric(a) && IsNumeric(b)) return Convert.ToDouble(a).CompareTo(Convert.ToDouble(b));
            if (a is string sa && b is string sb) return string.Compare(sa, sb, StringComparison.Ordinal);
            if (a is bool ba && b is bool bb) return ba.CompareTo(bb);
            return string.Compare(a?.ToString() ?? "", b?.ToString() ?? "", StringComparison.Ordinal);
        }
    }
}