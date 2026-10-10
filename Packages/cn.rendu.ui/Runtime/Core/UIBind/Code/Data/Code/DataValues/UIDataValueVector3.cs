using System;
using RD.EditorKit;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// Vector3 数据值
    /// 承载 Vector3；大小比较（&gt;、&lt;、&gt;=、&lt;=）无意义，恒返回 false
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueVector3 : UIDataValueBase<Vector3>, IEquatable<UIDataValueVector3>
    {
        /// <summary>格式化为 "X:.. Y:.. Z:.."</summary>
        public override string ToString()
        {
            var value = GetValue();
            return $"X:{value.x} Y:{value.y} Z:{value.z}";
        }

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.Vector3;

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(Vector3);

        #region 对比函数

        /// <summary>类对比：向量相等即视为相等</summary>
        public bool Equals(UIDataValueVector3 other)
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
        protected override bool EqualsValue(Vector3 value)
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

            return GetValue() == (Vector3)obj;
        }

        /// <summary>取哈希码</summary>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
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
        public static bool operator !=(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于：向量无意义，恒 false</summary>
        public static bool operator >(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        /// <summary>小于：向量无意义，恒 false</summary>
        public static bool operator <(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        /// <summary>大于等于：向量无意义，恒 false</summary>
        public static bool operator >=(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        /// <summary>小于等于：向量无意义，恒 false</summary>
        public static bool operator <=(UIDataValueVector3 lhs, UIDataValueVector3 rhs)
        {
            return false;
        }

        #endregion
    }
}
