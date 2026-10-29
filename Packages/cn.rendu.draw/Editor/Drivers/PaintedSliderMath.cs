namespace RD.Draw.Editor
{
    /// <summary>
    /// 自绘滑杆的纯数学: 值 ↔ 指针位置。
    ///
    /// 单独成类是为了可测 —— 拖动滑杆时真正容易错的就是这几行(端点、越界、退化区间、零宽),
    /// 而它们与 UIElements 一点关系都没有, 不该被埋进 PointerMove 的回调里。
    ///
    /// 坐标系约定: <c>x</c> 与 <c>travel</c> 都以**滑块可以走的行程**为准 ——
    /// 轨道宽减去滑块宽, 滑块的左缘取值范围是 [0, travel]。
    /// </summary>
    internal static class PaintedSliderMath
    {
        /// <summary>值 → [0,1] 比例。区间退化(max &lt;= min)时恒为 0(松手也停在最小值, 不会乱跳)。</summary>
        public static float Ratio(float value, float min, float max)
        {
            var span = max - min;
            if (span <= 0f) return 0f;

            return Clamp01((value - min) / span);
        }

        /// <summary>比例 → 滑块左缘的偏移(像素)。</summary>
        public static float ThumbOffset(float value, float min, float max, float travel)
        {
            if (travel <= 0f) return 0f;
            return Ratio(value, min, max) * travel;
        }

        /// <summary>指针 x(相对轨道左缘) → 值。越界的点夹到两端, 行程为 0 时取最小值。</summary>
        public static float ValueFromX(float x, float travel, float min, float max)
        {
            if (travel <= 0f) return min;
            if (max - min <= 0f) return min;

            return min + (max - min) * Clamp01(x / travel);
        }

        private static float Clamp01(float value)
        {
            if (value < 0f) return 0f;
            if (value > 1f) return 1f;
            return value;
        }
    }
}
