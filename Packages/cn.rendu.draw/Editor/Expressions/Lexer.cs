using System;
using System.Collections.Generic;
using System.Globalization;

namespace RD.Draw.Editor.Expressions
{
    //-------------------------------------- Tokenizer --------------------------------------

    /// <summary>一个词法单元: 种类 + 原文(数字/标识符带解析好的值)。</summary>
    internal struct Token
    {
        public TokenKind Kind;
        public string    Text;
        public double    Num;
    }

    /// <summary>词法分析: 表达式源码 → 词法单元流。句法在 Parser, 求值在 Interpreter。</summary>
    internal static class Lexer
    {

        internal static readonly string[] MultiOps = { "==", "!=", "<=", ">=", "&&", "||" };

        internal static List<Token> Tokenize(string code)
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
                    tokens.Add(new Token { Kind = TokenKind.String, Text = sb.ToString() });
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
                        tokens.Add(new Token { Kind = TokenKind.Number, Text = text, Num = num });
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

                    tokens.Add(new Token { Kind = TokenKind.Ident, Text = code.Substring(start, i - start) });
                    continue;
                }

                var matched = false;
                foreach (var op in MultiOps)
                {
                    if (i + op.Length <= n && code.Substring(i, op.Length) == op)
                    {
                        tokens.Add(new Token { Kind = TokenKind.Op, Text = op });
                        i += op.Length;
                        matched = true;
                        break;
                    }
                }

                if (matched) continue;

                if ("=<>!?:.()+-*/%,&|[]".IndexOf(c) >= 0)
                {
                    tokens.Add(new Token { Kind = TokenKind.Op, Text = c.ToString() });
                    i++;
                    continue;
                }

                return null;
            }

            tokens.Add(new Token { Kind = TokenKind.End, Text = "" });
            return tokens;
        }
    }
}
