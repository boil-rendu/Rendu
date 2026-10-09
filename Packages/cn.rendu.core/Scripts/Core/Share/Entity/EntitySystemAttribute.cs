using System;

namespace RD.Core
{
    /// <summary>
    /// 标记 System 类为「实体 System」，<see cref="EntitySystemSingleton"/> 在 <c>Awake</c> 扫描时通过反射找到带此特性的类并注册到 <c>TypeSystems</c>。
    /// 可应用于类和方法。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EntitySystemAttribute: BaseAttribute
    {
    }
}
