#if UNITY_EDITOR

namespace RD.Draw.Editor
{
    /// <summary>
    /// 布局策略: 每个控件族的布局规则只有这一份(纯函数)。
    ///
    /// 它只产出中立 <see cref="LayoutHint"/>、不做任何宿主 API 调用 —— 布局决策因此可单测、
    /// 可被复用, 而不是"两份长得像的代码"。翻译路径只做"LayoutHint → 本地 API"的
    /// 机械翻译（把 <see cref="LayoutHint"/> 翻成具体的布局 API）。
    ///
    /// 原先这些规则散落在 Driver 的 FieldOptions / ButtonOptions / Label 分支 / BeginVertical
    /// 与 ApplyLabelAndWidth / Button / ApplyTextSpec / BeginVertical 里, 各写一遍;
    /// 现在只有这一份。
    /// </summary>
    internal static class LayoutPolicy
    {
        /// <summary>
        /// 字段族(输入框/下拉/滑条等)的宽度规则:
        ///   - width &gt; 0 → 定宽(锁死 min, 不许收缩);
        ///   - width &lt;= 0 → 撑满行内剩余宽, 且 min-width=0 允许收缩(防长值把列顶宽)。
        /// </summary>
        public static LayoutHint ForField(float width)
        {
            return width > 0f
                ? new LayoutHint { Width = width, MinWidth = width }
                : new LayoutHint { FillWidth = true, MinWidth = 0f };
        }

        /// <summary>
        /// 按钮的布局规则:
        ///   - 宽度: 定宽(spec.Width &gt; 0) / 撑满(ExpandWidth 且未定宽) / 自然宽;
        ///   - 高度: 显式定高(Height &gt; 0) / 缺省(Height = 0, 翻译层各自用 ButtonHeight);
        ///   - 外边距: 无底纹或定宽 → 0, 否则 ButtonMargin;
        ///   - 内边距: 定宽紧凑键(非无底纹) → CompactButtonPadding。
        /// </summary>
        public static LayoutHint ForButton(in DrawButtonSpec spec)
        {
            var margin = spec.Frameless || spec.Width > 0f ? 0f : RDTheme.ButtonMargin;

            var hint = new LayoutHint
            {
                FillWidth = spec.ExpandWidth && spec.Width <= 0f,
                Width     = spec.Width > 0f ? spec.Width : 0f,
                Height    = spec.Height,   // 0 = 缺省, 翻译层各自用 ButtonHeight / skin 自然高

                MarginLeft   = margin,
                MarginRight  = margin,
                MarginTop    = margin,
                MarginBottom = margin,
            };

            // 定宽紧凑键(集合头的 + / 翻页箭头 / 条目 × 之外的定宽小键): 内边距收成 (2,2,1,1)
            if (spec.Width > 0f && spec.Frameless == false)
            {
                var pad = RDTheme.CompactButtonPadding;
                hint.PaddingLeft   = pad.left;
                hint.PaddingRight  = pad.right;
                hint.PaddingTop    = pad.top;
                hint.PaddingBottom = pad.bottom;
            }

            return hint;
        }

        /// <summary>
        /// 文本的布局规则:
        ///   - MaxWidth &gt; 0 → 上限宽(min=0, max=w), 非定宽;
        ///   - Width &gt; 0 → 定宽;
        ///   - 两者都为 0 → 自然宽并占满行(stretchWidth);
        ///   - Height &gt; 0 → 定高。
        /// </summary>
        public static LayoutHint ForLabel(in DrawTextSpec spec)
        {
            var hint = new LayoutHint
            {
                Width    = spec.Width > 0f ? spec.Width : 0f,
                Height   = spec.Height > 0f ? spec.Height : 0f,
                MaxWidth = spec.MaxWidth > 0f ? spec.MaxWidth : 0f,
            };

            // 上限宽: min=0 允许收缩, 不把外层容器的最小宽撑大
            if (spec.MaxWidth > 0f) hint.MinWidth = 0f;

            // 未定宽时占满行(stretchWidth / FillWidth)
            if (spec.Width <= 0f) hint.FillWidth = true;

            return hint;
        }

        /// <summary>
        /// 分组盒的布局规则:
        ///   - width &gt; 0 → Width + MaxWidth 双锁(只写 width 会被子控件最小宽撑大);
        ///   - expandWidth / expandHeight → 撑满;
        ///   - boxed → 纵向外边距 BoxMargin + 内边距 BoxSpacing(横向外边距 0, 面板边缘不留空白带)。
        /// </summary>
        public static LayoutHint ForBox(bool boxed, float width, bool expandHeight, bool expandWidth)
        {
            var hint = new LayoutHint
            {
                FillHeight = expandHeight,
                FillWidth  = expandWidth,
            };

            if (width > 0f)
            {
                hint.Width    = width;
                hint.MaxWidth = width;   // 双锁: 防子控件最小宽撑大
            }

            if (boxed)
            {
                hint.MarginTop    = RDTheme.BoxMargin;
                hint.MarginBottom = RDTheme.BoxMargin;
                hint.PaddingLeft   = RDTheme.BoxSpacing;
                hint.PaddingRight  = RDTheme.BoxSpacing;
                hint.PaddingTop    = RDTheme.BoxSpacing;
                hint.PaddingBottom = RDTheme.BoxSpacing;
            }

            return hint;
        }

        /// <summary>
        /// 工具栏/分段页签里每一枚项的布局规则: 平分整行(ExpandWidth / FillWidth)。
        /// 扁高(ToolbarHeight)、直角、项紧贴、居中这些几何归翻译层(值已由 RDTheme 单源)。
        /// </summary>
        public static LayoutHint ForToolbarItem()
        {
            return new LayoutHint { FillWidth = true };
        }
    }
}

#endif
