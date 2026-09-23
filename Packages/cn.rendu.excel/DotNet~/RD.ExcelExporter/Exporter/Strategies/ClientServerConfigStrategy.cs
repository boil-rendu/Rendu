using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD
{
    /// <summary>
    /// ClientServer 配置类型策略（共享）
    /// cs配置 → 导出所有平台
    /// </summary>
    public class ClientServerConfigStrategy : IConfigTypeStrategy
    {
        public ConfigType Type => ConfigType.cs;
        public string GetAssemblyKey() => "ClientServer";

        public bool ShouldGenerate(Table table)
        {
            // cs配置（table.C && table.S）或任何配置都导出到ClientServer目录
            return table.C || table.S;
        }

        public bool TryGetAssembly(Dictionary<string, Assembly> assemblies, out Assembly assembly)
        {
            return assemblies.TryGetValue(GetAssemblyKey(), out assembly);
        }
    }
}
