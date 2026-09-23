namespace RD
{
    /// <summary>
    /// 配置类型枚举
    /// </summary>
    public enum ConfigType
    {
        c = 0,
        s = 1,
        cs = 2,
    }

    /// <summary>
    /// ConfigType 扩展方法
    /// </summary>
    public static class ConfigTypeExtensions
    {
        /// <summary>
        /// 检查配置类型是否包含指定的标志
        /// </summary>
        public static bool HasFlag(this ConfigType configType, ConfigType flag)
        {
            return (configType & flag) == flag;
        }
    }
}
