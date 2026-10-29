#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 设计令牌门面: 仓库里所有视觉值的唯一入口。
    ///
    /// 渲染器只能读 <c>RDTheme.Xxx</c>, 不允许写死任何像素/颜色。分两半:
    ///   - 运行时度量(跟随当前编辑器主题, 从基准 UI 系统现场量一次并缓存), 见 <see cref="Metrics"/>;
    ///   - 静态令牌(产品规格: 颜色、圆角、图标尺寸等, 不随主题变, 或只随 isProSkin 变)。
    ///
    /// 本类把原先散落在 <c>DrawStyles</c> / <c>DrawMetrics</c> / 两个 Driver 里的写死值收成一份,
    /// 后端( / )改为读同一个 <c>RDTheme</c>。Phase 1 只换取值来源, 不动调用结构。
    /// </summary>
    internal static class RDTheme
    {
        /// <summary>运行时度量的唯一来源( 现场量)。</summary>
        internal static IMetricsProvider Metrics => EditorMetricsProvider.Instance;

        //-------------------------------------- 运行时度量(委托 Metrics) --------------------------------------

        public static float RowSpacing => Metrics.RowSpacing;
        public static float FieldMinHeight => Metrics.FieldMinHeight;
        public static float ButtonHeight => Metrics.ButtonHeight;
        public static float BoxSpacing => Metrics.BoxSpacing;
        public static float BoxMargin => Metrics.BoxMargin;
        public static float ButtonMargin => Metrics.ButtonMargin;
        public static float MiniFontSize => Metrics.MiniFontSize;
        public static float ScrollbarWidth => Metrics.ScrollbarWidth;
        public static float ScrollbarHeight => Metrics.ScrollbarHeight;
        public static float ScrollViewInsets => Metrics.ScrollViewInsets;
        public static float BoxInsets => Metrics.BoxInsets;
        public static float ToolbarHeight => Metrics.ToolbarHeight;
        public static float ButtonTintGray => Metrics.ButtonTintGray;
        public static float PopupTintGray => Metrics.PopupTintGray;
        public static float TextFieldTintGray => Metrics.TextFieldTintGray;
        public static float LabelWidth => Metrics.LabelWidth;

        //-------------------------------------- 静态几何(产品规格, 不随主题变) --------------------------------------

        /// <summary>一级缩进的宽度 —— 原来的常量。</summary>
        public const float IndentWidth = 10f;

        /// <summary>标签列宽的兜底值(EditorGUIUtility.labelWidth 取不到时用)。</summary>
        public const float LabelWidthDefault = 150f;

        /// <summary>标签列宽的上限(避免被别处改过的 labelWidth 把面板拉歪)。</summary>
        public const float LabelWidthMax = 300f;

        /// <summary>输入区的文字左留白: 0 —— 字段紧贴标签列右边缘。</summary>
        public const float InputPaddingLeft = 0f;

        /// <summary>表头横条([BoxGroup] 的分节标题)的高度: 固定 20。</summary>
        public const float PanelHeaderHeight = 20f;

        /// <summary>表头横条上下边线的粗细(1px)。</summary>
        public const float PanelHeaderBorderThickness = 1f;

        /// <summary>表头横条上下内边距(文字与边线之间各 1px)。</summary>
        public const float PanelHeaderPaddingVertical = 1f;

        /// <summary>卡片左右内边距: 3px 是"值框到框线"的呼吸。</summary>
        public const float CardPadding = 3f;

        /// <summary>卡片顶部内边距(2f)。</summary>
        public const float CardPaddingTop = 2f;

        /// <summary>卡片底部内边距(3f)。</summary>
        public const float CardPaddingBottom = 3f;

        /// <summary>卡片边框的粗细(1px 线，border width，同一个值)。</summary>
        public const float CardBorderThickness = 1f;

        /// <summary>浮层卡片的圆角(仅自绘元素有圆角; 矩形卡片没有)。</summary>
        public const float GhostCornerRadius = 3f;

        /// <summary>表头下那条分隔线的粗细。</summary>
        public const float TableHeaderLineThickness = 1f;

        /// <summary>分组头 ([TitleGroup]) 横线的粗细。</summary>
        public const float TitleGroupLineThickness = 2f;

        /// <summary>空标签折叠头的宽度(像素): 只放那枚三角。</summary>
        public const float FoldoutArrowWidth = 14f;

        /// <summary>拖拽浮层卡片的高度: 一行字 + 上下各一点余量。</summary>
        public const float GhostHeight = 24f;

        /// <summary>列边界抓取区外扩量: 线只有 1px, 左右各外扩这么多(不占布局宽)。</summary>
        public const float ColumnResizeHitMargin = 2f;

        /// <summary>列边界抓取区的命中宽: 布局占 1px, 余下的靠负外边距外扩。</summary>
        public const float ColumnResizeHitWidth = 5f;

        /// <summary>按钮内图标尺寸。</summary>
        public const float ButtonIconSize = 16f;

        /// <summary>按钮内图标左缘留白。</summary>
        public const float ButtonIconLeft = 6f;

        /// <summary>带图标按钮的文字区左内边距(图标左缘 + 图标宽 + 4px 间隙)。</summary>
        public const float ButtonIconPadLeft = ButtonIconLeft + ButtonIconSize + 4f;

        //-------------------------------------- 颜色(随 isProSkin 变) --------------------------------------

        /// <summary>表头横条底色: 比面板底色亮一档(暗色主题)。</summary>
        public static Color PanelHeaderBackground => EditorGUIUtility.isProSkin
            ? new Color(0.26f, 0.26f, 0.26f, 1f)
            : new Color(0.76f, 0.76f, 0.76f, 1f);

        /// <summary>表头横条上下边线: 与表格网格线同一个"浅一档"的灰(暗色主题)。</summary>
        public static Color PanelHeaderBorder => EditorGUIUtility.isProSkin
            ? new Color(0.32f, 0.32f, 0.32f, 1f)
            : new Color(0.60f, 0.60f, 0.60f, 1f);

        /// <summary>卡片(列表条目)的底色: 比单元格亮一档。</summary>
        public static Color CardBackground => EditorGUIUtility.isProSkin
            ? new Color(0.20f, 0.20f, 0.20f, 1f)
            : new Color(0.82f, 0.82f, 0.82f, 1f);

        /// <summary>卡片边框: 与表格网格线同一档的浅灰。</summary>
        public static Color CardBorder => EditorGUIUtility.isProSkin
            ? new Color(0.24f, 0.24f, 0.24f, 1f)
            : new Color(0.66f, 0.66f, 0.66f, 1f);

        /// <summary>
        /// 表格行 / 表头下 / 列边界的分隔线颜色: 比底色亮一档。整套网格线只有这一个值。
        /// </summary>
        public static readonly Color TableHeaderLineColor = new(0.32f, 0.32f, 0.32f, 0.5f);

        /// <summary>列表条目之间 / 列表头下沿的分隔线颜色: 与表格网格线同一个值。</summary>
        public static readonly Color ListItemLineColor = TableHeaderLineColor;

        /// <summary>分组头 ([TitleGroup]) 横线的颜色。</summary>
        public static readonly Color TitleGroupLineColor = new(0.4f, 0.4f, 0.4f, 0.3f);

        /// <summary>拖拽排序的落点线: 2px 的蓝, 与 Unity 的选中/高亮同色系。</summary>
        public static readonly Color DropIndicatorColor = new(0.24f, 0.49f, 0.90f, 1f);

        /// <summary>多选页签"已选中"键的底色(近似编辑器选中色)。</summary>
        public static readonly Color ToolbarToggleOnColor = new(0.32f, 0.55f, 0.85f, 1f);

        //-------------------------------------- 控件 chrome(原先散落在旧实现的值, 全部上收) --------------------------------------

        /// <summary>紧凑按钮(定宽小键)的内边距: 现在写死的 (2,2,1,1)。返回新实例避免被 RectOffset 共享。</summary>
        public static RectOffset CompactButtonPadding => new(1, 1, 1, 1);

        /// <summary>紧凑按钮的外边距: 现在写死的 0。返回新实例避免被 RectOffset 共享。</summary>
        public static RectOffset CompactButtonMargin => new(0, 0, 0, 0);

        /// <summary>无底纹按钮悬停提亮系数: 现在写死的 0.5f。</summary>
        public const float FramelessHoverLerp = 0.5f;

        //-------------------------------------- 拖拽排序(浮层 + 源条目压暗) --------------------------------------

        /// <summary>浮层卡片的宽 = 源条目宽 × 这个比例, 再夹到 [<see cref="DragGhostMinWidth"/>, <see cref="DragGhostMaxWidth"/>]。</summary>
        public const float DragGhostWidthRatio = 0.45f;

        /// <summary>浮层卡片的最小宽: 太窄时一句摘要会被截成半个包名。</summary>
        public const float DragGhostMinWidth = 160f;

        /// <summary>浮层卡片的最大宽: 再宽就像一张独立的面板, 读者会以为它是列表的一部分。</summary>
        public const float DragGhostMaxWidth = 320f;

        /// <summary>浮层卡片相对指针的横向偏移(不压住指针)。</summary>
        public const float DragGhostPointerOffsetX = 14f;

        /// <summary>浮层卡片相对指针的纵向偏移。</summary>
        public const float DragGhostPointerOffsetY = 8f;

        /// <summary>浮层内文字的左右留白(后端同一份)。</summary>
        public const float DragGhostTextPadding = 7f;

        /// <summary>
        /// 被拖条目在**源位置**的压暗色(叠一层半透明黑)。
        /// 与 <see cref="DragSourceDimOverlay"/> 语义相同、手段不同: 本项用叠色,
        /// 彼项铺一条半透明黑元素, 两种叠加方式在不透明像素上得到的观感不同。
        /// </summary>
        public static Color DragSourceDim => EditorGUIUtility.isProSkin
            ? new Color(0.07f, 0.07f, 0.07f, 0.55f)
            : new Color(0.86f, 0.86f, 0.86f, 0.60f);

        /// <summary>源条目压暗遮罩(铺满条目的半透明黑元素)。</summary>
        public static readonly Color DragSourceDimOverlay = new(0f, 0f, 0f, 0.38f);

        //-------------------------------------- 自绘控件(Phase 4: 不用宿主主题) --------------------------------------

        /// <summary>复选框方框的边长( toggle 就是 14×14)。</summary>
        public const float CheckboxSize = 14f;

        /// <summary>复选框方框的边线粗细。</summary>
        public const float CheckboxBorderThickness = 1f;

        /// <summary>复选框方框的底色。</summary>
        public static Color CheckboxBackground => EditorGUIUtility.isProSkin
            ? new Color(0.22f, 0.22f, 0.22f, 1f)
            : new Color(0.85f, 0.85f, 0.85f, 1f);

        /// <summary>复选框方框的边线。</summary>
        public static Color CheckboxBorder => EditorGUIUtility.isProSkin
            ? new Color(0.36f, 0.36f, 0.36f, 1f)
            : new Color(0.62f, 0.62f, 0.62f, 1f);

        /// <summary>勾的颜色。</summary>
        public static Color CheckboxCheck => EditorGUIUtility.isProSkin
            ? new Color(0.82f, 0.82f, 0.82f, 1f)
            : new Color(0.25f, 0.25f, 0.25f, 1f);

        /// <summary>勾的字号(方框 14px 时 11px 刚好撑满又不顶边)。</summary>
        public const float CheckboxCheckFontSize = 11f;

        /// <summary>滑杆轨道的粗细。</summary>
        public const float SliderTrackHeight = 3f;

        /// <summary>滑杆滑块的边长(圆形, 取正方形边长)。</summary>
        public const float SliderThumbSize = 12f;

        /// <summary>滑杆轨道的颜色。</summary>
        public static Color SliderTrack => EditorGUIUtility.isProSkin
            ? new Color(0.23f, 0.23f, 0.23f, 1f)
            : new Color(0.72f, 0.72f, 0.72f, 1f);

        /// <summary>滑杆滑块的颜色。</summary>
        public static Color SliderThumb => EditorGUIUtility.isProSkin
            ? new Color(0.62f, 0.62f, 0.62f, 1f)
            : new Color(0.55f, 0.55f, 0.55f, 1f);

        /// <summary>滑杆滑块的边线。</summary>
        public static Color SliderThumbBorder => CheckboxBorder;

        //-------------------------------------- 文本/按钮规格 --------------------------------------

        /// <summary>集合/表格头的计数行: 一行加粗文字。</summary>
        public static readonly DrawTextSpec CountedHeader = DrawTextSpec.Of(bold: true);

        /// <summary>表格表头行文字: 居中、与正文同字号(靠横条底色区分)。</summary>
        public static readonly DrawTextSpec TableHeaderText =
            DrawTextSpec.Of(DrawAlign.Center, height: PanelHeaderHeight);

        /// <summary>列表头右侧的计数文字("12 items" / "Empty"): 次要信息, mini 灰字。</summary>
        public static readonly DrawTextSpec CountLabel = DrawTextSpec.Of(mini: true);

        /// <summary>列表 / 表格头部的添加按钮: 定宽 24、无底纹。</summary>
        public static readonly DrawButtonSpec AddButton = DrawButtonSpec.Of(width: 24).WithFrameless();

        /// <summary>分页器的上一页/下一页按钮: 定宽 14、无底纹。</summary>
        public static readonly DrawButtonSpec PagerButton = DrawButtonSpec.Of(width: 14).WithFrameless();

        /// <summary>列表头右侧的"展开全部 / 收起分页"键: 一枚 ▲/▼ 字形(无底纹)。</summary>
        public static readonly DrawButtonSpec ExpandButton = DrawButtonSpec.Of(width: 18).WithFrameless();

        /// <summary>列表条目右侧的删除键: 一枚 × 字形, 定宽 16、高 18、无底纹。</summary>
        public static readonly DrawButtonSpec ItemRemoveButton =
            DrawButtonSpec.Of(width: 16, height: 18).WithFrameless();
    }
}

#endif
