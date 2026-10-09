using System;

namespace RD.Core
{
    /// <summary>
    /// System 类的元信息接口：标识一个 System 服务于哪个实体类型（<see cref="Type"/>）以及注册到哪个系统键（<see cref="SystemType"/>）。
    /// 由 <see cref="EntitySystemSingleton"/> 在 <c>Awake</c> 阶段扫描并注册到 <c>TypeSystems</c>。
    /// </summary>
    public interface ISystemType
    {
        /// <summary>目标实体类型（按此类型索引系统）。</summary>
        Type Type();

        /// <summary>系统类型键（如 <c>IAwakeSystem</c>、<c>IAwakeSystem&lt;A&gt;</c>），用于在 <c>TypeSystems.Map</c> 内查表。</summary>
        Type SystemType();
    }
}
