using System;
using System.Collections.Generic;
using System.Linq;
using RD.EditorKit;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// List&lt;long&gt; 数据值
    /// 承载长整型列表；写入时原地 Clear+AddRange 复用容器；比较按元素逐一相等；大小比较按元素个数
    /// </summary>
    [HideLabel]
    [HideReferenceObjectPicker]
    [Serializable]
    public class UIDataValueListLong : UIDataValueBase<List<long>>, IEquatable<UIDataValueListLong>
    {
        /// <summary>格式化为字符串（直接调用 List.ToString，非逐元素拼接）</summary>
        public override string ToString()
        {
            return GetValue().ToString();
        }

        /// <summary>对应的绑定数据类型</summary>
        public override EUIBindDataType UIBindDataType => EUIBindDataType.List_Long;

        /// <summary>构造：初始化为空列表</summary>
        public UIDataValueListLong()
        {
            if (GetValue() == null)
            {
                SetValue(new List<long>());
            }
        }

        /// <summary>承载的运行时类型</summary>
        public override Type UIDataValueType => typeof(List<long>);

        /// <summary>写入值：容器已存在时原地 Clear + AddRange（避免每帧产生新列表）</summary>
        protected override void SetValueFrom(List<long> value)
        {
            if (GetValue() == null)
            {
                base.SetValueFrom(value);
                return;
            }

            GetValue().Clear();
            GetValue().AddRange(value);
        }

        #region 对比函数

        /// <summary>逐元素比较列表是否相等（长度相同且对应元素都相等）</summary>
        private bool EqualsList(List<long> value)
        {
            if (GetValue()?.Count != value.Count)
            {
                return false;
            }

            return !GetValue().Where((t, i) => t != value[i]).Any();
        }

        /// <summary>类对比：列表元素逐一相等即视为相等</summary>
        public bool Equals(UIDataValueListLong other)
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

            return EqualsList(other.GetValue());
        }

        /// <summary>值对比：逐元素比较</summary>
        protected override bool EqualsValue(List<long> value)
        {
            return EqualsList(value);
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

            var value = (List<long>)obj;

            return EqualsList(value);
        }

        /// <summary>取哈希码（null 返回 0）</summary>
        public override int GetHashCode()
        {
            return GetValue()?.GetHashCode() ?? 0;
        }

        /// <summary>相等运算符</summary>
        public static bool operator ==(UIDataValueListLong lhs, UIDataValueListLong rhs)
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
        public static bool operator !=(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>大于：按元素个数比较</summary>
        public static bool operator >(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count > rhs.GetValue().Count;
        }

        /// <summary>小于：按元素个数比较</summary>
        public static bool operator <(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count < rhs.GetValue().Count;
        }

        /// <summary>大于等于：按元素个数比较</summary>
        public static bool operator >=(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count >= rhs.GetValue().Count;
        }

        /// <summary>小于等于：按元素个数比较</summary>
        public static bool operator <=(UIDataValueListLong lhs, UIDataValueListLong rhs)
        {
            return lhs.GetValue().Count <= rhs.GetValue().Count;
        }

        #endregion
    }
}
