using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Serialization;
using UnityEngine;

// [Serialize] 属**下层序列化器**(cn.rendu.serialization, 命名空间 RD.Serialization), 不属本包:
// 它是"数据要落盘"的声明, 检视器只是顺带认它 —— 这个 using 不能省, 否则私有成员不会被收集。

namespace RD.Draw.Editor.Metadata
{
    /// <summary>
    /// 成员收集: 按类型反射出可绘制的成员, 并缓存结果。
    ///
    /// 反射只跟类型有关, 所以缓存可以跨帧跨对象复用 —— 这份缓存由本对象持有,
    /// 不再是散落在引擎各处的一堆静态字典。
    /// </summary>
    internal sealed class MemberCollector
    {
        private readonly Dictionary<Type, CachedMember[]> m_Cache = new();

        /// <summary>取某个类型的全部可绘制成员(按 Order 排序, 同序按声明顺序)。</summary>
        public CachedMember[] Get(Type type)
        {
            if (type == null) return Array.Empty<CachedMember>();
            if (m_Cache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var list = new List<CachedMember>();
            var seq = 0;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.Public | BindingFlags.NonPublic |
                                       BindingFlags.FlattenHierarchy;

            foreach (var field in type.GetFields(flags))
            {
                if (field.IsDefined(typeof(HideInInspector), true)) continue;
                if (field.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true)) continue;
                if (IsEngineInternal(field)) continue;

                // 可见性规则: 公开字段、Unity 序列化字段([SerializeField])、
                // 序列化字段([Serialize]) 与显式 [ShowInInspector] 都显示, 其余私有成员隐藏
                var show = field.IsDefined(typeof(ShowInInspectorAttribute), true) ||
                           field.IsDefined(typeof(SerializeField), true) ||
                           field.IsDefined(typeof(SerializeAttribute), true) ||
                           (field.IsPublic && field.IsStatic == false);
                if (show == false) continue;

                list.Add(new CachedMember
                {
                    Info = field,
                    Kind = MemberKind.Field,
                    Seq = seq++,
                    IsStatic = field.IsStatic,
                    ValueType = field.FieldType,
                    Order = GetOrder(field),
                    Attrs = field.GetCustomAttributes(false),
                });
            }

            foreach (var prop in type.GetProperties(flags))
            {
                if (prop.IsDefined(typeof(HideInInspector), true)) continue;
                if (prop.GetMethod == null) continue;
                if (IsEngineInternal(prop)) continue;
                // 属性需 [ShowInInspector] 或 [Serialize] 才显示
                if (prop.IsDefined(typeof(ShowInInspectorAttribute), true) == false &&
                    prop.IsDefined(typeof(SerializeAttribute), true) == false)
                {
                    continue;
                }

                list.Add(new CachedMember
                {
                    Info = prop,
                    Kind = MemberKind.Property,
                    Seq = seq++,
                    IsStatic = prop.GetMethod.IsStatic,
                    ValueType = prop.PropertyType,
                    Order = GetOrder(prop),
                    Attrs = prop.GetCustomAttributes(false),
                });
            }

            foreach (var method in type.GetMethods(flags))
            {
                if (method.IsSpecialName) continue;
                if (method.GetParameters().Length > 0) continue;
                if (IsEngineInternal(method)) continue;

                var isButton = method.IsDefined(typeof(ButtonAttribute), true) ||
                               method.IsDefined(typeof(ButtonGroupAttribute), true);
                var isOnInspectorGUI = method.IsDefined(typeof(OnInspectorGUIAttribute), true);
                if (isButton == false && isOnInspectorGUI == false) continue;

                list.Add(new CachedMember
                {
                    Info = method,
                    Kind = isOnInspectorGUI ? MemberKind.MethodOnInspectorGUI : MemberKind.MethodButton,
                    Seq = seq++,
                    IsStatic = method.IsStatic,
                    Order = GetOrder(method),
                    Attrs = method.GetCustomAttributes(false),
                });
            }

            list.Sort((a, b) =>
            {
                if (a.Order != b.Order) return a.Order.CompareTo(b.Order);
                return a.Seq.CompareTo(b.Seq);
            });

            cached = list.ToArray();
            m_Cache[type] = cached;
            return cached;
        }

        private static double GetOrder(MemberInfo info)
        {
            // [PropertyOrder] 优先于 [Button] 的 Order: 业务代码的书写惯例是 [Button(...)] 在前、
            // [PropertyOrder(...)] 在后, 按声明顺序取首个匹配会把 PropertyOrder 吃掉。
            var buttonOrder = 0d;
            foreach (var attr in info.GetCustomAttributes(false))
            {
                if (attr is PropertyOrderAttribute order)
                {
                    return order.Order;
                }

                if (attr is ButtonAttribute button && button.Order != 0)
                {
                    buttonOrder = button.Order;
                }
            }

            return buttonOrder;
        }

        /// <summary>
        /// 是否为引擎/框架内部的基类成员(EditorWindow/ScriptableObject 等)。
        /// 这类字段(如 wantsMouseMove)不应被当作业务字段绘制进窗口。
        /// </summary>
        private static bool IsEngineInternal(MemberInfo info)
        {
            if (info == null || info.DeclaringType == null) return false;
            var asm = info.DeclaringType.Assembly.GetName().Name;
            if (string.IsNullOrEmpty(asm)) return false;
            if (asm.StartsWith("UnityEngine", StringComparison.Ordinal)) return true;
            if (asm.StartsWith("UnityEditor", StringComparison.Ordinal)) return true;
            if (asm.StartsWith("mscorlib", StringComparison.Ordinal)) return true;
            return false;
        }
    }
}
