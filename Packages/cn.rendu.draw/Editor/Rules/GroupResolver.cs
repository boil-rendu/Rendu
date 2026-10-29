using System;
using System.Collections.Generic;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rendering;

namespace RD.Draw.Editor.Rules
{
    /// <summary>
    /// 分组树构建: 把成员按它们的分组特性挂到一棵树上, 并按类型缓存。
    ///
    /// 树的形状只跟类型有关(成员集合 + 分组特性), 所以缓存可以跨帧复用。
    /// </summary>
    internal sealed class GroupResolver
    {
        private readonly Dictionary<Type, GroupNode> m_Cache = new();

        /// <summary>取某个类型的分组树(根节点代表"未分组"区域)。</summary>
        public GroupNode Get(Type type, CachedMember[] members)
        {
            if (type == null) return new GroupNode { Kind = GroupKind.None };
            if (m_Cache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var root = new GroupNode { Kind = GroupKind.None };
            foreach (var m in members)
            {
                var node = ResolveGroupNode(root, m);
                node.Members.Add(m);
            }

            SortGroupTree(root);
            m_Cache[type] = root;
            return root;
        }

        /// <summary>按成员的(可多个)分组属性, 在分组树上定位它所属的最内层分组。</summary>
        private static GroupNode ResolveGroupNode(GroupNode root, CachedMember m)
        {
            var descriptors = new List<GroupDescriptor>();
            foreach (var attr in m.Attrs)
            {
                if (attr is PropertyGroupAttribute group)
                {
                    descriptors.Add(DescribeGroup(group));
                }
            }

            if (descriptors.Count == 0)
            {
                return root;
            }

            // 父分组先建: 按 GroupID 路径深度升序, 同深度保持属性声明顺序。
            descriptors.Sort((a, b) => PathDepth(a.GroupId).CompareTo(PathDepth(b.GroupId)));

            // 每个分组属性路径的落点; 描述符按路径深度升序, 最后一个(最深)的叶子即成员所属的最内层组
            var leaf = root;
            foreach (var descriptor in descriptors)
            {
                // 每个分组属性都从根沿自己的 GroupID 路径走。不能接着上一个 descriptor 的终点继续走 ——
                // 那样第二个属性会从第一个组的内部重新找同名前缀, 造出"组里套同名组"的重复中间节点,
                // 成员(及其 Width 等描述)会落到副本节点上(视觉上多一层无名容器/重名标头)。
                var current = root;
                var segments = descriptor.GroupId.Split('/');
                var path = "";
                for (var i = 0; i < segments.Length; i++)
                {
                    var segment = segments[i];
                    if (string.IsNullOrEmpty(segment)) continue;

                    path = path.Length == 0 ? segment : path + "/" + segment;
                    var isLeaf = i == segments.Length - 1;

                    var child = FindChild(current, path);
                    if (child == null)
                    {
                        child = new GroupNode
                        {
                            GroupId   = path,
                            GroupName = segment,
                            Parent    = current,
                            Kind      = GroupKind.Vertical, // 隐式中间层, 真实类型由它自己的分组属性补齐
                            Seq       = m.Seq,
                        };
                        current.Children.Add(child);
                    }

                    if (isLeaf)
                    {
                        ApplyDescriptor(child, descriptor, m.Seq);
                        leaf = child;
                    }

                    current = child;
                }
            }

            return leaf;
        }

        private static GroupNode FindChild(GroupNode parent, string groupId)
        {
            for (var i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i].GroupId == groupId) return parent.Children[i];
            }

            return null;
        }

        /// <summary>分组路径深度: "信息" 是 1, "信息/更新版本" 是 2。</summary>
        public static int PathDepth(string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) return 0;
            var depth = 1;
            for (var i = 0; i < groupId.Length; i++)
            {
                if (groupId[i] == '/') depth++;
            }

