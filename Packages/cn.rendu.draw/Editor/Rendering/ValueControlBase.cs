using System;
using UnityEngine;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 值画法的共享骨架。
    ///
    /// 目前只抽了两条确实跨多个画法重复的知识:
    ///   - "有 [MinValue]+[MaxValue] 就画滑杆, 否则画输入框" 的判据;
    ///   - 建树器的传递。
    /// 其余各画法形态差异很大, 强行统一只会造出一个处处带默认分支的巨类。
    /// </summary>
    internal abstract class ValueControlBase : IValueControl
    {
        protected readonly WidgetBuilder Builder;

        protected ValueControlBase(WidgetBuilder builder)
        {
            Builder = builder;
        }

        public abstract bool CanHandle(in ValueDrawArgs args);

        public abstract void Build(in ValueDrawArgs args);

        /// <summary>
        /// 取滑杆区间: <c>[MinValue]</c> 与 <c>[MaxValue]</c> **同时存在且都是字面值**时才用滑杆。
        ///
        /// 两者的 MemberName 形式(= 另一个成员的当前值)是运行期数值, 在即时模式下每帧都会变,
        /// 拿它当滑杆上下限会让控件两端抖动, 因此那种形式仍然走输入框。
        /// </summary>
        protected static bool TrySliderRange(CachedMember member, out float min, out float max)
        {
            min = 0f;
            max = 0f;

            var minAttr = member.GetAttr<MinValueAttribute>();
            var maxAttr = member.GetAttr<MaxValueAttribute>();
            if (minAttr == null || maxAttr == null) return false;
            if (minAttr.MemberName != null || maxAttr.MemberName != null) return false;

            min = Convert.ToSingle(minAttr.MinValue);
            max = Convert.ToSingle(maxAttr.MaxValue);
            return true;
        }

        /// <summary>标量无值时的兜底(Unity 侧对 null 的宽类型字段常走这条路)。</summary>
        protected static float NumberOrZero(object value)
        {
            return value == null ? 0f : Convert.ToSingle(value);
        }

        /// <summary>整数无值时的兜底。</summary>
        protected static long IntegerOrZero(object value)
        {
            return value == null ? 0L : Convert.ToInt64(value);
        }
    }
}