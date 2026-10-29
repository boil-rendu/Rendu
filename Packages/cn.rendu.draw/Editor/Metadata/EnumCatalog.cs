using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Metadata
{
    /// <summary>
    /// 枚举目录: 某个枚举类型的全部取值(按声明顺序)与它们的显示文本。
    ///
    /// 两者都只跟枚举类型有关, 所以按类型缓存, 跨帧复用。
    /// </summary>
    internal sealed class EnumCatalog
    {
        private readonly ILabelFormatter m_Labels;
        private readonly Dictionary<Type, Array> m_ValuesCache = new();
        private readonly Dictionary<Type, string[]> m_LabelCache = new();
        private readonly Dictionary<Type, bool> m_FlagsCache = new();
        private readonly Dictionary<Type, long> m_FlagsMaskCache = new();

        public EnumCatalog(ILabelFormatter labels)
        {
            m_Labels = labels;
        }

        /// <summary>
        /// 是否 [Flags] 枚举。用缓存而不是每次 <c>type.IsDefined</c> —— 那是一次反射查询,
        /// 而枚举控件每帧都要走这一步。答案只跟类型有关, 没有失效的理由。
        /// </summary>
        public bool IsFlags(Type enumType)
        {
            if (enumType == null) return false;
            if (m_FlagsCache.TryGetValue(enumType, out var cached)) return cached;

            var isFlags = enumType.IsDefined(typeof(FlagsAttribute), false);
            m_FlagsCache[enumType] = isFlags;
            return isFlags;
        }

        /// <summary>
        /// 全部已定义正位的并集: [Flags] 枚举的"全选"判据(值 -1 或恰等于该并集的成员)。
        /// 位是常量, 算一次缓存起来即可。
        /// </summary>
        public long GetFlagsMask(Type enumType)
        {
            if (enumType == null) return 0L;
            if (m_FlagsMaskCache.TryGetValue(enumType, out var cached)) return cached;

            var mask = 0L;
            if (IsFlags(enumType))
            {
                var values = GetValues(enumType);
                for (var i = 0; i < values.Length; i++)
                {
                    var bit = Convert.ToInt64(values.GetValue(i));
                    if (bit > 0) mask |= bit;
                }
            }

            m_FlagsMaskCache[enumType] = mask;
            return mask;
        }

        /// <summary>
        /// 枚举的全部值, 按**声明顺序**返回 —— 不能用 <see cref="Enum.GetValues"/>: 它按数值大小排序,
        /// [Flags] 里声明在首位的"全部"(1<<30)会被扔到末尾, 页签顺序与声明顺序对不上。
        /// </summary>
        public Array GetValues(Type enumType)
        {
            if (enumType == null) return Array.Empty<object>();
            if (m_ValuesCache.TryGetValue(enumType, out var cached)) return cached;

            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            var values = Array.CreateInstance(enumType, fields.Length);
            for (var i = 0; i < fields.Length; i++)
            {
                // GetRawConstantValue 给的是底层整数值(int/long), 直接塞进枚举数组在 .NET 上会抛
                // InvalidCastException("Object cannot be stored in an array of this type"),
                // 必须先转成枚举值再写回。
                values.SetValue(Enum.ToObject(enumType, fields[i].GetRawConstantValue()), i);
            }

            m_ValuesCache[enumType] = values;
            return values;
        }

        /// <summary>枚举全部值的显示文本(与 <see cref="GetValues"/> 同序), 按类型缓存一次。</summary>
        public string[] GetLabels(Type enumType)
        {
            if (enumType == null) return Array.Empty<string>();
            if (m_LabelCache.TryGetValue(enumType, out var cached)) return cached;

            var values = GetValues(enumType);
            var labels = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                labels[i] = GetLabel(enumType, values.GetValue(i));
            }

            m_LabelCache[enumType] = labels;
            return labels;
        }

        private string GetLabel(Type enumType, object value)
        {
            try
            {
                var field = enumType.GetField(value.ToString());
                if (field != null)
                {
                    foreach (var attr in field.GetCustomAttributes(false))
                    {
                        if (attr is LabelTextAttribute label && string.IsNullOrEmpty(label.Text) == false)
                        {
                            return label.Text;
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }

            return m_Labels.FriendlyName(value.ToString());
        }
    }
}
