using System;

namespace RD
{
    /// <summary>
    /// 数值转换器
    /// </summary>
    public class NumericConverter : IValueConverter
    {
        private static readonly string[] SupportedTypes = { "int", "uint", "int32", "int64", "long", "float", "double" };

        public bool CanConvert(string type) => Array.Exists(SupportedTypes, t => t == type);

        public string Convert(string value)
        {
            return string.IsNullOrEmpty(value) ? "0" : value;
        }
    }
}
