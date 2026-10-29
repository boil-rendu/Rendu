using System;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 分页器: "◀ [当前页] / 总页数 ▶"。
    ///
    /// 列表与表格都要用它, 所以独立成类 —— 翻页规则(持久化页面态 + 请求重绘)只有一份。
    ///
    /// 它不自带 BeginHorizontal/FlexibleSpace, 便于内联进列表头那一行。
    /// </summary>
    internal sealed class ListPager
    {
        private readonly DrawContext m_Ctx;

        public ListPager(DrawContext ctx)
        {
            m_Ctx = ctx;
        }

        /// <summary>
        /// 产一段分页器(不自带行容器, 由调用方内联进横排)。
        ///
        /// 翻页是"渲染后"才发生的动作: 三个控件都只产节点, 目标页在动作里算、在动作里落库,
        /// 本帧的 <paramref name="pageIndex"/> 只用来决定箭头要不要翻。
        /// </summary>
        public void Build(WidgetBuilder builder, object owner, string name, int pageIndex, int pageCount)
        {
            // 三件控件按"单行控件高度"钉齐: 页码框本身就是单行高,
            // 箭头与 "/ 总数" 是文字/字形, 不钉高度就会各自贴着自己那条边 —— 一枚高、一枚低,
            // 排在页码框两侧就是"七上八下"。
            var rowHeight = m_Ctx.Driver.LineHeight;
            var button    = RDTheme.PagerButton.WithHeight(rowHeight);

            builder.Button("◀", button, () =>
            {
                if (pageIndex > 0) GoTo(owner, name, pageIndex - 1);
            });

            // 当前页是一个可输入的窄数字框(参考界面就是 "◀ 2 / 2 ▶"): 页数一多, 连点箭头翻十几页
            // 不如直接输页码。标签宽压到 0 —— 否则  会把 labelWidth 那一段(默认 150px)
            // 从这 22px 里扣掉, 数字框只剩一条缝。
            builder.PushLabelWidth(0f);
            builder.IntegerField("", pageIndex + 1L, PageNumberWidth, typed =>
            {
                // 越界的输入当这帧没输过(不夹、不写回): 用户想输 "12" 时先落的那个 "1" 会被夹成
                // 合法值再把光标顶掉, 越输越乱; 越界时保持当前页, 箭头照旧可用。
                if (typed >= 1 && typed <= pageCount && typed - 1 != pageIndex)
                {
                    GoTo(owner, name, (int)typed - 1);
                }
            });
            builder.PopScope();

            // 斜杠后面只留一个空格: 数字框自带左右边距, 再补空格会把 "/ 3" 推成独立的一块
            builder.Label($"/ {pageCount}", DrawTextSpec.Of(mini: true, height: rowHeight));

            builder.Button("▶", button, () =>
            {
                if (pageIndex < pageCount - 1) GoTo(owner, name, pageIndex + 1);
            });
        }

        /// <summary>换页: 存会话 + 排重绘, 两件事只有这一份。</summary>
        private void GoTo(object owner, string name, int target)
        {
            m_Ctx.Session.SetPage(owner, name, target);
            m_Ctx.Driver.RequestRepaint();
        }

        /// <summary>页码框的宽: 刚好装下两位页码 —— 再宽就成了一只显眼的空盒子, 抢过计数。</summary>
        private const float PageNumberWidth = 22f;
    }
}
