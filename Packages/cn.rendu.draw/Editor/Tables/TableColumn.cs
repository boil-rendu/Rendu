using System.Collections.Generic;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Tables
{
    /// <summary>
    /// 表格的一列。约定: 同一个分组(VerticalGroup/BoxGroup...)的成员合并成唯一一列,
    /// 该列单元格内竖向堆叠这些成员; 无分组的成员各自独立成列。
    /// </summary>
    internal sealed class TableColumn
    {
        public List<CachedMember> Members = new();
        public string GroupKey = "";   // 顶层分组 ID(用于合并表头)
        public string GroupName = "";
        public string Label = "";
        public int Width = 100;
        public bool WidthIsExplicit = false;   // 列宽是否由 [TableColumnWidth] 显式指定(否则弹性均分撑满)
        public bool Resizable = true;          // 列宽能否被用户拖拽(来自 [TableColumnWidth(Resizable = false)] 的否决)

        /// <summary>
        /// 用户拖拽列宽在会话里的键("表格成员名#列序")。
        ///
        /// 只在**每帧的副本**上填(见 TableRenderer.CloneColumns): 列定义按元素类型缓存,
        /// 同一个键在不同表格里并不相同(表成员名不同), 写进缓存会串。
        /// </summary>
        public string SessionKey = "";
    }
}
