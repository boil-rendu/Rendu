using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD
{
    /// <summary>
    /// Client 配置类型策略
    /// c配置或cs配置 → 导出
    /// </summary>
    public class ClientConfigStrategy : IConfigTypeStrategy
    {
        public ConfigType Type => ConfigType.c;
        public string GetAssemblyKey() => "Client";

        public bool ShouldGenerate(Table table)
        {
            // c配置（table.C）或cs配置（table.C && table.S）都导出到Client目录
            return table.C;
        }

        public bool TryGetAssembly(Dictionary<string, Assembly> assemblies, out Assembly assembly)
        {
            return assemblies.TryGetValue(GetAssemblyKey(), out assembly);
        }
    }
}
