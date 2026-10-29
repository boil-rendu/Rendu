using System;
using System.Collections.Generic;

namespace RD.Draw.Editor.Expressions
{
    internal sealed class Parser
    {
        internal readonly List<Token> m_Tokens;
        internal int m_Pos;
        internal bool m_Error;

        public Parser(List<Token> tokens)
        {
            m_Tokens = tokens;
        }

        internal Token Peek()
        {
            return m_Tokens[Math.Min(m_Pos, m_Tokens.Count - 1)];
        }

        internal Token Next()
        {
            var t = Peek();
            if (m_Pos < m_Tokens.Count - 1) m_Pos++;
            return t;
        }

        internal bool Accept(string op)
        {
            var t = Peek();
            if (t.Kind == TokenKind.Op && t.Text == op)
            {
                Next();
                return true;
            }

            return false;
        }

        internal bool IsOp(string op)
        {
            var t = Peek();
            return t.Kind == TokenKind.Op && t.Text == op;
        }

        public SyntaxNode ParseRoot()
        {
            var left = ParseAssign();

            // 这个解析器是"遇到不认识的东西就停下"的风格。不校验的话, @A[0]、@A. 这类
            // 会被静默截断成一个错值(比如 @Items[0 会变成整个 @Items), 调用方却看不到异常。
            // 两种截断都要拦: 还剩着没消费的 token, 以及解析中途主动放弃(置了 m_Error)。
            if (left == null || m_Error || Peek().Kind != TokenKind.End) return null;

            return left;
        }

        internal SyntaxNode ParseAssign()
        {
            var left = ParseTernary();
            if (Accept("="))
            {
                var right = ParseAssign();
                if (left == null) return right;
                return new SyntaxNode(SyntaxKind.Assign) { A = left, B_ = right };
            }

            return left;
        }

        internal SyntaxNode ParseTernary()
        {
            var cond = ParseOr();
            if (Accept("?"))
            {
                var whenTrue = ParseTernary();
                if (Accept(":") == false) return cond;
                var whenFalse = ParseTernary();
                return new SyntaxNode(SyntaxKind.Ternary) { A = cond, B_ = whenTrue, C = whenFalse };
            }

            return cond;
        }

        internal SyntaxNode ParseOr()
        {
            var left = ParseAnd();
            while (Accept("||"))
            {
                left = new SyntaxNode(SyntaxKind.Binary) { S = "||", A = left, B_ = ParseAnd() };
            }

            return left;
        }

        internal SyntaxNode ParseAnd()
        {
            var left = ParseEquality();
            while (Accept("&&"))
            {
                left = new SyntaxNode(SyntaxKind.Binary) { S = "&&", A = left, B_ = ParseEquality() };
            }

            return left;
        }

        internal SyntaxNode ParseEquality()
        {
            var left = ParseRelational();
            while (Accept("==") || Accept("!="))
            {
                var op = m_Tokens[m_Pos - 1].Text;
                left = new SyntaxNode(SyntaxKind.Binary) { S = op, A = left, B_ = ParseRelational() };
            }

            return left;
        }

        internal SyntaxNode ParseRelational()
        {
            var left = ParseAdditive();
            while (Accept("<") || Accept("<=") || Accept(">") || Accept(">="))
            {
                var op = m_Tokens[m_Pos - 1].Text;
                left = new SyntaxNode(SyntaxKind.Binary) { S = op, A = left, B_ = ParseAdditive() };
            }

            return left;
        }

        internal SyntaxNode ParseAdditive()
        {
            var left = ParseMultiplicative();
            while (Accept("+") || Accept("-"))
            {
                var op = m_Tokens[m_Pos - 1].Text;
                left = new SyntaxNode(SyntaxKind.Binary) { S = op, A = left, B_ = ParseMultiplicative() };
            }

            return left;
        }

        internal SyntaxNode ParseMultiplicative()
        {
            var left = ParseUnary();
            while (Accept("*") || Accept("/") || Accept("%"))
            {
                var op = m_Tokens[m_Pos - 1].Text;
                left = new SyntaxNode(SyntaxKind.Binary) { S = op, A = left, B_ = ParseUnary() };
            }

            return left;
        }

        internal SyntaxNode ParseUnary()
        {
            if (Accept("!")) return new SyntaxNode(SyntaxKind.Unary) { S = "!", A = ParseUnary() };
            if (Accept("-")) return new SyntaxNode(SyntaxKind.Unary) { S = "-", A = ParseUnary() };
            return ParsePostfix();
        }

        internal SyntaxNode ParsePostfix()
        {
            var primary = ParsePrimary();
            if (primary == null) return null;

            // 方法/变量根带括号 -> 调用。结果仍可继续接成员访问或索引(如 @GetList()[0])
            if (primary.Kind == SyntaxKind.Var && IsOp("("))
            {
                Next();
                primary = new SyntaxNode(SyntaxKind.Call) { S = primary.S, Args = ParseArgs() };
            }

            // 成员访问 / 索引链
            if (IsOp(".") == false && IsOp("[") == false) return primary;

            var chain = new SyntaxNode(SyntaxKind.Chain) { A = primary, Args = new List<SyntaxNode>() };
            while (true)
            {
                if (Accept("."))
                {
                    var id = Next();
                    if (id.Kind != TokenKind.Ident)
                    {
                        m_Error = true;
                        return chain;
                    }

                    if (Accept("("))
                    {
                        chain.Args.Add(new SyntaxNode(SyntaxKind.Call) { S = id.Text, Args = ParseArgs() });
                    }
                    else
                    {
                        chain.Args.Add(new SyntaxNode(SyntaxKind.Var) { S = id.Text });
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

                    chain.Args.Add(new SyntaxNode(SyntaxKind.Index) { A = index });
                    continue;
                }

                return chain;
            }
        }

        internal List<SyntaxNode> ParseArgs()
        {
            var args = new List<SyntaxNode>();
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

        internal SyntaxNode ParsePrimary()
        {
            var t = Peek();
            switch (t.Kind)
            {
                case TokenKind.Number:
                    Next();
                    return new SyntaxNode(SyntaxKind.Num) { Num = t.Num };
                case TokenKind.String:
                    Next();
                    return new SyntaxNode(SyntaxKind.Str) { S = t.Text };
                case TokenKind.Ident:
                    Next();
                    if (t.Text == "true") return new SyntaxNode(SyntaxKind.Bool) { B = true };
                    if (t.Text == "false") return new SyntaxNode(SyntaxKind.Bool) { B = false };
                    if (t.Text == "null") return new SyntaxNode(SyntaxKind.Null);
                    return new SyntaxNode(SyntaxKind.Var) { S = t.Text };
                case TokenKind.Op:
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
}
