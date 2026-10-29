using System;
using System.Collections.Generic;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Tables
{
    /// <summary>
    /// 表格结构: 由元素类型推出"有哪些列、每列有哪些成员、列宽是多少"。
    ///
    /// 列的形状只跟元素类型有关, 所以按类型缓存; 列宽里的"显式宽度"是静态信息,
    /// 而"均分出来的宽度"每帧由可用宽重算(见表格绘制部分)。
    /// </summary>
    internal sealed class TableSchema
    {
        private const int DefaultTableColumnWidth = 100;

        private readonly MemberCollector m_Members;
        private readonly ILabelFormatter m_Labels;
        private readonly Dictionary<Type, List<TableColumn>> m_Cache = new();

        public TableSchema(MemberCollector members, ILabelFormatter labels)
        {
            m_Members = members;
            m_Labels  = labels;
        }

        /// <summary>取某个元素类型的列定义。</summary>
        public List<TableColumn> Get(Type elementType)
        {
            if (elementType == null) return new List<TableColumn>();
            if (m_Cache.TryGetValue(elementType, out var cached))
            {
                return cached;
            }

            var allMembers = m_Members.Get(elementType);
            var columns = new List<TableColumn>();
            var seenKeys = new HashSet<string>();

            // [HideInTables]: 该成员不进表格 —— 既不单独成列, 也不出现在所属分组的单元格里。
            // 元素类型的所有成员都被隐藏时列集合为空, 上层据此退回普通列表(见 CollectionRenderer)。
            var members = new List<CachedMember>(allMembers.Length);
            foreach (var m in allMembers)
            {
                if (m.GetAttr<HideInTablesAttribute>() == null) members.Add(m);
            }

            // 按"分组"归并: 同一顶层分组的成员合并成唯一一列(单元格内竖向堆叠);
            // 无分组的成员用 "§成员名" 作唯一键, 各自独立成列。列序 = 分组首次出现顺序。
            foreach (var member in members)
            {
                var group = GetOuterGroup(member);
                var key = string.IsNullOrEmpty(group) ? "§" + member.RawName : group;
                if (seenKeys.Add(key) == false) continue;

                var groupMembers = new List<CachedMember>();
                var width = 0;
                var explicitWidth = 0;
                var resizable = true;
                foreach (var m in members)
                {
                    if (GetOuterGroup(m) != group) continue;
                    groupMembers.Add(m);
                    var w = m.GetAttr<TableColumnWidthAttribute>();
                    if (w != null && w.Width > 0)
                    {
                        // 显式列宽(取组内最大), 固定值
                        if (w.Width > explicitWidth) explicitWidth = w.Width;
                        // 同一列里只要有一个成员声明"不许拖", 这列就不许拖 ——
                        // 拖宽会连带改掉那个按像素排版的成员(按钮列、徽标列)的落点
                        if (w.Resizable == false) resizable = false;
                    }
                    else
                    {
                        // 成员在单元格内竖向堆叠, 列宽取最宽成员而非累加, 否则大分组会把表格撑出横向滚动条
                        if (DefaultTableColumnWidth > width) width = DefaultTableColumnWidth;
                    }
                }

                var widthIsExplicit = explicitWidth > 0;
                width = widthIsExplicit ? explicitWidth : width;

                columns.Add(new TableColumn
                {
                    Members         = groupMembers,
                    GroupKey        = group,
                    GroupName       = group,
                    Label           = string.IsNullOrEmpty(group) ? m_Labels.GetLabel(groupMembers[0]) : group,
                    Width           = width,
                    WidthIsExplicit = widthIsExplicit,
                    Resizable       = resizable,
                });
            }

            m_Cache[elementType] = columns;
            return columns;
        }

        /// <summary>取最外层的分组 ID(路径深度最小), 作为表头合并带的依据。</summary>
        public static string GetOuterGroup(CachedMember m)
        {
            var best = "";
            var bestDepth = int.MaxValue;
            foreach (var attr in m.Attrs)
            {
                if (!(attr is PropertyGroupAttribute group)) continue;

                var id = string.IsNullOrEmpty(group.GroupID) ? "" : group.GroupID.Trim('/');
                if (string.IsNullOrEmpty(id)) continue;

                var depth = GroupResolver.PathDepth(id);
                if (depth < bestDepth)
                {
                    bestDepth = depth;
                    var index = id.IndexOf('/');
                    best = index < 0 ? id : id.Substring(0, index);
                }
            }

            return best;
        }
    }
}
