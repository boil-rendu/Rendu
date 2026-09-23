namespace RD
{
    /// <summary>
    /// 值转换器接口
    /// </summary>
    public interface IValueConverter
    {
        /// <summary>
        /// 是否支持该类型
        /// </summary>
        bool CanConvert(string type);

        /// <summary>
        /// 转换值
        /// </summary>
        string Convert(string value);
    }
}
