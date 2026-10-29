#if UNITY_EDITOR

namespace RD.Draw.Editor
{
    /// <summary>
    /// 运行时度量源: 从编辑器默认控件现场量一次并缓存(量不到就钉兜底常量)。
    ///
    /// 只有一个实现(<see cref="EditorMetricsProvider"/>): 读编辑器控件的既定规格, 也就是原先
    /// <c>DrawMetrics</c> 里的量测逻辑。渲染后端本身就是保留模式(UIElements), 这些度量直接取自它的控件规格,
    /// 不存在另一套需要跟随的体系。
    /// </summary>
    internal interface IMetricsProvider
    {
        /// <summary>行与行之间的空隙。</summary>
        float RowSpacing { get; }

        /// <summary>单行字段的高度。</summary>
        float FieldMinHeight { get; }

        /// <summary>按钮高度 —— 按按钮实际高度锁高, 不反过来给塞值。</summary>
        float ButtonHeight { get; }

        /// <summary>box 分组的内边距。</summary>
        float BoxSpacing { get; }

        /// <summary>box 分组与外界的空隙。</summary>
        float BoxMargin { get; }

        /// <summary>
        /// 按钮的外边距。
        /// 相邻按钮之间的缝是"上一条的下边距 + 下一条的上边距"拼出来的, 也要照抄。
        /// </summary>
        float ButtonMargin { get; }

        /// <summary>mini 文本的字号。</summary>
        float MiniFontSize { get; }

        /// <summary>竖向滚动条的宽度(滚动区让位、列宽反算都要用)。</summary>
        float ScrollbarWidth { get; }

        /// <summary>横向滚动条的高度(表格高度公式要把它算进去)。</summary>
        float ScrollbarHeight { get; }

        /// <summary>滚动区横向 inset: 滚动区左右 padding + 竖向滚动条宽。</summary>
        float ScrollViewInsets { get; }

        /// <summary>分组盒左右 padding 之和。</summary>
        float BoxInsets { get; }

        /// <summary>工具栏的高度 —— 工具条里的分段按钮按它锁高。</summary>
        float ToolbarHeight { get; }

        /// <summary>
        /// 按钮底纹灰度。控件底纹是乘在纹理上的, 铺纯色时必须乘同一个灰度才同色。
        /// </summary>
        float ButtonTintGray { get; }

        /// <summary>下拉框/开关类控件的底纹灰度。</summary>
        float PopupTintGray { get; }

        /// <summary>文本/数字输入框的底纹灰度(比 popup 深一截)。</summary>
        float TextFieldTintGray { get; }

        /// <summary>
        /// 标签列宽 —— 直接用 EditorGUIUtility.labelWidth(编辑器标签列宽偏好, 检视器同样遵循)。
        /// 这一项**不缓存**: 用户随时可能拖标签列宽, 缓存等于"改了设置要等重编译"。
        /// </summary>
        float LabelWidth { get; }
    }
}

#endif
