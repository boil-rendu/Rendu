using System.Collections.Generic;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// 分组树的遍历与枚举 —— 树这一侧的结构知识。
    ///
    /// 渲染器只负责"把拿到的项画出来"; 怎么混排、怎么排序、折叠组要不要收尾,
    /// 这些都是树的事, 放在这里而不是散在绘制代码里。
    /// </summary>
    internal static class GroupTraverse
    {
        /// <summary>
        /// 枚举某节点下待处理的条目: 子分组与成员按 (Order, 声明序) 混排成一条列表。
        ///
        /// 排序是<b>稳定</b>的 —— 同一 Order 下按 <c>Seq</c>(成员/分组在类型里的声明序),
        /// 因此"用户只写了 Order、没管先后"时仍然得到可预期的排布。
        ///
        /// 结果在 <see cref="GroupResolver"/> 建树时排好并挂在 <see cref="GroupNode.Entries"/> 上,
        /// 这里只负责取; 每帧不再 new List / Sort。
        /// </summary>
        public static List<GroupEntry> Entries(GroupNode node)
        {
            if (node == null) return Empty;
            // 兜底: 树若不是由 GroupResolver 建的(或将来有地方手工造节点), 这里补排一次。
            if (node.Entries.Count == 0 && (node.Children.Count > 0 || node.Members.Count > 0))
            {
                Rebuild(node);
            }

            return node.Entries;
        }

        private static readonly List<GroupEntry> Empty = new();

        /// <summary>
        /// 把节点当前的子分组/成员重排进 <see cref="GroupNode.Entries"/>。
        /// 由 <see cref="GroupResolver"/> 在"成员与子分组都排好序之后"调用一次。
        /// </summary>
        internal static void Rebuild(GroupNode node)
        {
            var entries = node.Entries;
            entries.Clear();
            entries.Capacity = node.Children.Count + node.Members.Count;

            foreach (var child in node.Children)
            {
                entries.Add(GroupEntry.ForGroup(child));
            }

            foreach (var member in node.Members)
            {
                entries.Add(GroupEntry.ForMember(member));
            }

            entries.Sort();
        }
    }
}
