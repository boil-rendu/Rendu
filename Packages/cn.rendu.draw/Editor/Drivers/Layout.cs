using UnityEngine.UIElements;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 后端的**布局语义**入口: 引擎说的是"占满宽 / 占满高 / 定宽定高",
    /// 由这里翻译成 flexGrow / alignSelf, 各控件不再自己判断父容器方向。
    ///
    /// 为什么必须收在一处: flexGrow 是**主轴**增长 —— 横排里是宽, 竖排里是**高**。
    /// 表格单元格的高度由同行最高的那一列决定, 单元格里多出来的高度会被所有 flexGrow 子元素
    /// 分摊掉: 字段凭空高十几像素、按钮被拉长、行与行之间多出空档
    /// (布局密度的差异就是这么攒出来的)。
    ///
    /// 密度/布局判断只允许经过这里 —— 新增控件也只调这三个方法, 不要在别处直接写 flexGrow。
    /// </summary>
    internal static class Layout
    {
        /// <summary>
        /// 元素是否落在横排容器里。先看父容器的内联样式(各 Begin* 都显式写方向, 第一帧就可靠),
        /// 内联为空时退回已结算样式 —— 主题自带方向的容器(Toolbar 之类)只能靠后者判。
        /// </summary>
        internal static bool InRow(VisualElement element)
        {
            var parent = element.parent;
            if (parent == null) return false;

            var inline = parent.style.flexDirection;
            if (inline == FlexDirection.Row) return true;
            if (inline == FlexDirection.Column) return false;

            return parent.resolvedStyle.flexDirection == FlexDirection.Row;
        }

        /// <summary>占满可用宽: 横排里靠 flexGrow(主轴), 竖排里是交叉轴, 用 alignSelf=Stretch。</summary>
        internal static void FillWidth(VisualElement element)
        {
            var inRow = InRow(element);
            element.style.flexGrow  = inRow ? 1f : 0f;
            element.style.alignSelf = inRow ? StyleKeyword.Null : Align.Stretch;
        }

        /// <summary>占满可用高: 竖排里靠 flexGrow(主轴), 横排里是交叉轴, 用 alignSelf=Stretch。</summary>
        internal static void FillHeight(VisualElement element)
        {
            var inRow = InRow(element);
            element.style.flexGrow  = inRow ? 0f : 1f;
            element.style.alignSelf = inRow ? Align.Stretch : StyleKeyword.Null;
        }

        /// <summary>不参与主轴拉伸: 定宽/定高的控件用它, 免得在竖排里被拉高。</summary>
        internal static void Fixed(VisualElement element)
        {
            element.style.flexGrow  = 0f;
            element.style.alignSelf = Align.FlexStart;
        }
    }
}
