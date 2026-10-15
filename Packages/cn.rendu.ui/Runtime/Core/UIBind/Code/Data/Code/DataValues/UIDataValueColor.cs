using System;
using RD.Draw;
using UnityEngine;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// Color 数据值
    /// 承载 Color；默认值为白色；大小比较（&gt;、&lt;、&gt;=、&lt;=）无意义，调用时会打印错误
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueColor : UIDataValueBase<Color>, IEquatable<UIDataValueColor>
    {
        /// <summary>格式化为字符串</summary>
        public override string ToString()
        {
            return GetValue().ToString();
        }

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.Color;

        /// <summary>构造：默认值为白色</summary>
        public UIDataValueColor()
        {
            SetValue(Color.white);
        }

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(Color);

        #region 对比函数

        /// <summary>类对比：颜色相等即视为相等</summary>
        public bool Equals(UIDataValueColor other)
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
        protected override bool EqualsValue(Color value)
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

            return GetValue() == (Color)obj;
        }

        /// <summary>取哈希码</summary>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueColor lhs, UIDataValueColor rhs)
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
        public static bool operator !=(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于：颜色无意义，打印错误</summary>
        public static bool operator >(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        /// <summary>小于：颜色无意义，打印错误</summary>
        public static bool operator <(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        /// <summary>大于等于：颜色无意义，打印错误</summary>
        public static bool operator >=(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        /// <summary>小于等于：颜色无意义，打印错误</summary>
        public static bool operator <=(UIDataValueColor lhs, UIDataValueColor rhs)
        {
            Logger.LogError($"请不要比较这个类型 {EUIBindDataType.Color}");
            return false;
        }

        #endregion
    }
}
