using System;
using RD.Draw;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// Bool 数据值
    /// 承载 bool；大小比较（&gt;、&lt;、&gt;=、&lt;=）无意义，调用时会打印错误并返回 false
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueBool : UIDataValueBase<bool>, IEquatable<UIDataValueBool>
    {
        /// <summary>格式化为 "True" / "False"</summary>
        public override string ToString()
        {
            return GetValue() ? "True" : "False";
        }

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(bool);

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.Bool;

        #region 对比函数

        //类对比
        /// <summary>类对比：同一个 bool 值相等即视为相等</summary>
        public bool Equals(UIDataValueBool other)
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

        //值对比
        /// <summary>值对比：与裸值比较是否相等</summary>
        protected override bool EqualsValue(bool value)
        {
            return GetValue() == value;
        }

        //值对比 少用 这个有装箱拆箱的性能开销
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

            return GetValue() == (bool)obj;
        }

        /// <summary>取哈希码</summary>
        public override int GetHashCode()
        {
            return GetValue().GetHashCode();
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueBool lhs, UIDataValueBool rhs)
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
        public static bool operator !=(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于：bool 无意义，打印错误</summary>
        public static bool operator >(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 >");
            return false;
        }

        /// <summary>小于：bool 无意义，打印错误</summary>
        public static bool operator <(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 <");
            return false;
        }

        /// <summary>大于等于：bool 无意义，打印错误</summary>
        public static bool operator >=(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 >=");
            return false;
        }

        /// <summary>小于等于：bool 无意义，打印错误</summary>
        public static bool operator <=(UIDataValueBool lhs, UIDataValueBool rhs)
        {
            Logger.LogError($"注意你正在对比2个bool的 大小 <=");
            return false;
        }

        #endregion
    }
}
