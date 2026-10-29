namespace RD.Draw.Editor
{
    /// <summary>
    /// 一次拖拽排序在本帧的状态。
    /// </summary>
    public readonly struct ReorderDrag
    {
        /// <summary>
        /// 没有拖拽时的状态。不要用 <c>default</c>: 结构体的默认 <see cref="Source"/> 是 0,
        /// 会被当成"正在拖第 0 条", 空列表也会画出落点线。
        /// </summary>
        public static ReorderDrag None => new(-1, -1, false);

        /// <summary>被拖条目的下标; -1 = 没有条目正被拖。</summary>
        public readonly int Source;

        /// <summary>
        /// 松手会落到的**插入位置**(0..Count, 与列表下标同一坐标系);
        /// -1 = 算不出(条目区间还没量到)。
        /// </summary>
        public readonly int Target;

        /// <summary>本帧松手: 引擎据此把 Source 挪到 Target, 且只写一次。</summary>
        public readonly bool Dropped;

        public ReorderDrag(int source, int target, bool dropped)
        {
            Source  = source;
            Target  = target;
            Dropped = dropped;
        }

        /// <summary>拖拽进行中(含松手那一帧)。</summary>
        public bool Active => Source >= 0;
    }
}
