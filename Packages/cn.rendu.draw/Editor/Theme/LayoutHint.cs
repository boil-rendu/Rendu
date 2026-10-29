namespace RD.Draw.Editor
{
    /// <summary>
    /// 中立的布局描述: 渲染器把它翻译成 style.*(保留模式的布局属性)。
    ///
    /// 它是 <see cref="LayoutPolicy"/> 的产物 —— 只表达"控件该多宽、多大边距、何时撑满"这些
    /// **决策**, 不碰  也不碰 UIElements。两个渲染器各自做"LayoutHint → 本地 API"的机械翻译。
    ///
    /// 数值约定: 尺寸用 0 表示"未指定"(与现有代码的 <c>&gt; 0</c> 判断一致); 边距/内边距用 0 表示"无"。
    /// </summary>
    internal struct LayoutHint
    {
        /// <summary>占满可用宽(横排里是主轴 flexGrow, 竖排里是交叉轴 alignSelf)。</summary>
        public bool FillWidth;

        /// <summary>占满可用高。</summary>
        public bool FillHeight;

        /// <summary>定宽(min=max=w)。0 = 未指定。</summary>
        public float Width;

        /// <summary>定高。0 = 未指定(翻译层各自决定缺省高度)。</summary>
        public float Height;

        /// <summary>最小宽。0 = 允许收缩(不锁下限); &gt;0 = 锁下限。</summary>
        public float MinWidth;

        /// <summary>最小高。0 = 未指定。</summary>
        public float MinHeight;

        /// <summary>上限宽(min=0, max=w)。0 = 未指定。</summary>
        public float MaxWidth;

        /// <summary>上限高。0 = 未指定。</summary>
        public float MaxHeight;

        public float MarginLeft;
        public float MarginRight;
        public float MarginTop;
        public float MarginBottom;

        public float PaddingLeft;
        public float PaddingRight;
        public float PaddingTop;
        public float PaddingBottom;
    }
}
