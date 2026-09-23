using System;
using System.Collections.Generic;
using System.Linq;

namespace RD
{
    /// <summary>
    /// 值转换器 - 使用策略模式管理各种类型转换
    /// </summary>
    public static class ValueConverter
    {
        private static readonly List<IValueConverter> _converters = new List<IValueConverter>
        {
            new IntegerArrayConverter(),
            new StringArrayConverter(),
            new NumericConverter(),
            new StringConverter(),
            new BooleanConverter()
        };

        /// <summary>
        /// 将指定类型的值转换为目标格式字符串
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <param name="value">原始值</param>
        /// <returns>转换后的字符串</returns>
        public static string Convert(string type, string value)
        {
            var converter = _converters.FirstOrDefault(c => c.CanConvert(type));
            
            if (converter == null)
                throw new Exception($"不支持此类型: {type}");

            return converter.Convert(value);
        }
    }
}
