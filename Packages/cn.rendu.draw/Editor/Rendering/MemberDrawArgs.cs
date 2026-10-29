using UnityEditor;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// "画这个成员"这件事的全部输入。
    ///
    /// 内嵌一个 <see cref="ValueDrawArgs"/>: 值控件那条路径需要的正是它的子集,
    /// 于是兜底 handler 不必重新拼一遍, 也不必把参数再拆散。
    /// 相比之下多出来的只有 host / depth / inTableCell —— 它们是"往哪儿画"的上下文,
    /// 值控件层用不上, 所以留在这个更大的结构里。
    /// </summary>
    internal readonly struct MemberDrawArgs
    {
        public MemberDrawArgs(ValueDrawArgs value, EditorWindow host, int depth, bool inTableCell)
        {
            Value      = value;
            Host       = host;
            Depth      = depth;
            InTableCell = inTableCell;
        }

        /// <summary>值的部分(标签/类型/当前值/成员元数据)。</summary>
        public ValueDrawArgs Value { get; }

        /// <summary>宿主窗口(内联展开时要把树的根给谁)。</summary>
        public EditorWindow Host { get; }

        /// <summary>嵌套深度(防止自引用导致的无限展开)。</summary>
        public int Depth { get; }

        /// <summary>是否在表格单元格内 —— 标签、进度条等会因此让位给表头。</summary>
        public bool InTableCell { get; }
    }
}