            return depth;
        }

        /// <summary>
        /// 把一个分组属性解析成组描述: 全部渲染语义(Kind / 标题显隐 / 折叠默认态 / 标题样式 / 宽度)
        /// 由特性自身的虚成员给出, 本方法只做搬运 —— 新增分组特性时改特性类即可, 这里不动。
        /// </summary>
        private static GroupDescriptor DescribeGroup(PropertyGroupAttribute group)
        {
            return new GroupDescriptor
            {
                GroupId                      = string.IsNullOrEmpty(group.GroupID) ? "_DefaultGroup" : group.GroupID.Trim('/'),
                Order                        = group.Order,
                Kind                         = group.Kind,
                ShowLabel                    = group.ShowLabel,
                Expanded                     = group.Expanded,
                CenterLabel                  = group.CenterLabel,
                HorizontalLine               = group.HorizontalLine,
                BoldTitle                    = group.BoldTitle,
                Alignment                    = group.Alignment,
                HideWhenChildrenAreInvisible = group.HideWhenChildrenAreInvisible,
                VisibleIf                    = group.VisibleIf,
                Width                        = group.Width,
            };
        }

        private static void ApplyDescriptor(GroupNode node, GroupDescriptor descriptor, int seq)
        {
            node.Kind                         = descriptor.Kind;
            node.Order                        = descriptor.Order;
            node.ShowLabel                    = descriptor.ShowLabel;
            node.CenterLabel                  = descriptor.CenterLabel;
            node.Width                        = descriptor.Width;
            node.Expanded                     = descriptor.Expanded;
            node.HorizontalLine               = descriptor.HorizontalLine;
            node.BoldTitle                    = descriptor.BoldTitle;
            node.Alignment                    = descriptor.Alignment;
            node.HideWhenChildrenAreInvisible = descriptor.HideWhenChildrenAreInvisible;
            node.VisibleIf                    = descriptor.VisibleIf;
            if (seq < node.Seq) node.Seq = seq;
        }

        private static void SortGroupTree(GroupNode node)
        {
            node.Members.Sort((a, b) =>
            {
                if (a.Order != b.Order) return a.Order.CompareTo(b.Order);
                return a.Seq.CompareTo(b.Seq);
            });

            node.Children.Sort((a, b) =>
            {
                var oa = GroupSortOrder(a);
                var ob = GroupSortOrder(b);
                if (oa != ob) return oa.CompareTo(ob);
                return a.Seq.CompareTo(b.Seq);
            });

            // 混排结果在这里一次算好(见 GroupNode.Entries): 排序键此后不再变化,
            // 绘制期每帧重排同一个节点纯属重复劳动。
            GroupTraverse.Rebuild(node);

            foreach (var child in node.Children)
            {
                SortGroupTree(child);
            }
        }

        /// <summary>分组定位顺序: 显式指定 Order 时用它, 否则跟随组内最小 PropertyOrder 的位置。</summary>
        public static double GroupSortOrder(GroupNode node)
        {
            return node.Order != 0 ? node.Order : MinMemberOrder(node);
        }

        /// <summary>组内(含子孙)成员的最小 PropertyOrder。</summary>
        private static double MinMemberOrder(GroupNode node)
        {
            var min = double.MaxValue;
            foreach (var m in node.Members)
            {
                if (m.Order < min) min = m.Order;
            }

            foreach (var child in node.Children)
            {
                var childMin = MinMemberOrder(child);
                if (childMin < min) min = childMin;
            }

            return min == double.MaxValue ? 0d : min;
        }

        /// <summary>由一个分组属性解析出的描述(一个分组属性实例)。</summary>
        private struct GroupDescriptor
        {
            public string GroupId;
            public double Order;
            public GroupKind Kind;
            public bool ShowLabel;
            public bool CenterLabel;
            public bool Expanded;
            public bool HorizontalLine;
            public bool BoldTitle;
            public TitleAlignment Alignment;
            public bool HideWhenChildrenAreInvisible;
            public string VisibleIf;
            public float Width;
        }
    }
}
