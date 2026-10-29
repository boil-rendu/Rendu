using UnityEngine;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 分页区间: "这一页从第几条画到第几条"。
    ///
    /// 规则收在这里, 并把"页数至少 1、页索引不小于 0"写成不变式 —— 绘制循环不必再自己防越界。
    ///
    /// 它是纯计算, 不碰 UI、不读会话: 会话里的页码由调用方取出来传进去, 画完再写回。
    /// </summary>
    internal readonly struct PagingRange
    {
        private PagingRange(bool enabled, int pageSize, int pageCount, int pageIndex, int start, int end)
        {
            Enabled   = enabled;
            PageSize  = pageSize;
            PageCount = pageCount;
            PageIndex = pageIndex;
            Start     = start;
            End       = end;
        }

        /// <summary>调用方是否启用了分页(仍可能只有一页)。</summary>
        public bool Enabled { get; }

        /// <summary>每页条数(至少 1)。</summary>
        public int PageSize { get; }

        /// <summary>总页数(至少 1; 空数据也是 1 页, 只是没有内容)。</summary>
        public int PageCount { get; }

        /// <summary>当前页下标(已夹到 [0, PageCount - 1])。</summary>
        public int PageIndex { get; }

        /// <summary>本页首条下标(含)。</summary>
        public int Start { get; }

        /// <summary>本页末条下标(不含), 即 [Start, End) 是要画的数据行。</summary>
        public int End { get; }

        /// <summary>
        /// 由数据条数、每页条数与"用户上次停留的页"算出本次的区间。
        /// 未启用分页时返回整段 [0, itemCount)。
        /// </summary>
        public static PagingRange Compute(int itemCount, int pageSize, bool enabled, int requestedPage)
        {
            if (itemCount < 0) itemCount = 0;
            if (pageSize <= 0) pageSize = 1;

            if (enabled == false)
            {
                return new PagingRange(false, pageSize, 1, 0, 0, itemCount);
            }

            var pageCount = Mathf.Max(1, Mathf.CeilToInt((float)itemCount / pageSize));
            var pageIndex = Mathf.Clamp(requestedPage, 0, pageCount - 1);
            var start     = pageIndex * pageSize;
            var end       = Mathf.Min(start + pageSize, itemCount);
            return new PagingRange(true, pageSize, pageCount, pageIndex, start, end);
        }
    }
}
