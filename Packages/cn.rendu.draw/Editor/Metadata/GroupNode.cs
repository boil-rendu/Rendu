using System.Collections.Generic;

namespace RD.Draw.Editor.Metadata
{
    /// <summary>
    /// 分组树节点。PropertyGroupAttribute 语义:
    /// 分组属性通过 GroupID("父/子" 路径) 把成员挂到一棵分组树上, GroupName 只取路径末段;
    /// 同级分组按 Order 排序; 没有挂分组属性的成员直接挂在根节点上(根节点永远可见)。
    /// 用"树"而不是"边遍历边开关 GUI 的状态机", 才能正确处理分组嵌套
    /// (如 [VerticalGroup("信息")] + [ButtonGroup("信息/更新版本")])与折叠,
    /// 也不会出现 Begin/End 失衡, 或"未分组成员被误判为折叠组而整组跳过"这类问题。
    /// </summary>
    internal sealed class GroupNode
    {
        public GroupKind Kind;
        public string GroupId = "";
        public string GroupName = "";
        public float Width;
        public double Order;
        public int Seq;
        public bool HideWhenChildrenAreInvisible = true;
        public string VisibleIf;
        public bool ShowLabel = true;
        public bool CenterLabel;
        public bool Expanded;
        public bool HorizontalLine = true;
        public bool BoldTitle = true;
        public TitleAlignment Alignment;
        public GroupNode Parent;
        public readonly List<GroupNode> Children = new();
        public readonly List<CachedMember> Members = new();

        /// <summary>
        /// 子分组与成员按 (Order, 声明序) 混排后的结果。
        /// 它是**树的一部分**, 不是绘制期的临时表: 排序键(Order/Seq)在建树时就固定,
        /// 由 <see cref="Rules.GroupResolver"/> 在建树结束时填好, 之后只读。
        /// </summary>
        public readonly List<GroupEntry> Entries = new();
    }
}
