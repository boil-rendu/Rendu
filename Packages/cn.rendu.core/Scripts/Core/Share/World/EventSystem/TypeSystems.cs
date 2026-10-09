using System;
using System.Collections.Generic;

namespace RD.Core
{
    /// <summary>
    /// 二维索引：按「实体/类型」<c>type</c> ×「<see cref="ISystemType"/>」<c>systemType</c> 维护 <see cref="SystemObject"/> 列表，
    /// 由 <see cref="EntitySystemSingleton"/> 持有，被 <see cref="EntitySystem.Publish{T}"/> 派发时按实体类型查找处理函数。
    /// </summary>
    /// <remarks>
    /// 每个 <see cref="OneTypeSystems"/> 内：
    /// <list type="bullet">
    /// <item><c>Map</c>：<c>systemType -> SystemObject 列表</c>（handler 主体）。</item>
    /// <item><c>ClassType</c>：该类型订阅的所有事件 <c>systemType</c> 列表，供 <see cref="EntitySystem.RegisterSystem"/> 入队时遍历。</item>
    /// </list>
    /// </remarks>
    public class TypeSystems
    {
        /// <summary>单个类型在 <see cref="TypeSystems"/> 中的子索引。</summary>
        public class OneTypeSystems
        {
            /// <summary>事件 <c>systemType</c> → 该事件注册到的 <see cref="SystemObject"/> 列表（<c>Map</c>）。</summary>
            public readonly UnOrderMultiMap<Type, SystemObject> Map = new();
            // 这里不用hash，数量比较少，直接for循环速度更快
            /// <summary>该类型订阅的所有事件 <c>systemType</c> 列表（<c>List</c>，供 <see cref="EntitySystem.RegisterSystem"/> 入队时遍历）。</summary>
            public readonly List<Type> ClassType = new();
        }

        private readonly Dictionary<Type, OneTypeSystems> typeSystemsMap = new();

        /// <summary>取出或新建类型 <paramref name="type"/> 对应的 <see cref="OneTypeSystems"/>（不存在则创建并缓存）。</summary>
        public OneTypeSystems GetOrCreateOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            if (systems != null)
            {
                return systems;
            }

            systems = new OneTypeSystems();
            this.typeSystemsMap.Add(type, systems);
            return systems;
        }

        /// <summary>取类型 <paramref name="type"/> 的 <see cref="OneTypeSystems"/>，不存在返回 <c>null</c>。</summary>
        public OneTypeSystems GetOneTypeSystems(Type type)
        {
            OneTypeSystems systems = null;
            this.typeSystemsMap.TryGetValue(type, out systems);
            return systems;
        }

        /// <summary>按 (实体类型, 事件 systemType) 取该类型对该事件注册的所有 <see cref="SystemObject"/> 列表，未注册返回 <c>null</c>。</summary>
        public List<SystemObject> GetSystems(Type type, Type systemType)
        {
            OneTypeSystems oneTypeSystems = null;
            if (!this.typeSystemsMap.TryGetValue(type, out oneTypeSystems))
            {
                return null;
            }

            if (!oneTypeSystems.Map.TryGetValue(systemType, out List<SystemObject> systems))
            {
                return null;
            }

            return systems;
        }
    }
}
