using System;
using RD.Draw;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// Float 数据值
    /// 承载 float；相等判断使用 0.0001 容差以规避浮点误差
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueFloat : UIDataValueBase<float>, IEquatable<UIDataValueFloat>
    {
        /// <summary>格式化为字符串</summary>
        public override string ToString()
        {
            return GetValue().ToString();
        }

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.Float;

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(float);

        #region 对比函数

        /// <summary>类对比：差值小于 0.0001 视为相等</summary>
        public bool Equals(UIDataValueFloat other)
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

            return Math.Abs(GetValue() - other.GetValue()) < 0.0001f;
        }

        /// <summary>值对比（0.0001 容差）</summary>
        protected override bool EqualsValue(float value)
        {
            return Math.Abs(GetValue() - value) < 0.0001f;
        }

        /// <summary>object 对比（0.0001 容差，有装箱拆箱开销）</summary>
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

            return Math.Abs(GetValue() - (float)obj) < 0.0001f;
        }

        /// <summary>取哈希码</summary>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueFloat lhs, UIDataValueFloat rhs)
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
        public static bool operator !=(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于</summary>
        public static bool operator >(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() > rhs.GetValue();
        }

        /// <summary>小于</summary>
        public static bool operator <(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() < rhs.GetValue();
        }

        /// <summary>大于等于</summary>
        public static bool operator >=(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() >= rhs.GetValue();
        }

        /// <summary>小于等于</summary>
        public static bool operator <=(UIDataValueFloat lhs, UIDataValueFloat rhs)
        {
            return lhs.GetValue() <= rhs.GetValue();
        }

        #endregion
    }
}
