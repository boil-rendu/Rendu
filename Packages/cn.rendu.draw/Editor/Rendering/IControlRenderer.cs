namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 原生控件的契约边界: 把"某个语义控件怎么渲染"这个差异收进一个可插拔实现。
    ///
    /// 今天后端都注入 <see cref="DriverControlRenderer"/> —— 它把控件翻译成 <see cref="Driver"/>
    /// 原语, 后端差异(立即模式 / 保留模式 / 宿主主题)本来就收在 Driver 里。
    ///
    /// 将来要像素级对齐观感, 只需换一个自绘实现(自己画字段/下拉/折叠),
    /// 在 <c>Renderer</c> 的构造里换一个注入即可 —— 树遍历、RDTheme、LayoutPolicy 全部不动。
    /// </summary>
    internal interface IControlRenderer
    {
        /// <summary>该实现能否渲染这个控件(按控件类型 + 可选能力判别)。</summary>
        bool Supports(ControlWidget widget);

        /// <summary>渲染一个控件并回填交互结果(新值写回节点字段 / 点击写回 ctx)。</summary>
        void Render(ControlWidget widget, RenderContext ctx);
    }

    /// <summary>
    /// 默认实现: 控件 → <see cref="Driver"/> 原语。
    ///
    /// 它是"原生控件怎么画"的唯一一份代码 —— 之前这段 <c>switch</c> 长在旧  解释器里,
    /// 而 <c>Renderer</c> 只能整体转发; 抽到接口实现之后, 换后端 = 换一个实现, 不再复制翻译逻辑。
    ///
    /// 交互结果只写回节点字段(新值/选中下标/点击的按钮 Key), 数据写回一律由 <c>ChangeApplier</c> 做 ——
    /// 解释器不碰成员, 也就不会在渲染途中改数据。
    /// </summary>
    internal sealed class DriverControlRenderer : IControlRenderer
    {
        public bool Supports(ControlWidget widget)
        {
            switch (widget)
            {
                case ButtonWidget:
                case FoldoutWidget:
                case TextFieldWidget:
                case IntegerFieldWidget:
                case FloatFieldWidget:
                case SliderWidget:
                case ToggleWidget:
                case PopupWidget:
                case EnumFlagsWidget:
                case ToolbarWidget:
                case ToolbarMultiWidget:
                case ObjectFieldWidget:
                case StructFieldWidget:
                    return true;
                default:
                    return false;
            }
        }

        public void Render(ControlWidget widget, RenderContext ctx)
        {
            var drv = ctx.Ctx.Driver;

            switch (widget)
            {
                case ButtonWidget b:
                    if (drv.Button(b.Text, b.Spec)) ctx.ClickedButtonKeys.Add(b.Key);
                    break;

                case FoldoutWidget f:
                    var expanded = f.Expanded;
                    var next     = drv.Foldout(expanded, f.Label);
                    f.Expanded   = next;
                    // 保留模式不会自动重画: 折叠态一变就得主动请求, 否则子树不增不减。
                    if (next != expanded) drv.RequestRepaint();
                    break;

                case TextFieldWidget tf:
                    tf.Value = drv.TextField(tf.Label, tf.Value, tf.Width);
                    break;

                case IntegerFieldWidget i:
                    i.Value = drv.IntegerField(i.Label, i.Value, i.Width);
                    break;

                case FloatFieldWidget fl:
                    fl.Value = drv.FloatField(fl.Label, fl.Value, fl.Width);
                    break;

                case SliderWidget sl:
                    sl.Value = drv.Slider(sl.Label, sl.Value, sl.Min, sl.Max, sl.Width);
                    break;

                case ToggleWidget tg:
                    tg.Value = drv.ToggleField(tg.Label, tg.Value, tg.Width);
                    break;

                case PopupWidget pp:
                    pp.Selected = drv.Popup(pp.Label, pp.Selected, pp.Options, pp.Width);
                    break;

                case EnumFlagsWidget ef:
                    ef.Value = drv.EnumFlagsField(ef.Label, ef.Value, ef.Width);
                    break;

                case ToolbarWidget tb:
                    tb.Selected = drv.Toolbar(tb.Selected, tb.Options, tb.Width);
                    break;

                case ToolbarMultiWidget tm:
                    tm.ClickedIndex = drv.ToolbarMulti(tm.Options, tm.States, tm.Width);
                    break;

                case ObjectFieldWidget of:
                    of.Value = drv.ObjectField(of.Label, of.Value, of.Type, of.Width);
                    break;

                case StructFieldWidget sf:
                    sf.Value = drv.StructField(sf.Label, sf.Value, sf.Type, sf.Width);
                    break;
            }
        }
    }
}
