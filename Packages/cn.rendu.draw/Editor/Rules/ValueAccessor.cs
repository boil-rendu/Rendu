using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// 值的读写与成员查找。
    ///
    /// 引擎里"读一个值"有好几条路径(成员元数据 / 成员名 / 方法), 它们的查找规则与缓存
    /// 必须一致 —— 否则同一个名字在 [ShowIf] 与 @ 表达式里会解析到不同的东西。
    /// 这些规则与缓存都收在本对象里。
    /// </summary>
    internal sealed class ValueAccessor
    {
        /// <summary>成员/方法查找的可见性范围: 只认本层声明, 逐层向基类走由查找方自己控制。</summary>
        private const BindingFlags MemberLookupFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                                      BindingFlags.DeclaredOnly;

        /// <summary>按 (类型, 方法名, 形参个数, 是否只认静态) 缓存的方法查找结果, 含"没有"这个否定结果(null 值)。</summary>
        private readonly Dictionary<(Type, string, int, bool), MethodInfo> m_MethodCache = new();

        /// <summary>按 (类型, 名字, 是否只认静态) 缓存的字段/属性查找结果, 含"没有"这个否定结果(null 值)。</summary>
        private readonly Dictionary<(Type, string, bool), MemberInfo> m_FieldPropertyCache = new();

        //-------------------------------------- 读 --------------------------------------

        /// <summary>按成员元数据读值; 读取失败(异常)时记一条警告并返回 null。</summary>
        public object Read(CachedMember m, object target)
        {
            try
            {
                if (m.Kind == MemberKind.Field)
                {
                    return ((FieldInfo)m.Info).GetValue(m.IsStatic ? null : target);
                }

                var prop = (PropertyInfo)m.Info;
                return prop.GetValue(prop.GetMethod.IsStatic ? null : target, null);
            }
            catch (Exception e) when (!(e is ExitGUIException))
            {
                DrawLog.Warn($"[Inspector] 读取失败 {m.RawName}\n{e.Message}");
                return null;
            }
        }

        /// <summary>按成员元数据读值; 静态成员忽略 target, 只写属性(无 getter)返回 null。</summary>
        public object Read(MemberInfo member, object target)
        {
            switch (member)
            {
                case FieldInfo field:
                    return field.IsStatic ? field.GetValue(null) : field.GetValue(target);
                case PropertyInfo prop:
                    if (prop.GetMethod == null) return null;
                    return prop.GetMethod.IsStatic ? prop.GetValue(null, null) : prop.GetValue(target, null);
                default:
                    return null;
            }
        }

        /// <summary>
        /// 按名字取成员值(条件显隐里不写 @ 的裸成员名走这条)。
        /// 顺序: 无参方法 &gt; 字段/属性(取最派生的那个); 名字解析不到时按"视为可见"返回 true。
        /// 解析结果是有缓存的 —— 条件每帧都要重算, 但"这个名字对应哪个成员"是恒定的。
        /// </summary>
        public object GetMemberValue(object target, string memberName)
        {
            if (target == null || string.IsNullOrEmpty(memberName)) return null;

            var method = FindMethod(target, memberName);
            if (method != null)
            {
                try
                {
                    return method.IsStatic ? method.Invoke(null, null) : method.Invoke(target, null);
                }
                catch
                {
                    return false;
                }
            }

            var member = ResolveFieldOrProperty(target.GetType(), memberName, false);
            if (member == null)
            {
                // 名字解析不到的既有语义是"视为可见": 宁可不隐藏, 也不要因为一个拼错的名字把成员整片吞掉。
                // 但"宁可不隐藏"不该连线索都不留: 拼错的名字(条件里的成员名、@ 表达式里的裸名字)报一次,
                // 否则成员一直显示, 谁也想不到是名字写错了。
                DrawLog.WarnOnce($"member-unresolved:{target.GetType().FullName}.{memberName}",
                                      $"[Inspector] 条件成员名解析不到: {target.GetType().Name}.{memberName}, " +
                                      "已按“视为可见”处理 —— 请检查成员名是否写错。");
                return true;
            }

            try
            {
                return Read(member, target);
            }
            catch
            {
                return false;
            }
        }

        //-------------------------------------- 写 --------------------------------------

        /// <summary>按成员元数据写值(自动做类型转换)。</summary>
        public void Write(CachedMember m, object target, object value)
        {
            if (m.Kind == MemberKind.Field)
            {
                var field = (FieldInfo)m.Info;
                field.SetValue(m.IsStatic ? null : target, ConvertValue(value, field.FieldType));
            }
            else
            {
                var prop = (PropertyInfo)m.Info;
                if (prop.SetMethod == null) return;
                prop.SetValue(prop.SetMethod.IsStatic ? null : target, ConvertValue(value, prop.PropertyType), null);
            }
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsInstanceOfType(value)) return value;

            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.ToObject(targetType, value);
                }
                catch
                {
                    return value;
                }
            }

            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }

        //-------------------------------------- 调用 --------------------------------------

        /// <summary>调用方法并吞掉异常(按钮 / [OnValueChanged] 回调可能修改数据, 失败不该炸掉整帧)。</summary>
        public void InvokeSafe(object target, MethodInfo method)
        {
            try
            {
                if (method.IsStatic)
                {
                    method.Invoke(null, null);
                }
                else if (target != null)
                {
                    method.Invoke(target, null);
                }
            }
            catch (Exception e) when (!(e is ExitGUIException))
            {
                DrawLog.Error($"[Inspector] 方法执行失败 {method.Name}\n{e}");
            }
        }

        /// <summary>调用成员(成员可能是方法)。</summary>
        public void InvokeSafe(object target, MemberInfo info)
        {
            if (info is MethodInfo method) InvokeSafe(target, method);
        }

        //-------------------------------------- 查找 --------------------------------------

        /// <summary>
        /// 按名字解析字段或属性(取最派生的那个), 结果按 (类型, 名字, 是否只认静态) 缓存。
        /// 逐层向基类走而不是一次 GetMember/GetProperty: 一次查在"同名方法重载"时会混进方法节点,
        /// 在"派生类用 new 隐藏基类同名成员"时返回的是成员数组(顺序未定义);
        /// 逐层走把"只认字段/属性, 且取最派生的那个"变成确定规则。
        /// </summary>
        public MemberInfo ResolveFieldOrProperty(Type type, string name, bool staticOnly)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;

            var key = (type, name, staticOnly);
            if (m_FieldPropertyCache.TryGetValue(key, out var cached)) return cached;

            var flags = MemberLookupFlags |
                        (staticOnly ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

            MemberInfo member = null;
            for (var current = type; current != null && member == null; current = current.BaseType)
            {
                member = (MemberInfo)current.GetField(name, flags) ?? current.GetProperty(name, flags);
            }

            m_FieldPropertyCache[key] = member;
            return member;
        }

        /// <summary>
        /// 按名字与形参个数找方法, 结果按 (类型, 名字, 形参个数, 是否只认静态) 缓存。
        /// 不能用 Type.GetMethod(name, flags): 同名重载会抛 AmbiguousMatchException,
        /// 而按钮 / 回调 / 表达式调用本来就要求形参个数确定, 显式只认这一个版本反而更确定。
        /// 逐层向声明它的类型找, 顺带省掉 GetMethods 一次扫全表(它会分配整个方法数组)。
        /// </summary>
        public MethodInfo FindMethod(Type type, string name, int argCount, bool staticOnly)
        {
            if (type == null || string.IsNullOrEmpty(name)) return null;

            var key = (type, name, argCount, staticOnly);
            if (m_MethodCache.TryGetValue(key, out var cached)) return cached;

            var flags = MemberLookupFlags |
                        (staticOnly ? BindingFlags.Static : BindingFlags.Instance | BindingFlags.Static);

            MethodInfo method = null;
            for (var current = type; current != null && method == null; current = current.BaseType)
            {
                foreach (var candidate in current.GetMethods(flags))
                {
                    if (candidate.Name != name || candidate.IsSpecialName) continue;
                    if (candidate.GetParameters().Length != argCount) continue;
                    method = candidate;
                    break;
                }
            }

            m_MethodCache[key] = method;
            return method;
        }

        /// <summary>引擎侧入口: 按钮 / 回调 / CustomAddFunction 一律按无参方法调用。</summary>
        public MethodInfo FindMethod(object target, string name)
        {
            return target == null ? null : FindMethod(target.GetType(), name, 0, false);
        }
    }
}
