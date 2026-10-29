
namespace RD.Draw.Editor.Expressions
{
    /// <summary>
    /// 抽象语法树的一个节点。
    ///
    /// 它是**所有**形态共用的一个扁平节点(不是每个形态一个子类): 解析器造它、求值器读它,
    /// 两者共用同一套 <see cref="SyntaxKind"/> 分派, 加一种形态不必同步改一串子类。
    /// 代价是"用不到的槽位空着", 换来的是解析与求值两侧代码形状一致、少一层类型判断。
    ///
    /// 节点一经构造就不再变化(<see cref="ExpressionEvaluator"/> 会把整棵树缓存下来跨帧复用),
    /// 因此字段是公开的只读槽位而不是属性。
    /// </summary>
    internal sealed class SyntaxNode
    {
        /// <summary>节点形态。见 <see cref="SyntaxKind"/>。</summary>
        public SyntaxKind Kind;

        /// <summary>Num 的数值。</summary>
        public double Num;

        /// <summary>Var 名称 / 运算符 / Call 方法名。</summary>
        public string S;

        /// <summary>Bool 的字面量。</summary>
        public bool B;

        /// <summary>左子节点 / 三元的条件 / Chain 的根。</summary>
        public SyntaxNode A;

        /// <summary>右子节点 / 三元的 then 分支 / Assign 的值。</summary>
        public SyntaxNode B_;

        /// <summary>三元表达式的 else 分支。</summary>
        public SyntaxNode C;

        /// <summary>Call 的实参 / 取值链的每一步参数。</summary>
        public System.Collections.Generic.List<SyntaxNode> Args;

        public SyntaxNode(SyntaxKind kind)
        {
            Kind = kind;
        }
    }
}