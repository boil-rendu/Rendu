using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD
{
    /// <summary>
    /// Server 配置类型策略
    /// s配置或cs配置 → 导出
    /// </summary>
    public class ServerConfigStrategy : IConfigTypeStrategy
    {
        public ConfigType Type => ConfigType.s;
        public string GetAssemblyKey() => "Server";

        public bool ShouldGenerate(Table table)
        {
            // s配置（table.S）或cs配置（table.C && table.S）都导出到Server目录
            return table.S;
        }

        public bool TryGetAssembly(Dictionary<string, Assembly> assemblies, out Assembly assembly)
        {
            return assemblies.TryGetValue(GetAssemblyKey(), out assembly);
        }
    }
}
