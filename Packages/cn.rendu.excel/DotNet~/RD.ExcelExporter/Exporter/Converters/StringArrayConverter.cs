using System;

namespace RD
{
    /// <summary>
    /// 字符串数组转换器
    /// </summary>
    public class StringArrayConverter : IValueConverter
    {
        private static readonly string[] SupportedTypes = { "string[]", "int[][]" };

        public bool CanConvert(string type) => Array.Exists(SupportedTypes, t => t == type);

        public string Convert(string value)
        {
            return $"[{value}]";
        }
    }
}
