using System;
using RD.Draw;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// Int 数据值
    /// 承载 int；支持 &gt;、&lt;、&gt;=、&lt;= 大小比较
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueInt : UIDataValueBase<int>, IEquatable<UIDataValueInt>
    {
        /// <summary>格式化为字符串</summary>
        public override string ToString()
        {
            return GetValue().ToString();
        }

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(int);

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.Int;

        #region 对比函数

        /// <summary>类对比：同一整数值相等即视为相等</summary>
        public bool Equals(UIDataValueInt other)
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
        protected override bool EqualsValue(int value)
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

            return GetValue() == (int)obj;
        }

        /// <summary>取哈希码</summary>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueInt lhs, UIDataValueInt rhs)
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
        public static bool operator !=(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于</summary>
        public static bool operator >(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() > rhs.GetValue();
        }

        /// <summary>小于</summary>
        public static bool operator <(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() < rhs.GetValue();
        }

        /// <summary>大于等于</summary>
        public static bool operator >=(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() >= rhs.GetValue();
        }

        /// <summary>小于等于</summary>
        public static bool operator <=(UIDataValueInt lhs, UIDataValueInt rhs)
        {
            return lhs.GetValue() <= rhs.GetValue();
        }

        #endregion
    }
}
