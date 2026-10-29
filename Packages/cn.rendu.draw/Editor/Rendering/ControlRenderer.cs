namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    ///  的自绘实现(Phase 4)。
    ///
    /// 分工: **哪些控件自绘**是策略, 归这里; **自绘出来的元素长什么样**归 <see cref="Driver"/>
    /// (元素只能在那儿创建, 调和器也归它管)。所以这个类是"接口线"真正被用起来的地方:
    /// 把注入的实现从 <see cref="DriverControlRenderer"/> 换成它, 自绘的那几类控件就换了一套画法,
    /// 树遍历 / RDTheme / LayoutPolicy / 产树方一概不动。
    ///
    /// 自绘 <c>Toggle</c> / <c>Slider</c> / <c>Toolbar</c> / <c>ToolbarMulti</c> —— 这几类的交互
    /// 完全在控件自己内部(点击、拖动), 自绘不会丢掉任何宿主能力; 而它们恰好是宿主主题差异最大的
    /// 地方(勾选框、滑杆、页签选中态)。
    ///
    /// 其余控件都依赖宿主: 文本编辑与 IME(TextField/Integer/Float)、下拉浮层(Popup)、
    /// 位组合菜单(EnumFlags)、对象选择(ObjectField/StructField), 自绘它们等于重写输入法与浮层,
    /// 不在"有界"的范围内 —— 它们继续交给原生控件。
    ///
    /// 兜底: 自绘开关关掉时, 一律退回 <see cref="DriverControlRenderer"/>。
    /// </summary>
    internal sealed class ControlRenderer : IControlRenderer
    {
        private readonly DriverControlRenderer m_Native = new();

        // 认领范围与原生实现一致: 一个控件必须在链上有人认领, 否则解释器只会画它的子树(等于消失)
        public bool Supports(ControlWidget widget) => m_Native.Supports(widget);

        public void Render(ControlWidget widget, RenderContext ctx)
        {
            if (Driver.UseSelfDrawnControls && ctx.Ctx.Driver is Driver backend)
            {
                switch (widget)
                {
                    case ToggleWidget toggle:
                        toggle.Value = backend.PaintedToggleField(toggle.Label, toggle.Value, toggle.Width);
                        return;

                    case SliderWidget slider:
                        slider.Value = backend.PaintedSliderField(slider.Label, slider.Value, slider.Min, slider.Max, slider.Width);
                        return;

                    case ToolbarWidget toolbar:
                        toolbar.Selected = backend.PaintedToolbarField(toolbar.Options, toolbar.Selected, toolbar.Width);
                        return;

                    case ToolbarMultiWidget multi:
                        multi.ClickedIndex = backend.PaintedMultiToolbarField(multi.Options, multi.States, multi.Width);
                        return;
                }
            }

            m_Native.Render(widget, ctx);
        }
    }
}
