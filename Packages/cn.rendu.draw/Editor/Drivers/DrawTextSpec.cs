namespace RD.Draw.Editor
{
    /// <summary>
    /// 一次文本绘制的要求: 字体样式 + 宽高约束。后端自行映射成自己的样式体系
    /// (保留模式走 class/style 属性)。
    ///
    /// 宽度有三种语义:
    /// - <see cref="Width"/> 定宽(min=max=w): 宽度精确, 但会把外层容器的最小宽一起撑大;
    /// - <see cref="MaxWidth"/> 上限宽(min=0, max=w): 宽度够时与定宽一样, 最小宽为 0 ——
    ///   用在"要精确对齐、但不能把外层撑宽"的地方(表格表头);
    /// - 两者都为 0: 自然宽。
    /// </summary>
    public struct DrawTextSpec
    {
        public DrawAlign Align;
        public bool       Bold;
        public bool       Mini;
        public bool       Wrap;
        public bool       RichText;
        public float      Width;
        public float      MaxWidth;
        public float      Height;

        /// <summary>按需构造: 只有要改的项才传, 其余取默认值(左对齐、非加粗、自然宽)。</summary>
        public static DrawTextSpec Of(DrawAlign align = DrawAlign.Left, bool bold = false, bool mini = false,
                                      bool wrap = false, bool richText = false, float width = 0f, float height = 0f,
                                      float maxWidth = 0f)
        {
            return new DrawTextSpec
            {
                Align    = align,
                Bold     = bold,
                Mini     = mini,
                Wrap     = wrap,
                RichText = richText,
                Width    = width,
                MaxWidth = maxWidth,
                Height   = height,
            };
        }
    }
}