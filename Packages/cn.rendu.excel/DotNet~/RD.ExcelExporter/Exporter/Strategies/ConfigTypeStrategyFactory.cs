using System;
using System.Collections.Generic;

namespace RD
{
    /// <summary>
    /// 配置类型策略工厂
    /// </summary>
    public static class ConfigTypeStrategyFactory
    {
        private static readonly Dictionary<ConfigType, IConfigTypeStrategy> _strategies = new Dictionary<ConfigType, IConfigTypeStrategy>
        {
            { ConfigType.c, new ClientConfigStrategy() },
            { ConfigType.s, new ServerConfigStrategy() },
            { ConfigType.cs, new ClientServerConfigStrategy() }
        };

        /// <summary>
        /// 获取指定配置类型的策略
        /// </summary>
        public static IConfigTypeStrategy GetStrategy(ConfigType configType)
        {
            return _strategies[configType];
        }
    }
}
