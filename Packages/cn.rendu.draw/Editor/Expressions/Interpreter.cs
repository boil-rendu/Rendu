using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Expressions
{
    //-------------------------------------- 解释执行 --------------------------------------

    internal static class Interpreter
    {
        /// <summary>
        /// 求值时用到的取值器 —— 走引擎那份进程级服务, 与绘制路径的查找规则/缓存完全同源。
        ///
        /// 刻意取自 <see cref="EngineServices"/> 而不是 <see cref="Engine"/>: 门面是最外层,
        /// 内层回头找门面会形成类型环(Engine → 渲染器 → 解释器 → Engine)。
        /// </summary>
        private static ValueAccessor Values => EngineServices.Shared.Values;

        /// <summary>求值时反射取成员的查找标志(公开与非公开、实例与静态)。</summary>
        internal const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static;

    /// <summary>
    /// 按类型缓存的索引器(只认单参数索引器)与形参类型。
    /// 每走一次自定义 this[...] 都要 GetProperties 扫全表 + 取一次参数数组,
    /// 这两笔分配都随重绘反复发生。
    /// </summary>
    internal static readonly Dictionary<Type, (PropertyInfo Property, Type ParamType)[]> s_IndexerCache = new();

        public static object Eval(SyntaxNode node, ExpressionContext ctx)
        {
            if (node == null) return null;
            switch (node.Kind)
            {
                case SyntaxKind.Num:
                    return node.Num;
                case SyntaxKind.Str:
                    return node.S;
                case SyntaxKind.Bool:
                    return node.B;
                case SyntaxKind.Null:
                    return null;
                case SyntaxKind.Var:
                    return ResolveRoot(node.S, ctx);
                case SyntaxKind.Unary:
                {
                    var v = Eval(node.A, ctx);
                    if (node.S == "!") return !ValueOps.Truthy(v);
                    if (node.S == "-") return ValueOps.IsNumeric(v) ? -Convert.ToDouble(v) : v;
                    return v;
                }
                case SyntaxKind.Binary:
                    return EvalBinary(node, ctx);
                case SyntaxKind.Ternary:
                {
                    var cond = Eval(node.A, ctx);
                    return ValueOps.Truthy(cond) ? Eval(node.B_, ctx) : Eval(node.C, ctx);
                }
                case SyntaxKind.Assign:
                {
                    var value = Eval(node.B_, ctx);
                    SetLeft(node.A, value, ctx);
                    return value;
                }
                case SyntaxKind.Chain:
                    return EvalChain(node, ctx);
                case SyntaxKind.Call:
                {
                    var args = EvalArgs(node.Args, ctx);
                    return InvokeUnbound(node.S, args, ctx);
                }
                default:
                    return null;
            }
        }

        internal static object EvalBinary(SyntaxNode node, ExpressionContext ctx)
        {
            var l = Eval(node.A, ctx);
            var r = Eval(node.B_, ctx);
            switch (node.S)
            {
                case "&&": return ValueOps.Truthy(l) && ValueOps.Truthy(r);
                case "||": return ValueOps.Truthy(l) || ValueOps.Truthy(r);
                case "==": return ValueOps.Equal(l, r);
                case "!=": return !ValueOps.Equal(l, r);
                case "<": return ValueOps.Compare(l, r) < 0;
                case "<=": return ValueOps.Compare(l, r) <= 0;
                case ">": return ValueOps.Compare(l, r) > 0;
                case ">=": return ValueOps.Compare(l, r) >= 0;
                case "+":
                    return l is string || r is string
                               ? (l?.ToString() ?? "") + (r?.ToString() ?? "")
                               : Numeric(l, r, (x, y) => x + y);
                case "-": return Numeric(l, r, (x, y) => x - y);
                case "*": return Numeric(l, r, (x, y) => x * y);
                case "/": return Numeric(l, r, (x, y) => x / y);
                case "%":
                    return Numeric(l, r, (x, y) => Math.Abs(y) < 1e-12 ? 0 : x % y);
                default:
                    return null;
            }
        }

        internal static object Numeric(object a, object b, Func<double, double, double> op)
        {
            return ValueOps.IsNumeric(a) && ValueOps.IsNumeric(b)
                       ? op(Convert.ToDouble(a), Convert.ToDouble(b))
                       : null;
        }

        //-------------------------------------- 根与链 --------------------------------------

        /// <summary>解析裸标识符根: 预置变量 > 目标实例成员 > 全局静态类型。</summary>
        internal static object ResolveRoot(string name, ExpressionContext ctx)
        {
            switch (name)
            {
                case "$property":
                    return new PropertyAccessor { Context = ctx };
                case "$value":
                    return ctx.Value;
                case "$root":
                case "this":
                    return ctx.Target;
            }

            if (ctx.Target != null)
            {
                var member = Values.ResolveFieldOrProperty(ctx.Target.GetType(), name, false);
                if (member != null) return Values.Read(member, ctx.Target);
            }

            var global = GlobalTypeIndex.FindGlobalType(name);
            return global;
        }

        internal static object EvalChain(SyntaxNode chain, ExpressionContext ctx)
        {
            var current = Eval(chain.A, ctx);
            if (chain.Args == null) return current;
            for (var i = 0; i < chain.Args.Count; i++)
            {
                current = Step(chain.Args[i], current, ctx);
            }

            return current;
        }

        /// <summary>执行链上一步。Var 步骤=字段/属性/无参方法；Call 步骤=带参方法。</summary>
        internal static object Step(SyntaxNode step, object current, ExpressionContext ctx)
        {
            if (current == null) return null;

            var asType = current as Type;
            var staticAccess = asType != null;
            var runType = staticAccess ? asType : current.GetType();
            var target = staticAccess ? null : current;

            if (step.Kind == SyntaxKind.Call)
            {
                var args = EvalArgs(step.Args, ctx);
                var mi = Values.FindMethod(runType, step.S, args.Length, staticAccess);
                if (mi == null) return null;
                return InvokeRaw(mi, mi.IsStatic ? null : target, args);
            }

            if (step.Kind == SyntaxKind.Index)
            {
                return IndexInto(current, Eval(step.A, ctx));
            }

            var name = step.S;
            var member = Values.ResolveFieldOrProperty(runType, name, staticAccess);
            if (member is FieldInfo || member is PropertyInfo readable && readable.GetMethod != null)
            {
                return Values.Read(member, target);
            }

            var noArg = Values.FindMethod(runType, name, 0, staticAccess);
            if (noArg != null) return InvokeRaw(noArg, noArg.IsStatic ? null : target, null);

            return null;
        }

        /// <summary>
        /// 索引访问: 数组 / 字符串 / List / Dictionary, 以及自定义的 this[...] 索引器。
        /// 越界与键缺失一律返回 null —— 表达式求值不该因为越界就把整段绘制打断。
        /// </summary>
        internal static object IndexInto(object collection, object key)
        {
            if (collection == null || key == null) return null;

            switch (collection)
            {
                case Array array:
                {
                    var i = ToIndex(key);
                    return i >= 0 && i < array.Length ? array.GetValue(i) : null;
                }
                case string text:
                {
                    var i = ToIndex(key);
                    if (i < 0 || i >= text.Length) return null;
                    return text[i];
                }
                case IList list:
                {
                    var i = ToIndex(key);
                    return i >= 0 && i < list.Count ? list[i] : null;
                }
                case IDictionary dict:
                    return dict.Contains(key) ? dict[key] : null;
            }

            var indexers = GetIndexers(collection.GetType());
            for (var i = 0; i < indexers.Length; i++)
            {
                var argument = CoerceArg(key, indexers[i].ParamType);
                if (argument == null) continue;
                return indexers[i].Property.GetValue(collection, new[] { argument });
            }

            return null;
        }

        internal static int ToIndex(object key)
        {
            if (key is double d) return (int)d;
            try
            {
                return Convert.ToInt32(key, CultureInfo.InvariantCulture);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>把索引实参转成形参类型; 转不了返回 null, 由调用方跳过该索引器。</summary>
        internal static object CoerceArg(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsInstanceOfType(value)) return value;

            if (value is double number)
            {
                var narrowed = targetType.IsEnum
                                   ? Enum.ToObject(targetType, (long)number)
                                   : NarrowNumber(number, targetType);
                return targetType.IsInstanceOfType(narrowed) ? narrowed : null;
            }

            try
            {
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        internal static void SetLeft(SyntaxNode node, object value, ExpressionContext ctx)
        {
            try
            {
                if (node.Kind == SyntaxKind.Chain)
                {
                    var chain = node;
                    var current = Eval(chain.A, ctx);
                    if (current == null) return;
                    if (chain.Args == null || chain.Args.Count == 0) return;

                    for (var i = 0; i < chain.Args.Count - 1; i++)
                    {
                        current = Step(chain.Args[i], current, ctx);
                        if (current == null) return;
                    }

                    var last = chain.Args[chain.Args.Count - 1];
                    if (last.Kind != SyntaxKind.Var) return;
                    SetMember(current, last.S, value);
                    return;
                }

                if (node.Kind == SyntaxKind.Var)
                {
                    var name = node.S;
                    if (name == "$value" && ctx.Value is PropertyAccessor) return;
                    if (name == "$value")
                    {
                        ctx.Value = value;
                    }
                }
            }
            catch (Exception e)
            {
                DrawLog.Warn($"[Inspector] 表达式赋值失败: {e.Message}");
            }
        }

        internal static void SetMember(object obj, string name, object value)
        {
            if (obj == null) return;

            var member = Values.ResolveFieldOrProperty(obj.GetType(), name, false);
            switch (member)
            {
                case FieldInfo fi:
                    fi.SetValue(obj, ConvertAssign(value, fi.FieldType));
                    break;
                case PropertyInfo pi when pi.SetMethod != null:
                    pi.SetValue(obj, ConvertAssign(value, pi.PropertyType), null);
                    break;
            }
        }

        /// <summary>取该类型的单参数索引器; 结果按类型缓存, 避免每次索引访问都扫一遍属性表。</summary>
        internal static (PropertyInfo Property, Type ParamType)[] GetIndexers(Type type)
        {
            if (s_IndexerCache.TryGetValue(type, out var cached)) return cached;

            var list = new List<(PropertyInfo, Type)>();
            foreach (var property in type.GetProperties(AllFlags))
            {
                var parameters = property.GetIndexParameters();
                if (parameters.Length != 1) continue;
                list.Add((property, parameters[0].ParameterType));
            }

            var result = list.ToArray();
            s_IndexerCache[type] = result;
            return result;
        }

        internal static object ConvertAssign(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsInstanceOfType(value)) return value;
            try
            {
                return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>未绑定根的方法调用: 目标实例方法 > 目标静态方法。</summary>
        internal static object InvokeUnbound(string name, object[] args, ExpressionContext ctx)
        {
            if (ctx.Target != null)
            {
                var mi = Values.FindMethod(ctx.Target.GetType(), name, args.Length, false);
                if (mi != null) return InvokeRaw(mi, mi.IsStatic ? null : ctx.Target, args);
            }

            return null;
        }

        internal static object InvokeRaw(MethodInfo mi, object target, object[] args)
        {
            try
            {
                return mi.Invoke(mi.IsStatic ? null : target, CoerceArgs(mi, args));
            }
            catch (Exception e)
            {
                DrawLog.Warn($"[Inspector] 方法调用失败 {mi.DeclaringType?.Name}.{mi.Name}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 按形参类型收窄实参。
        /// 表达式里的数字字面量一律是 double, 而反射默认 binder 只做拓宽转换,
        /// 于是 @Add(1, 2) 打到 Add(int, int) 会因 double→int 失败, 且结果跨运行时不一致
        /// (Mono/IL2CPP 与 CoreCLR 对 binder 的宽容度不同)。这里显式收窄, 让行为确定下来。
        /// 只处理能确定的情况, 其余原样交给反射。
        /// </summary>
        internal static object[] CoerceArgs(MethodInfo mi, object[] args)
        {
            if (args == null || args.Length == 0) return args;

            var parameters = mi.GetParameters();
            if (parameters.Length != args.Length) return args;

            for (var i = 0; i < args.Length; i++)
            {
                var value = args[i];
                if (value == null) continue;

                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsInstanceOfType(value)) continue;

                if (value is double number)
                {
                    args[i] = parameterType.IsEnum
                        ? Enum.ToObject(parameterType, (long)number)
                        : NarrowNumber(number, parameterType);
                }
            }

            return args;
        }

        /// <summary>把 double 字面量收窄到数值形参; 不认识的形参类型原样返回, 交由反射处理。</summary>
        internal static object NarrowNumber(double value, Type targetType)
        {
            if (targetType == typeof(float)) return (float)value;
            if (targetType == typeof(decimal)) return (decimal)value;
            if (targetType == typeof(int)) return (int)value;
            if (targetType == typeof(long)) return (long)value;
            if (targetType == typeof(short)) return (short)value;
            if (targetType == typeof(sbyte)) return (sbyte)value;
            if (targetType == typeof(byte)) return (byte)value;
            if (targetType == typeof(uint)) return (uint)value;
            if (targetType == typeof(ulong)) return (ulong)value;
            if (targetType == typeof(ushort)) return (ushort)value;
            return value;
        }

        internal static object[] EvalArgs(List<SyntaxNode> args, ExpressionContext ctx)
        {
            if (args == null || args.Count == 0) return new object[0];
            var result = new object[args.Count];
            for (var i = 0; i < args.Count; i++)
            {
                result[i] = Eval(args[i], ctx);
            }

            return result;
        }
    }
}
