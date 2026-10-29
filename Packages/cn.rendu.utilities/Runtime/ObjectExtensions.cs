using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Object = UnityEngine.Object;

namespace RD.Utilities
{
    /// <summary>
    /// 通用深拷贝(与 <c>SerializationUtility.CreateCopy</c> 同语义)。
    /// 反射逐字段复制(含私有字段 / 自动属性后备字段 / 继承成员), 数组 / <see cref="IList"/> /
    /// <see cref="IDictionary"/> 递归深拷, 环引用靠 visited 表兜底, 超深按 <see cref="MaxDepth"/> 截断。
    /// 不可变或不可复制的类型(string / 枚举 / 基元 / 委托 / <see cref="Object"/>)按引用共享。
    /// 调用方零改动: 目标类型没有实例 <c>Clone()</c> 时, <c>x.Clone()</c> 解析到本扩展方法。
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>递归深度上限, 超过后按浅拷贝兜底(防自引用链表之类的病态结构)。</summary>
        private const int MaxDepth = 64;

        private const BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        /// <summary>深拷贝任意对象。</summary>
        public static T Clone<T>(this T source)
        {
            return (T)DeepClone(source, new Dictionary<object, object>(ReferenceComparer.Instance), 0);
        }

        private static object DeepClone(object source, Dictionary<object, object> visited, int depth)
        {
            if (source == null) return null;

            var type = source.GetType();

            // 不可变 / 不可复制的按引用共享
            if (source is string || source is Enum || source is Delegate || source is Object ||
                type.IsPrimitive)
            {
                return source;
            }

            if (depth >= MaxDepth) return source;

            // 环引用: 已拷贝过的实例直接拿半成品(字段随后会补齐)
            if (visited.TryGetValue(source, out var cached)) return cached;

            if (source is Array array)
            {
                var clone = Array.CreateInstance(type.GetElementType(), array.Length);
                visited[source] = clone;
                for (var i = 0; i < array.Length; i++)
                {
                    clone.SetValue(DeepClone(array.GetValue(i), visited, depth + 1), i);
                }

                return clone;
            }

            if (source is IDictionary dictionary)
            {
                var clone = (IDictionary)CreateInstance(type);
                visited[source] = clone;
                foreach (DictionaryEntry entry in dictionary)
                {
                    clone.Add(DeepClone(entry.Key, visited, depth + 1),
                              DeepClone(entry.Value, visited, depth + 1));
                }

                return clone;
            }

            if (source is IList list)
            {
                var clone = (IList)CreateInstance(type);
                visited[source] = clone;
                foreach (var item in list)
                {
                    clone.Add(DeepClone(item, visited, depth + 1));
                }

                return clone;
            }

            // 普通类 / 结构: 逐字段深拷(自动属性的 &lt;X&gt;k__BackingField 也在字段里, 一并带走)
            var instance = CreateInstance(type);
            visited[source] = instance;
            foreach (var field in type.GetFields(FieldFlags))
            {
                if (field.IsLiteral) continue;   // const 无实例存储
                var value = field.GetValue(source);
                if (value == null) continue;
                field.SetValue(instance, DeepClone(value, visited, depth + 1));
            }

            return instance;
        }

        /// <summary>优先公有/私有无参构造; 只有带参构造的类型(POCO)走未初始化对象 + 字段灌值。</summary>
        private static object CreateInstance(Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);
            try
            {
                return Activator.CreateInstance(type, nonPublic: true);
            }
            catch
            {
                return FormatterServices.GetUninitializedObject(type);
            }
        }

        /// <summary>按引用 identity 比较(值类型每次装箱都是新对象, 进不了环, 无所谓)。</summary>
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
