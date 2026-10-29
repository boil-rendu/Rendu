using RD.Draw.Editor.Tables;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 诊断输出通道: 引擎把"表格几何"这类排障信息交给它, 而不是直接 Debug.Log。
    ///
    /// 为什么要有这个接口: 表格横向滚动条的成因只能靠数值对比定位(盒宽 / 行内各格宽 /
    /// 滚动区视口宽), 所以引擎会持续测量并输出。直接 Debug.Log 有两个问题:
    ///   1. 格式与频率是引擎硬编码的, 宿主想接到自己的日志窗口/测试断言里做不到;
    ///   2. "只在数值变化时打一次"需要跨帧记住上一次的签名 —— 若这份状态做成引擎内部静态,
    ///      就成了第二个进程级可变全局。
    /// 走接口后, 默认实现照旧打到 Console, 宿主可替换, 去重状态跟着实例走。
    ///
    /// 参数刻意用数组而不是引擎内部的 <c>TableColumn</c>: 诊断通道是一个公开契约,
    /// 不该把内部结构暴露给实现者。
    ///
    /// 这是一个**可选项**: 宿主不接管就用默认实现, 绘制行为不受影响。
    /// </summary>
    public interface IDiagnosticSink
    {
        /// <summary>一行表格即将开始绘制(清空上一行的单元格宽度记录)。</summary>
        void BeginTableRow();

        /// <summary>记录一个单元格的实测宽度; width &lt;= 0 表示"这次没量到", 实现应忽略。</summary>
        void AddTableCellWidth(float width);

        /// <summary>记录内层滚动区的内容宽; width &lt;= 0 表示"没量到", 实现应忽略。</summary>
        void SetTableScrollWidth(float width);

        /// <summary>
        /// 列宽分发的输入与结果。
        /// <paramref name="reserved"/> 是索引/删除列/滚动条 inset 占掉的固定宽,
        /// <paramref name="flex"/> 是留给弹性列均分的宽, <paramref name="flexCount"/> 是弹性列个数。
        /// </summary>
        void TableDistribution(float contentWidth, int reserved, int flex, int flexCount,
                               float[] columnWidths, bool[] explicitWidths);

        /// <summary>分发后真正使用的列宽(重绘时用)。</summary>
        void TableColumns(float[] columnWidths, string[] columnLabels, float contentWidth,
                          float rootWidth, float viewWidth, float viewportWidth);

        /// <summary>一行的几何汇总: 盒宽 / 行内格宽之和 / 滚动区宽。三者的差就是横向滚动条的成因。</summary>
        void TableGeometry(float boxWidth);
    }
}
