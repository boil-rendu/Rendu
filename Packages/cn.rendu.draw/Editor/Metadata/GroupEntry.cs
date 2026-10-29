using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Metadata
{
    /// <summary>
    /// 分组树的一项: 要么是一个子分组, 要么是一个成员。
    /// 它属于**树**, 不属于渲染器 —— 枚举"这个节点下有哪些项、它们的先后顺序"(排序规则: 先 Order 后声明序)
    /// 是分组树的结构知识, 与"怎么画"无关, 因此独立于渲染器。
    ///
    /// 实现 <see cref="IComparable{T}"/> 让调用方直接 <c>Sort()</c>, 不必知道排序键是什么。
    /// </summary>
    internal readonly struct GroupEntry : System.IComparable<GroupEntry>
    {
        private readonly double m_Order;
        private readonly int    m_Seq;

        private GroupEntry(CachedMember member, GroupNode group, double order, int seq)
        {
            Member = member;
            Group  = group;
            m_Order = order;
            m_Seq   = seq;
        }

        public static GroupEntry ForMember(CachedMember member)
        {
            return new GroupEntry(member, null, member.Order, member.Seq);
        }

        public static GroupEntry ForGroup(GroupNode group)
        {
            return new GroupEntry(null, group, GroupResolver.GroupSortOrder(group), group.Seq);
        }

        /// <summary>成员项(与 <see cref="Group"/> 二选一, 不会同时为真)。</summary>
        public CachedMember Member { get; }

        /// <summary>子分组项(与 <see cref="Member"/> 二选一, 不会同时为真)。</summary>
        public GroupNode Group { get; }

        public bool IsGroup => Group != null;

        /// <summary>先按 Order, 再按声明序(Seq) —— 保证同 Order 时仍是声明顺序。</summary>
        public int CompareTo(GroupEntry other)
        {
            if (m_Order != other.m_Order) return m_Order.CompareTo(other.m_Order);
            return m_Seq.CompareTo(other.m_Seq);
        }
    }
}
