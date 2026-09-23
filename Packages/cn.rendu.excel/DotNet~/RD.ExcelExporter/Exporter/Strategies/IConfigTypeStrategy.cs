using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD
{
    /// <summary>
    /// 配置类型策略接口
    /// </summary>
    public interface IConfigTypeStrategy
    {
        /// <summary>
        /// 配置类型
        /// </summary>
        ConfigType Type { get; }
        /// <summary>
        /// 是否应该为该表生成配置
        /// </summary>
        bool ShouldGenerate(Table table);
        /// <summary>
        /// 获取 Assembly 字典的键
        /// </summary>
        string GetAssemblyKey();
        /// <summary>
        /// 尝试获取 Assembly
        /// </summary>
        bool TryGetAssembly(Dictionary<string, Assembly> assemblies, out Assembly assembly);
    }
}
