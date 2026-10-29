using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEditor;

namespace RD.Draw.Editor.Selectors
{
    /// <summary>
    /// 类型选择器的**候选类型来源**与实例化 —— 选择器窗口与集合的"+"共用这一份规则。
    ///
    /// 候选 = 基类型自身(仅当它是可实例化的具体类) + 所有派生/实现类型, 规则:
    ///   - 抽象类 / 接口只列实现, 自身不是候选(实例化不了);
    ///   - 泛型定义、泛型参数未闭合、编译器生成的类型一律排除;
    ///   - Unity 对象(MonoBehaviour / ScriptableObject / Texture …)不参与类型选择 ——
    ///     它们该走对象选择器, 用类型选择器 new 出来的是游离实例, 不是资产/场景对象;
    ///   - 按基类型缓存: 域重载后 TypeCache 会重建, 进程内的静态缓存也随之失效。
    ///
    /// 排序: 基类型自身永远排第一, 其余按类型名(同名再按全名)升序 —— 用户最想选的
    /// 通常就是"声明类型本身", 把它压到列表最上面省一次搜索。
    /// </summary>
    public static class TypeCandidates
    {
        private static readonly Dictionary<Type, Type[]> s_Cache = new();

        /// <summary>取某个基类型的候选类型(已过滤、已排序、已缓存)。</summary>
        public static IReadOnlyList<Type> Get(Type baseType)
        {
            if (baseType == null) return Array.Empty<Type>();
            if (typeof(UnityEngine.Object).IsAssignableFrom(baseType)) return Array.Empty<Type>();
            if (s_Cache.TryGetValue(baseType, out var cached)) return cached;

            var types = new List<Type>();
            try
            {
                if (IsCandidate(baseType, baseType)) types.Add(baseType);

                foreach (var type in TypeCache.GetTypesDerivedFrom(baseType))
                {
                    if (IsCandidate(type, baseType)) types.Add(type);
                }
            }
            catch (Exception e)
            {
                // 泛型定义之类会让 TypeCache 抛异常: 没有候选也比把窗口炸掉强
                DrawLog.Warn($"[Inspector] 收集 {baseType.Name} 的派生类型失败\n{e.Message}");
            }

            types.Sort((a, b) =>
            {
                if (a == baseType) return -1;
                if (b == baseType) return 1;
                var byName = string.CompareOrdinal(a.Name, b.Name);
                return byName != 0 ? byName : string.CompareOrdinal(a.FullName, b.FullName);
            });

            var result = types.ToArray();
            s_Cache[baseType] = result;
            return result;
        }

        /// <summary>
        /// 这个元素类型在"新增"时是否要先弹类型选择器:
        /// **引用类型一律弹**(哪怕只有一个候选: 具体类自身也在候选里, 用户还得选它或 None),
        /// 只有值类型/枚举/string 直接创建, Unity 对象走对象选择器、不弹。
        /// 不想弹的用 <c>[ListDrawerSettings(AlwaysAddDefaultValue = true)]</c> 显式关掉。
        ///
        /// 抽象类/接口没有实现时不弹: 弹出来也只有一行 None, 没有意义。
        /// </summary>
        public static bool NeedsPicker(Type baseType)
        {
            if (baseType == null) return false;
            if (baseType.IsValueType || baseType.IsEnum || baseType == typeof(string)) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(baseType)) return false;

            return Get(baseType).Count > 0;
        }

        /// <summary>
        /// 按类型创建实例。无参构造函数优先(会跑字段初始化); 没有就退到"未初始化对象" ——
        /// 与序列化内核读回时的策略一致: 检视器要的是"有对象可填", 不是构造函数的副作用。
        /// 失败时返回 false 并记一条警告, 由调用方决定要不要往集合里加 null。
        /// </summary>
        public static bool TryCreate(Type type, out object instance)
        {
            instance = null;
            if (type == null || type.IsAbstract || type.IsInterface) return false;

            // string 没有无参构造函数: Activator / 未初始化对象都造不出来, 但"加一条空字符串"是常规操作
            if (type == typeof(string))
            {
                instance = string.Empty;
                return true;
            }

            if (type.IsValueType)
            {
                instance = Activator.CreateInstance(type);
                return true;
            }

            try
            {
                instance = Activator.CreateInstance(type, nonPublic: true);
                return true;
            }
            catch (Exception)
            {
                // 没有无参构造函数: 下面用未初始化对象兜底
            }

            try
            {
                instance = FormatterServices.GetUninitializedObject(type);
                return true;
            }
            catch (Exception e)
            {
                DrawLog.Warn($"[Inspector] 无法创建类型 {type.FullName}\n{e.Message}");
                return false;
            }
        }

        private static bool IsCandidate(Type type, Type baseType)
        {
            if (type == null) return false;
            if (type.IsAbstract || type.IsInterface) return false;
            if (type.ContainsGenericParameters || type.IsGenericTypeDefinition) return false;
            if (type.IsDefined(typeof(CompilerGeneratedAttribute), false)) return false;
            if (baseType.IsAssignableFrom(type) == false) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;
            return true;
        }
    }
}
