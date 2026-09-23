namespace RD
{
    /// <summary>
    /// 字符串转换器
    /// </summary>
    public class StringConverter : IValueConverter
    {
        public bool CanConvert(string type) => type == "string";

        public string Convert(string value)
        {
            var v = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"\"{v}\"";
        }
    }
}
