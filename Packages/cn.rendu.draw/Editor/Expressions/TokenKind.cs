namespace RD.Draw.Editor.Expressions
{
    /// <summary>词法单元的种类 —— <see cref="Token.Kind"/> 的取值。</summary>
    internal enum TokenKind
    {
        /// <summary>标识符(变量名 / 成员名 / 方法名)。</summary>
        Ident,

        /// <summary>数值字面量(取值在 <see cref="Token.Num"/>)。</summary>
        Number,

        /// <summary>字符串字面量(取值在 <see cref="Token.Text"/>)。</summary>
        String,

        /// <summary>运算符或标点(取值在 <see cref="Token.Text"/>)。</summary>
        Op,

        /// <summary>流末尾标记: 解析器靠它判定"没有下一个了", 不对应任何字符。</summary>
        End,
    }
}