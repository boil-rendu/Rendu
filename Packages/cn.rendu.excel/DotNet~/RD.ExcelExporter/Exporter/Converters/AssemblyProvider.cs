using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD
{
    /// <summary>
    /// Assembly 提供者，管理配置类型的 Assembly 缓存
    /// </summary>
    public class AssemblyProvider
    {
        private readonly Dictionary<string, Assembly> _assemblies = new();

        /// <summary>
        /// 获取指定配置类型的 Assembly
        /// </summary>
        public Assembly GetAssembly(string excelRootDir, ConfigType configType)
        {
            var strategy = ConfigTypeStrategyFactory.GetStrategy(configType);
            string key = strategy.GetAssemblyKey();
            
            _assemblies.TryGetValue($"{excelRootDir}|{key}", out var assembly);
            return assembly;
        }

        /// <summary>
        /// 设置指定配置类型的 Assembly
        /// </summary>
        public void SetAssembly(string excelRootDir, ConfigType configType, Assembly assembly)
        {
            var strategy = ConfigTypeStrategyFactory.GetStrategy(configType);
            string key = strategy.GetAssemblyKey();
            
            _assemblies[$"{excelRootDir}|{key}"] = assembly;
        }
    }
}
