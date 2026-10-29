using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RD.Draw.Editor.Tables
{
    /// <summary>
    /// <see cref="IDiagnosticSink"/> 的默认实现: 输出到 Unity Console, 且**只在数值变化时打一次**。
    ///
    /// 去重状态(上一次的输入签名)必须是跨帧的 —— 它属于这个实例, 而实例由
    /// <see cref="EngineServices"/> 进程级持有。若把签名做成静态, 就等于用"输出格式"这个理由
    /// 换来第二个进程级可变全局。
    ///
    /// 注意"先判变化、再拼字符串": 这个 sink 接在每张表格每次绘制的路径上, 即使从不打印,
    /// 每帧拼签名/建 StringBuilder 也是一笔稳定开销。签名因此用整数比较, 只有真的要打印时
    /// 才构造与原来完全一致的那几行文本。
    /// </summary>
    internal sealed class TableGeometrySink : IDiagnosticSink
    {
        /// <summary>上一次 [TableDist] 的输入(取整比较, 与原签名 F0 的粒度一致)。</summary>
        private int m_DistributionContent = int.MinValue;
        private int m_DistributionReserved;
        private int m_DistributionFlex;

        /// <summary>上一次 [TableCols] 的输入(内容宽与列宽总和的取整值)。</summary>
        private int m_ColumnsContent = int.MinValue;
        private int m_ColumnsTotal;

        /// <summary>上一次 [TableGeom] 的输入: 盒宽/行宽和/滚动宽, 以及每格宽度的取整快照。</summary>
        private int m_GeometryBox = int.MinValue;
        private int m_GeometryRowTotal;
        private int m_GeometryScroll;
        private readonly List<int> m_GeometryCells = new();

        /// <summary>数据行内层滚动区的内容宽(视口宽; 内容超宽时是内容宽)。</summary>
        private float m_ScrollWidth;

        /// <summary>一行里各单元格的实际宽度。</summary>
        private readonly List<float> m_CellWidths = new();

        public void BeginTableRow() => m_CellWidths.Clear();

        public void AddTableCellWidth(float width)
        {
            // <=0 = 这次没量到(后端没实现 IDriverDiagnostics, 或 Layout 事件里量不到真值)。
            // 记 0 进去只会让日志变成"cells=[0 0 0]"这种假象, 不如不记。
            if (width <= 0f) return;
            m_CellWidths.Add(width);
        }

        public void SetTableScrollWidth(float width)
        {
            if (width <= 0f) return;
            m_ScrollWidth = width;
        }

        public void TableDistribution(float contentWidth, int reserved, int flex, int flexCount,
                                      float[] columnWidths, bool[] explicitWidths)
        {
            // 原签名是 "{contentWidth:F0}|{reserved}|{flex}", 这里用同一粒度做整数比较。
            var content = Mathf.RoundToInt(contentWidth);
            if (content == m_DistributionContent && reserved == m_DistributionReserved &&
                flex == m_DistributionFlex)
            {
                return;
            }

            m_DistributionContent  = content;
            m_DistributionReserved = reserved;
            m_DistributionFlex     = flex;

            var names = new StringBuilder();
            for (var i = 0; i < columnWidths.Length; i++)
            {
                var isExplicit = explicitWidths != null && i < explicitWidths.Length && explicitWidths[i];
                names.Append($"[{i}](w={columnWidths[i]},exp={isExplicit}) ");
            }

            Debug.Log($"[TableDist] ContentWidth={contentWidth:F1} reserved={reserved} flex={flex} " +
                      $"flexCount={flexCount} cols={columnWidths.Length} :: {names}");
        }

        public void TableColumns(float[] columnWidths, string[] columnLabels, float contentWidth,
                                 float rootWidth, float viewWidth, float viewportWidth)
        {
            var total = 0f;
            for (var i = 0; i < columnWidths.Length; i++)
            {
                total += columnWidths[i];
            }

            // 原签名是 "{contentWidth:F0}|{total:F0}"; 不变就返回, 不碰 StringBuilder。
            var content = Mathf.RoundToInt(contentWidth);
            var sum     = Mathf.RoundToInt(total);
            if (content == m_ColumnsContent && sum == m_ColumnsTotal) return;

            m_ColumnsContent = content;
            m_ColumnsTotal   = sum;

            var widths = new StringBuilder();
            for (var i = 0; i < columnWidths.Length; i++)
            {
                var label = columnLabels != null && i < columnLabels.Length ? columnLabels[i] : "";
                widths.Append($"{label}={columnWidths[i]} ");
            }

            Debug.Log($"[TableCols] ViewWidth={viewWidth:F0} RootWidth={rootWidth:F1} " +
                      $"ViewportWidth={viewportWidth:F1} " +
                      $"ContentWidth={contentWidth:F1} total={total} cols={columnWidths.Length} :: {widths}");
        }

        public void TableGeometry(float boxWidth)
        {
            var rowTotal = 0f;
            for (var i = 0; i < m_CellWidths.Count; i++)
            {
                rowTotal += m_CellWidths[i];
            }

            var box    = Mathf.RoundToInt(boxWidth);
            var total  = Mathf.RoundToInt(rowTotal);
            var scroll = Mathf.RoundToInt(m_ScrollWidth);

            if (box == m_GeometryBox && total == m_GeometryRowTotal && scroll == m_GeometryScroll &&
                SameCellWidths())
            {
                return;
            }

            m_GeometryBox      = box;
            m_GeometryRowTotal = total;
            m_GeometryScroll   = scroll;
            m_GeometryCells.Clear();
            for (var i = 0; i < m_CellWidths.Count; i++)
            {
                m_GeometryCells.Add(Mathf.RoundToInt(m_CellWidths[i]));
            }

            var cells = new StringBuilder();
            for (var i = 0; i < m_CellWidths.Count; i++)
            {
                cells.Append($"{m_CellWidths[i]:F0} ");
            }

            Debug.Log($"[TableGeom] box={boxWidth:F0} rowTotal={rowTotal:F0} scroll={m_ScrollWidth:F0} " +
                      $"cells=[{cells}]");
        }

        /// <summary>当前行的取整宽度序列是否与上次打印时一致(数量与逐项都要相同)。</summary>
        private bool SameCellWidths()
        {
            if (m_CellWidths.Count != m_GeometryCells.Count) return false;

            for (var i = 0; i < m_CellWidths.Count; i++)
            {
                if (Mathf.RoundToInt(m_CellWidths[i]) != m_GeometryCells[i]) return false;
            }

            return true;
        }
    }
}
