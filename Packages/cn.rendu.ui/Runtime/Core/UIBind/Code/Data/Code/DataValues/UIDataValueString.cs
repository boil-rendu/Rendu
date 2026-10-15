using System;
using RD.Draw;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// String 数据值
    /// 承载 string；大小比较（&gt;、&lt;、&gt;=、&lt;=）无意义，恒返回 false
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueString : UIDataValueBase<string>, IEquatable<UIDataValueString>
    {
        /// <summary>格式化为字符串（null 视为空串）</summary>
        public override string ToString()
        {
            return GetValue() ?? "";
        }

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.String;

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(string);

        #region 对比函数

        /// <summary>类对比：同一字符串值相等即视为相等</summary>
        public bool Equals(UIDataValueString other)
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
        protected override bool EqualsValue(string value)
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

            return GetValue() == (string)obj;
        }

        /// <summary>取哈希码（null 返回 0）</summary>
        public override int GetHashCode()
        {
            return GetValue()?.GetHashCode() ?? 0;
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueString lhs, UIDataValueString rhs)
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
        public static bool operator !=(UIDataValueString lhs, UIDataValueString rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于：字符串无意义，恒 false</summary>
        public static bool operator >(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        /// <summary>小于：字符串无意义，恒 false</summary>
        public static bool operator <(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        /// <summary>大于等于：字符串无意义，恒 false</summary>
        public static bool operator >=(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        /// <summary>小于等于：字符串无意义，恒 false</summary>
        public static bool operator <=(UIDataValueString lhs, UIDataValueString rhs)
        {
            return false;
        }

        #endregion
    }
}
