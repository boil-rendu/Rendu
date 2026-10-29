using System;
using System.Collections.Generic;
using System.Reflection;
using RD.Draw.Editor.Expressions;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor
{
    public static class ExpressionEvaluator
    {
    internal const BindingFlags AllFlags = BindingFlags.Public | BindingFlags.NonPublic |
                                          BindingFlags.Instance | BindingFlags.Static;

    internal static Dictionary<string, List<Type>> s_NameToTypes;
    internal static readonly object s_IndexLock = new object();

    /// <summary>
    /// 表达式源码(去掉 @ 后) → AST。AST 解析完就不再变化, 而同一个表达式在每次重绘时都会被重算,
    /// 于是词法 + 语法分析被白白重复几千遍并产生成吨临时对象。缓存后稳态下只剩求值。
    /// 只在编辑器主线程访问(与绘制同线程), 因此不加锁。
    /// </summary>
    internal static readonly Dictionary<string, SyntaxNode> s_AstCache = new();

    /// <summary>
    /// 解析失败标记(负缓存)。
    /// 写错的表达式每帧都会重新判定: 没有负缓存就会每帧重新词法分析一遍、每帧重报一条日志。
    /// 缓存下来后"报一次、之后当作空表达式"。
    /// </summary>
    internal static readonly SyntaxNode s_ParseFailed = new SyntaxNode(SyntaxKind.Null);

    /// <summary>
    /// AST 缓存上限。表达式来自特性参数, 数量天然有限; 但 Evaluate 是公开 API, 若被塞进
    /// 动态拼出来的字符串, 缓存会无界增长 —— 到上限整体清空即可(重新解析很便宜)。
    /// </summary>
    internal const int AstCacheLimit = 512;

    /// <summary>
    /// 按类型缓存的索引器(只认单参数索引器)与形参类型。
    /// 每走一次自定义 this[...] 就 GetProperties 扫全表 + 取一次参数数组, 这两笔分配都随重绘反复发生。
    /// </summary>
    internal static readonly Dictionary<Type, (PropertyInfo Property, Type ParamType)[]> s_IndexerCache = new();

    //-------------------------------------- 对外 API --------------------------------------

    public static bool IsExpression(string s)
    {
        return !string.IsNullOrEmpty(s) && s[0] == '@';
    }

    public static object Evaluate(string expression, object target, object value, PropertyState state)
    {
        if (string.IsNullOrEmpty(expression)) return null;
        var code = expression[0] == '@' ? expression.Substring(1) : expression;

        var node = GetAst(code);
        if (node == null || ReferenceEquals(node, s_ParseFailed)) return null;

        try
        {
            var ctx = new ExpressionContext
            {
                Target = target,
                Value = value,
                State = state ?? new PropertyState(),
            };
            return Interpreter.Eval(node, ctx);
        }
        catch (Exception e)
        {
            DrawLog.Warn($"[Inspector] 表达式求值失败 '{expression}': {e.Message}");
            return null;
        }
    }

    internal static SyntaxNode GetAst(string code)
    {
        if (s_AstCache.TryGetValue(code, out var cached)) return cached;

        if (s_AstCache.Count >= AstCacheLimit) s_AstCache.Clear();

        SyntaxNode node;
        try
        {
            var tokens = Lexer.Tokenize(code);
            node = tokens == null || tokens.Count == 0 ? null : new Parser(tokens).ParseRoot();
        }
        catch (Exception e)
        {
            DrawLog.Warn($"[Inspector] 表达式解析失败 '{code}': {e.Message}");
            node = null;
        }

        if (node == null)
        {
            // 解析不出来就不再重试: 只报一次, 之后按空表达式处理(返回 null, 条件判定为假)。
            // 这条警告很重要 —— 没有它, 一个拼错的表达式会永远静默地把成员藏起来。
            DrawLog.Warn($"[Inspector] 表达式语法错误, 已忽略: {code}");
            s_AstCache[code] = s_ParseFailed;
            return s_ParseFailed;
        }

        s_AstCache[code] = node;
        return node;
    }
    public static bool EvaluateBool(string expression, object target, object value, PropertyState state)
    {
        return Truthy(Evaluate(expression, target, value, state));
    }

    public static bool Truthy(object o)
    {
        return ValueOps.Truthy(o);
    }

    }
}
