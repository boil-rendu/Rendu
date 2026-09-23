using System;

namespace RD
{
    /// <summary>
    /// 整数数组转换器
    /// </summary>
    public class IntegerArrayConverter : IValueConverter
    {
        private static readonly string[] SupportedTypes = { "uint[]", "int[]", "int32[]", "long[]" };

        public bool CanConvert(string type) => Array.Exists(SupportedTypes, t => t == type);

        public string Convert(string value)
        {
            return string.IsNullOrEmpty(value) ? "[0]" : $"[{value}]";
        }
    }
}
