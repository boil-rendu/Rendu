using System;
using System.Collections.Generic;
using System.Reflection;

namespace RD.Draw.Editor.Expressions
{
    /// <summary>
    /// 全局短类型名索引: 让表达式里可以直接写 "Vector3" 而不用写全限定名。
    ///
    /// 索引要扫全部程序集的 Type, 只建一次(受锁保护, 之后只读)。它与表达式语法、
    /// 求值都无关, 所以独立成类 —— 求值器因此不必知道"短名怎么解析"这件事。
    /// </summary>
    internal static class GlobalTypeIndex
    {
        /// <summary>短名 → 同名的所有类型(同名可能多个, 按程序集顺序取第一个命中的)。</summary>
        private static Dictionary<string, List<Type>> s_NameToTypes;

        /// <summary>建索引期间的写锁; 建好后只读, 无需加锁。</summary>
        private static readonly object s_IndexLock = new object();

        //-------------------------------------- 全局短名类型索引 --------------------------------------

        internal static void EnsureTypeIndex()
        {
            if (s_NameToTypes != null) return;
            lock (s_IndexLock)
            {
                if (s_NameToTypes != null) return;
                var map = new Dictionary<string, List<Type>>();
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.IsDynamic) continue;
                    Type[] types;
                    try
                    {
                        types = asm.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types;
                    }
                    catch
                    {
                        continue;
                    }

                    if (types == null) continue;
                    foreach (var t in types)
                    {
                        if (t == null) continue;
                        if (map.TryGetValue(t.Name, out var list) == false)
                        {
                            list = new List<Type>();
                            map[t.Name] = list;
                        }

                        if (list.Count < 8) list.Add(t);
                    }
                }

                s_NameToTypes = map;
            }
        }

        internal static Type FindGlobalType(string shortName)
        {
            EnsureTypeIndex();
            if (s_NameToTypes.TryGetValue(shortName, out var list) == false) return null;

            for (var i = 0; i < list.Count; i++)
            {
                var asm = list[i].Assembly.FullName ?? "";
                if (asm.StartsWith("UnityEngine", StringComparison.Ordinal) ||
                    asm.StartsWith("UnityEditor", StringComparison.Ordinal) ||
                    asm.StartsWith("mscorlib", StringComparison.Ordinal))
                {
                    continue;
                }

                if (asm.StartsWith("System.", StringComparison.Ordinal) &&
                    asm.StartsWith("System.Private.", StringComparison.Ordinal) == false)
                {
                    continue;
                }

                return list[i];
            }

            return list[0];
        }
    }
}
