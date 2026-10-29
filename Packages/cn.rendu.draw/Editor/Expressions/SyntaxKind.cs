namespace RD.Draw.Editor.Expressions
{
    /// <summary>
    /// 抽象语法树节点的种类 —— <see cref="SyntaxNode.Kind"/> 的取值。
    ///
    /// 它就是一份"节点有哪几种形态"的清单: 字面量(Num/Str/Bool/Null)、变量与取值链(Var/Chain)、
    /// 调用与下标(Call/Index)、一元/二元运算(Unary/Binary)、三元与赋值(Ternary/Assign)。
    /// 求值器 <see cref="Interpreter"/> 按本枚举分派, 新增一种形态 = 这里加一项 + 解析器构造它
    /// + 求值器认它, 三处齐了才算一种新节点。
    /// </summary>
    internal enum SyntaxKind
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
}