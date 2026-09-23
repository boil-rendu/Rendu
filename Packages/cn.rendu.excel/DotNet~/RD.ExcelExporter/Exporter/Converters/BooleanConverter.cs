namespace RD
{
    /// <summary>
    /// 布尔值转换器
    /// </summary>
    public class BooleanConverter : IValueConverter
    {
        public bool CanConvert(string type) => type == "bool";

        public string Convert(string value)
        {
            if (value == "1") return "true";
            if (value == "0" || string.IsNullOrEmpty(value)) return "false";
            return value;
        }
    }
}
