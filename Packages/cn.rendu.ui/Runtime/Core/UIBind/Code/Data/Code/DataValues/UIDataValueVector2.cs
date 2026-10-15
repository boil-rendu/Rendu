using System;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// Vector2 数据值
    /// 承载 Vector2；大小比较（&gt;、&lt;、&gt;=、&lt;=）无意义，恒返回 false
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueVector2 : UIDataValueBase<Vector2>, IEquatable<UIDataValueVector2>
    {
        /// <summary>格式化为 "X:.. Y:.."</summary>
        public override string ToString()
        {
            var value = GetValue();
            return $"X:{value.x} Y:{value.y}";
        }

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.Vector2;

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(Vector2);

        #region 对比函数

        /// <summary>类对比：向量相等即视为相等</summary>
        public bool Equals(UIDataValueVector2 other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return GetValue() == other.GetValue();
        }

        /// <summary>值对比</summary>
        protected override bool EqualsValue(Vector2 value)
        {
            return GetValue() == value;
        }

        /// <summary>object 对比（有装箱拆箱开销）</summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != UIDataValueType)
            {
                return false;
            }

            return GetValue() == (Vector2)obj;
        }

        /// <summary>取哈希码</summary>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }

        /// <summary>不等运算符</summary>
        public static bool operator !=(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于：向量无意义，恒 false</summary>
        public static bool operator >(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        /// <summary>小于：向量无意义，恒 false</summary>
        public static bool operator <(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        /// <summary>大于等于：向量无意义，恒 false</summary>
        public static bool operator >=(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        /// <summary>小于等于：向量无意义，恒 false</summary>
        public static bool operator <=(UIDataValueVector2 lhs, UIDataValueVector2 rhs)
        {
            return false;
        }

        #endregion
    }
}
