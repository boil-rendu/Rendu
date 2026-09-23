using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace RD.Inspector.Editor
{
    /// <summary>
    /// 表达式求值与条件显隐共用的"值语义"（真值 / 数值判定 / 相等）。
    ///
    /// 这两条路径必须用同一套规则，否则同一个值在两处会得到相反结论。此前两边各写了一份，
    /// 且数值相等一处用 <c>double.Epsilon</c>（实际等同于要求完全相等）、一处用 1e-9：
    /// <c>[ShowIf("Speed", 0.1)]</c> 在 Speed 是 float 时会因 float→double 的表示误差被判成
    /// "不相等"，字段诡异地消失。统一到这里，容差只有一个定义。
    /// </summary>
    internal static class InspectorValue
    {
        /// <summary>
        /// 数值比较容差（相对）。
        /// 用相对容差而不是 == 或 <c>double.Epsilon</c>：字段常是 float，而特性参数与表达式里的
        /// 字面量是 double，<c>0.1f</c> 转 double 是 0.100000001490116…，与 0.1 差约 1.5e-9。
        /// 容差随量级放大也是必需的：1e9 附近的 float 相邻值间隔远大于任何固定绝对容差。
        /// </summary>
        internal const double NumericTolerance = 1e-6;

        /// <summary>布尔上下文：null / false / 空串 / 空集合 / 已销毁的 Unity 对象为假，其余为真（数值 0 也为真）。</summary>
        internal static bool Truthy(object value)
        {
            switch (value)
            {
                case null:
                    return false;
                case bool b:
                    return b;
                case string s:
                    return string.IsNullOrEmpty(s) == false;
                case UnityEngine.Object u:
                    return u != null;
                case ICollection c:
                    return c.Count > 0;
                default:
                    return true;
            }
        }

        /// <summary>是否为数值类型（枚举与 bool 不算）。</summary>
        internal static bool IsNumeric(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong ||
                   value is float || value is double || value is decimal;
        }

        /// <summary>相等判定：数值带容差比较，枚举可与字符串名同义比较。</summary>
        internal static bool Equal(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (Equals(a, b)) return true;

            if (IsNumeric(a) && IsNumeric(b))
            {
                var left = Convert.ToDouble(a);
                var right = Convert.ToDouble(b);
                var scale = Math.Max(1d, Math.Max(Math.Abs(left), Math.Abs(right)));
                return Math.Abs(left - right) <= NumericTolerance * scale;
            }

            if (a is Enum && b is string) return a.ToString() == b.ToString();
            if (b is Enum && a is string) return b.ToString() == a.ToString();
            return false;
        }
    }

    /// <summary>
    /// Inspector 属性状态（对应表达式 $property.State 语义）。
    /// 由引擎在绘制时构造，Expanded 等读写会联动实际折叠状态。
    /// </summary>
    public sealed class InspectorPropertyState
    {
        public bool Expanded = true;
        public bool Selected;
        public bool Visible = true;
        public bool Enabled = true;
    }

    /// <summary>
    /// 表达式求值上下文。
    /// Target=当前绘制对象(this)；Value=当前成员当前值($value)；State=当前成员状态($property.State)。
    /// </summary>
    public sealed class InspectorExpressionContext
    {
        public object Target;
        public object Value;
        public InspectorPropertyState State = new InspectorPropertyState();
    }

    /// <summary>
    /// Inspector 表达式求值器。
    /// 支持: @ 前缀、$property/$value/$root/this、成员访问链、方法调用(实例/静态/全局静态类)、
    /// 括号、比较/逻辑/算术、一元、三元、赋值(如 $property.State.Expanded = true)。
    /// </summary>
    public static class InspectorExpressionEvaluator
    {
        private const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance | BindingFlags.Static;

        private static Dictionary<string, List<Type>> s_NameToTypes;
        private static readonly object s_IndexLock = new object();

        /// <summary>
        /// 表达式源码(去掉 @ 后) → AST。AST 解析完就不再变化, 而同一个表达式在每次重绘时都会被重算,
        /// 于是词法 + 语法分析被白白重复几千遍并产生成吨临时对象。缓存后稳态下只剩求值。
        /// 只在编辑器主线程访问(与绘制同线程), 因此不加锁。
        /// </summary>
        private static readonly Dictionary<string, Node> s_AstCache = new();

        /// <summary>
        /// 解析失败标记(负缓存)。
        /// 写错的表达式每帧都会重新判定: 没有负缓存就会每帧重新词法分析一遍、每帧重报一条日志。
        /// 缓存下来后"报一次、之后当作空表达式"。
        /// </summary>
        private static readonly Node s_ParseFailed = new Node(NK.Null);

        /// <summary>
        /// AST 缓存上限。表达式来自特性参数, 数量天然有限; 但 Evaluate 是公开 API, 若被塞进
        /// 动态拼出来的字符串, 缓存会无界增长 —— 到上限整体清空即可(重新解析很便宜)。
        /// </summary>
        private const int AstCacheLimit = 512;

        /// <summary>
        /// 按类型缓存的索引器(只认单参数索引器)与形参类型。
        /// 每走一次自定义 this[...] 就 GetProperties 扫全表 + 取一次参数数组, 这两笔分配都随重绘反复发生。
        /// </summary>
        private static readonly Dictionary<Type, (PropertyInfo Property, Type ParamType)[]> s_IndexerCache = new();

        //-------------------------------------- 对外 API --------------------------------------

        public static bool IsExpression(string s)
        {
            return !string.IsNullOrEmpty(s) && s[0] == '@';
        }

        public static object Evaluate(string expression, object target, object value, InspectorPropertyState state)
        {
            if (string.IsNullOrEmpty(expression)) return null;
            var code = expression[0] == '@' ? expression.Substring(1) : expression;

            var node = GetAst(code);
            if (node == null || ReferenceEquals(node, s_ParseFailed)) return null;

            try
            {
                var ctx = new InspectorExpressionContext
                {
                    Target = target,
                    Value = value,
                    State = state ?? new InspectorPropertyState(),
                };
                return Interpreter.Eval(node, ctx);
            }
            catch (Exception e)
            {
                LogWarning($"[Inspector] 表达式求值失败 '{expression}': {e.Message}");
                return null;
            }
        }

        private static Node GetAst(string code)
        {
            if (s_AstCache.TryGetValue(code, out var cached)) return cached;

            if (s_AstCache.Count >= AstCacheLimit) s_AstCache.Clear();

            Node node;
            try
            {
                var tokens = Tokenize(code);
                node = tokens == null || tokens.Count == 0 ? null : new Parser(tokens).ParseRoot();
            }
            catch (Exception e)
            {
                LogWarning($"[Inspector] 表达式解析失败 '{code}': {e.Message}");
                node = null;
            }

            if (node == null)
            {
                // 解析不出来就不再重试: 只报一次, 之后按空表达式处理(返回 null, 条件判定为假)。
                // 这条警告很重要 —— 没有它, 一个拼错的表达式会永远静默地把成员藏起来。
                LogWarning($"[Inspector] 表达式语法错误, 已忽略: {code}");
                s_AstCache[code] = s_ParseFailed;
                return s_ParseFailed;
            }

            s_AstCache[code] = node;
            return node;
        }

        /// <summary>
        /// 记一条警告, 且不让"记日志"本身把求值打断(见 <see cref="InspectorLog"/>:
        /// Unity 的 Debug.* 在非 Unity 进程里会抛 SecurityException, 包住它这条路径才能被校验工程覆盖)。
        /// </summary>
        private static void LogWarning(string message)
        {
            InspectorLog.Warn(message);
        }

        public static bool EvaluateBool(string expression, object target, object value, InspectorPropertyState state)
        {
            return Truthy(Evaluate(expression, target, value, state));
        }

        public static bool Truthy(object o)
        {
            return InspectorValue.Truthy(o);
        }

        private static int CompareValues(object a, object b)
        {
            if (InspectorValue.IsNumeric(a) && InspectorValue.IsNumeric(b)) return Convert.ToDouble(a).CompareTo(Convert.ToDouble(b));
            if (a is string sa && b is string sb) return string.Compare(sa, sb, StringComparison.Ordinal);
            if (a is bool ba && b is bool bb) return ba.CompareTo(bb);
            return string.Compare(a?.ToString() ?? "", b?.ToString() ?? "", StringComparison.Ordinal);
        }

        //-------------------------------------- 全局短名类型索引 --------------------------------------

        private static void EnsureTypeIndex()
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

        private static Type FindGlobalType(string shortName)
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

        //-------------------------------------- Tokenizer --------------------------------------

        private enum Tok
        {
            Ident,
            Number,
            String,
            Op,
            End,
        }

        private struct Token
        {
            public Tok Type;
            public string Text;
            public double Num;
        }

        private static readonly string[] MultiOps = { "==", "!=", "<=", ">=", "&&", "||" };

        private static List<Token> Tokenize(string code)
        {
            var tokens = new List<Token>();
            var i = 0;
            var n = code.Length;

            while (i < n)
            {
                var c = code[i];
                if (char.IsWhiteSpace(c) || c == '@')
                {
                    i++;
                    continue;
                }

                if (c == '"' || c == '\'')
                {
                    var quote = c;
                    i++;
                    var sb = new System.Text.StringBuilder();
                    var closed = false;
                    while (i < n)
                    {
                        var ch = code[i];
                        if (ch == '\\' && i + 1 < n)
                        {
                            i++;
                            var esc = code[i];
                            switch (esc)
                            {
                                case 'n': sb.Append('\n'); break;
                                case 't': sb.Append('\t'); break;
                                case 'r': sb.Append('\r'); break;
                                default: sb.Append(esc); break;
                            }

                            i++;
                            continue;
                        }

                        if (ch == quote)
                        {
                            closed = true;
                            i++;
                            break;
                        }

                        sb.Append(ch);
                        i++;
                    }

                    if (closed == false) return null;
                    tokens.Add(new Token { Type = Tok.String, Text = sb.ToString() });
                    continue;
                }

                if (char.IsDigit(c) ||
                    c == '.' && i + 1 < n && char.IsDigit(code[i + 1]))
                {
                    var dotUsed = false;
                    var start = i;
                    while (i < n)
                    {
                        var ch = code[i];
                        if (char.IsDigit(ch))
                        {
                            i++;
                        }
                        else if (ch == '.' && dotUsed == false && i + 1 < n && char.IsDigit(code[i + 1]))
                        {
                            dotUsed = true;
                            i++;
                        }
                        else if ((ch == 'e' || ch == 'E') && i + 1 < n &&
                                 (char.IsDigit(code[i + 1]) || (code[i + 1] == '-' || code[i + 1] == '+') &&
                                  i + 2 < n && char.IsDigit(code[i + 2])))
                        {
                            i += 2;
                            while (i < n && char.IsDigit(code[i])) i++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    var text = code.Substring(start, i - start);
                    if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                    {
                        tokens.Add(new Token { Type = Tok.Number, Text = text, Num = num });
                    }
                    else
                    {
                        return null;
                    }

                    continue;
                }

                if (char.IsLetter(c) || c == '_' || c == '$')
                {
                    var start = i;
                    while (i < n)
                    {
                        var ch = code[i];
                        if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '$')
                        {
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    tokens.Add(new Token { Type = Tok.Ident, Text = code.Substring(start, i - start) });
                    continue;
                }

                var matched = false;
                foreach (var op in MultiOps)
                {
                    if (i + op.Length <= n && code.Substring(i, op.Length) == op)
                    {
                        tokens.Add(new Token { Type = Tok.Op, Text = op });
                        i += op.Length;
                        matched = true;
                        break;
                    }
                }

                if (matched) continue;

                if ("=<>!?:.()+-*/%,&|[]".IndexOf(c) >= 0)
                {
                    tokens.Add(new Token { Type = Tok.Op, Text = c.ToString() });
                    i++;
                    continue;
                }

                return null;
            }

            tokens.Add(new Token { Type = Tok.End, Text = "" });
            return tokens;
        }

        //-------------------------------------- AST --------------------------------------

        private enum NK
        {
            Num,
            Str,
            Bool,
            Null,
            Var,
            Chain,
            Call,
            Index,
            Unary,
            Binary,
            Ternary,
            Assign,
        }

        private sealed class Node
        {
            public NK K;
            public double Num;
            public string S;           // Var 名称 / 运算符 / Call 方法名
            public bool B;
            public Node A;             // 左 / 条件 / Chain 根
            public Node B_;            // 右 / then / Assign 值
            public Node C;             // else
            public List<Node> Args;    // Call / 链步骤参数

            public Node(NK k)
            {
                K = k;
            }
        }

        private sealed class Parser
        {
            private readonly List<Token> m_Tokens;
            private int m_Pos;
            private bool m_Error;

            public Parser(List<Token> tokens)
            {
                m_Tokens = tokens;
            }

            private Token Peek()
            {
                return m_Tokens[Math.Min(m_Pos, m_Tokens.Count - 1)];
            }

            private Token Next()
            {
                var t = Peek();
                if (m_Pos < m_Tokens.Count - 1) m_Pos++;
                return t;
            }

            private bool Accept(string op)
            {
                var t = Peek();
                if (t.Type == Tok.Op && t.Text == op)
                {
                    Next();
                    return true;
                }

                return false;
            }

            private bool IsOp(string op)
            {
                var t = Peek();
                return t.Type == Tok.Op && t.Text == op;
            }

            public Node ParseRoot()
            {
                var left = ParseAssign();

                // 这个解析器是"遇到不认识的东西就停下"的风格。不校验的话, @A[0]、@A. 这类
                // 会被静默截断成一个错值(比如 @Items[0 会变成整个 @Items), 调用方却看不到异常。
                // 两种截断都要拦: 还剩着没消费的 token, 以及解析中途主动放弃(置了 m_Error)。
                if (left == null || m_Error || Peek().Type != Tok.End) return null;

                return left;
            }

            private Node ParseAssign()
            {
                var left = ParseTernary();
                if (Accept("="))
                {
                    var right = ParseAssign();
                    if (left == null) return right;
                    return new Node(NK.Assign) { A = left, B_ = right };
                }

                return left;
            }

            private Node ParseTernary()
            {
                var cond = ParseOr();
                if (Accept("?"))
                {
                    var whenTrue = ParseTernary();
                    if (Accept(":") == false) return cond;
                    var whenFalse = ParseTernary();
                    return new Node(NK.Ternary) { A = cond, B_ = whenTrue, C = whenFalse };
                }

                return cond;
            }

            private Node ParseOr()
            {
                var left = ParseAnd();
                while (Accept("||"))
                {
                    left = new Node(NK.Binary) { S = "||", A = left, B_ = ParseAnd() };
                }

                return left;
            }

            private Node ParseAnd()
            {
                var left = ParseEquality();
                while (Accept("&&"))
                {
                    left = new Node(NK.Binary) { S = "&&", A = left, B_ = ParseEquality() };
                }

                return left;
            }

            private Node ParseEquality()
            {
                var left = ParseRelational();
                while (Accept("==") || Accept("!="))
                {
                    var op = m_Tokens[m_Pos - 1].Text;
                    left = new Node(NK.Binary) { S = op, A = left, B_ = ParseRelational() };
                }

                return left;
            }

            private Node ParseRelational()
            {
                var left = ParseAdditive();
                while (Accept("<") || Accept("<=") || Accept(">") || Accept(">="))
                {
                    var op = m_Tokens[m_Pos - 1].Text;
                    left = new Node(NK.Binary) { S = op, A = left, B_ = ParseAdditive() };
                }

                return left;
            }

            private Node ParseAdditive()
            {
                var left = ParseMultiplicative();
                while (Accept("+") || Accept("-"))
                {
                    var op = m_Tokens[m_Pos - 1].Text;
                    left = new Node(NK.Binary) { S = op, A = left, B_ = ParseMultiplicative() };
                }

                return left;
            }

            private Node ParseMultiplicative()
            {
                var left = ParseUnary();
                while (Accept("*") || Accept("/") || Accept("%"))
                {
                    var op = m_Tokens[m_Pos - 1].Text;
                    left = new Node(NK.Binary) { S = op, A = left, B_ = ParseUnary() };
                }

                return left;
            }

            private Node ParseUnary()
            {
                if (Accept("!")) return new Node(NK.Unary) { S = "!", A = ParseUnary() };
                if (Accept("-")) return new Node(NK.Unary) { S = "-", A = ParseUnary() };
                return ParsePostfix();
            }

            private Node ParsePostfix()
            {
                var primary = ParsePrimary();
                if (primary == null) return null;

                // 方法/变量根带括号 -> 调用。结果仍可继续接成员访问或索引(如 @GetList()[0])
                if (primary.K == NK.Var && IsOp("("))
                {
                    Next();
                    primary = new Node(NK.Call) { S = primary.S, Args = ParseArgs() };
                }

                // 成员访问 / 索引链
                if (IsOp(".") == false && IsOp("[") == false) return primary;

                var chain = new Node(NK.Chain) { A = primary, Args = new List<Node>() };
                while (true)
                {
                    if (Accept("."))
                    {
                        var id = Next();
                        if (id.Type != Tok.Ident)
                        {
                            m_Error = true;
                            return chain;
                        }

                        if (Accept("("))
                        {
                            chain.Args.Add(new Node(NK.Call) { S = id.Text, Args = ParseArgs() });
                        }
                        else
                        {
                            chain.Args.Add(new Node(NK.Var) { S = id.Text });
                        }

                        continue;
                    }

                    if (Accept("["))
                    {
                        var index = ParseTernary();
                        if (Accept("]") == false)
                        {
                            m_Error = true;
                            return chain;
                        }

                        chain.Args.Add(new Node(NK.Index) { A = index });
                        continue;
                    }

                    return chain;
                }
            }

            private List<Node> ParseArgs()
            {
                var args = new List<Node>();
                if (Accept(")")) return args;
                while (true)
                {
                    var node = ParseTernary();
                    if (node != null) args.Add(node);
                    if (Accept(",")) continue;
                    Accept(")");
                    break;
                }

                return args;
            }

            private Node ParsePrimary()
            {
                var t = Peek();
                switch (t.Type)
                {
                    case Tok.Number:
                        Next();
                        return new Node(NK.Num) { Num = t.Num };
                    case Tok.String:
                        Next();
                        return new Node(NK.Str) { S = t.Text };
                    case Tok.Ident:
                        Next();
                        if (t.Text == "true") return new Node(NK.Bool) { B = true };
                        if (t.Text == "false") return new Node(NK.Bool) { B = false };
                        if (t.Text == "null") return new Node(NK.Null);
                        return new Node(NK.Var) { S = t.Text };
                    case Tok.Op:
                        if (t.Text == "(")
                        {
                            Next();
                            var inner = ParseTernary();
                            if (Accept(")") == false) return null;
                            return inner;
                        }

                        return null;
                    default:
                        return null;
                }
            }
        }

        //-------------------------------------- 解释执行 --------------------------------------

        private static class Interpreter
        {
            public static object Eval(Node node, InspectorExpressionContext ctx)
            {
                if (node == null) return null;
                switch (node.K)
                {
                    case NK.Num:
                        return node.Num;
                    case NK.Str:
                        return node.S;
                    case NK.Bool:
                        return node.B;
                    case NK.Null:
                        return null;
                    case NK.Var:
                        return ResolveRoot(node.S, ctx);
                    case NK.Unary:
                    {
                        var v = Eval(node.A, ctx);
                        if (node.S == "!") return !Truthy(v);
                        if (node.S == "-") return InspectorValue.IsNumeric(v) ? -Convert.ToDouble(v) : v;
                        return v;
                    }
                    case NK.Binary:
                        return EvalBinary(node, ctx);
                    case NK.Ternary:
                    {
                        var cond = Eval(node.A, ctx);
                        return Truthy(cond) ? Eval(node.B_, ctx) : Eval(node.C, ctx);
                    }
                    case NK.Assign:
                    {
                        var value = Eval(node.B_, ctx);
                        SetLeft(node.A, value, ctx);
                        return value;
                    }
                    case NK.Chain:
                        return EvalChain(node, ctx);
                    case NK.Call:
                    {
                        var args = EvalArgs(node.Args, ctx);
                        return InvokeUnbound(node.S, args, ctx);
                    }
                    default:
                        return null;
                }
            }

            private static object EvalBinary(Node node, InspectorExpressionContext ctx)
            {
                var l = Eval(node.A, ctx);
                var r = Eval(node.B_, ctx);
                switch (node.S)
                {
                    case "&&": return Truthy(l) && Truthy(r);
                    case "||": return Truthy(l) || Truthy(r);
                    case "==": return InspectorValue.Equal(l, r);
                    case "!=": return !InspectorValue.Equal(l, r);
                    case "<": return CompareValues(l, r) < 0;
                    case "<=": return CompareValues(l, r) <= 0;
                    case ">": return CompareValues(l, r) > 0;
                    case ">=": return CompareValues(l, r) >= 0;
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

            private static object Numeric(object a, object b, Func<double, double, double> op)
            {
                return InspectorValue.IsNumeric(a) && InspectorValue.IsNumeric(b)
                           ? op(Convert.ToDouble(a), Convert.ToDouble(b))
                           : null;
            }

            //-------------------------------------- 根与链 --------------------------------------

            /// <summary>解析裸标识符根: 预置变量 > 目标实例成员 > 全局静态类型。</summary>
            private static object ResolveRoot(string name, InspectorExpressionContext ctx)
            {
                switch (name)
                {
                    case "$property":
                        return new InspectorPropertyAccessor { Context = ctx };
                    case "$value":
                        return ctx.Value;
                    case "$root":
                    case "this":
                        return ctx.Target;
                }

                if (ctx.Target != null)
                {
                    var member = InspectorEngine.ResolveFieldOrProperty(ctx.Target.GetType(), name, false);
                    if (member != null) return InspectorEngine.ReadMemberValue(member, ctx.Target);
                }

                var global = FindGlobalType(name);
                return global;
            }

            private static object EvalChain(Node chain, InspectorExpressionContext ctx)
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
            private static object Step(Node step, object current, InspectorExpressionContext ctx)
            {
                if (current == null) return null;

                var asType = current as Type;
                var staticAccess = asType != null;
                var runType = staticAccess ? asType : current.GetType();
                var target = staticAccess ? null : current;

                if (step.K == NK.Call)
                {
                    var args = EvalArgs(step.Args, ctx);
                    var mi = InspectorEngine.FindMethod(runType, step.S, args.Length, staticAccess);
                    if (mi == null) return null;
                    return InvokeRaw(mi, mi.IsStatic ? null : target, args);
                }

                if (step.K == NK.Index)
                {
                    return IndexInto(current, Eval(step.A, ctx));
                }

                var name = step.S;
                var member = InspectorEngine.ResolveFieldOrProperty(runType, name, staticAccess);
                if (member is FieldInfo || member is PropertyInfo readable && readable.GetMethod != null)
                {
                    return InspectorEngine.ReadMemberValue(member, target);
                }

                var noArg = InspectorEngine.FindMethod(runType, name, 0, staticAccess);
                if (noArg != null) return InvokeRaw(noArg, noArg.IsStatic ? null : target, null);

                return null;
            }

            /// <summary>
            /// 索引访问: 数组 / 字符串 / List / Dictionary, 以及自定义的 this[...] 索引器。
            /// 越界与键缺失一律返回 null —— 表达式求值不该因为越界就把整段绘制打断。
            /// </summary>
            private static object IndexInto(object collection, object key)
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

            private static int ToIndex(object key)
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
            private static object CoerceArg(object value, Type targetType)
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

            private static void SetLeft(Node node, object value, InspectorExpressionContext ctx)
            {
                try
                {
                    if (node.K == NK.Chain)
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
                        if (last.K != NK.Var) return;
                        SetMember(current, last.S, value);
                        return;
                    }

                    if (node.K == NK.Var)
                    {
                        var name = node.S;
                        if (name == "$value" && ctx.Value is InspectorPropertyAccessor) return;
                        if (name == "$value")
                        {
                            ctx.Value = value;
                        }
                    }
                }
                catch (Exception e)
                {
                    LogWarning($"[Inspector] 表达式赋值失败: {e.Message}");
                }
            }

            private static void SetMember(object obj, string name, object value)
            {
                if (obj == null) return;

                var member = InspectorEngine.ResolveFieldOrProperty(obj.GetType(), name, false);
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
            private static (PropertyInfo Property, Type ParamType)[] GetIndexers(Type type)
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

            private static object ConvertAssign(object value, Type targetType)
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
            private static object InvokeUnbound(string name, object[] args, InspectorExpressionContext ctx)
            {
                if (ctx.Target != null)
                {
                    var mi = InspectorEngine.FindMethod(ctx.Target.GetType(), name, args.Length, false);
                    if (mi != null) return InvokeRaw(mi, mi.IsStatic ? null : ctx.Target, args);
                }

                return null;
            }

            private static object InvokeRaw(MethodInfo mi, object target, object[] args)
            {
                try
                {
                    return mi.Invoke(mi.IsStatic ? null : target, CoerceArgs(mi, args));
                }
                catch (Exception e)
                {
                    LogWarning($"[Inspector] 方法调用失败 {mi.DeclaringType?.Name}.{mi.Name}: {e.Message}");
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
            private static object[] CoerceArgs(MethodInfo mi, object[] args)
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
            private static object NarrowNumber(double value, Type targetType)
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

            private static object[] EvalArgs(List<Node> args, InspectorExpressionContext ctx)
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

    /// <summary>
    /// 表达式 $property 访问器：State 联动的真实对象与状态。引擎绘制时作为上下文。
    /// </summary>
    public sealed class InspectorPropertyAccessor
    {
        internal InspectorExpressionContext Context;

        public InspectorPropertyState State => Context?.State;

        public object Value
        {
            get => Context?.Value;
            set
            {
                if (Context != null) Context.Value = value;
            }
        }
    }
}
