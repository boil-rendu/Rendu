using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// [Button] 方法的绘制: 按钮本身的规格(高度/宽度/图标/背景色)与点击后的调用。
    ///
    /// 它**不是渲染器**, 而是被编排者直接调用的一项绘制能力: 按钮不是值成员, 是"按成员种类
    /// 分发"里的一条独立分支。叫 Painter 而不是 Renderer, 是为了和"渲染一段结构"的那些类
    /// 区分开(命名约定见 <see cref="ValueMemberRenderer"/> 的类注释)。
    ///
    /// 单独成类之后, 按钮的宽度语义 —— 那套"表格单元格里的按钮怎么分宽度"的规则 ——
    /// 只存在于这一处。
    /// </summary>
    internal sealed class ButtonPainter
    {
        private readonly DrawContext m_Ctx;
        private readonly WidgetBuilder m_Builder;
        private readonly ValueAccessor m_Values;
        private readonly ILabelFormatter m_Labels;

        public ButtonPainter(DrawContext ctx, WidgetBuilder builder, ValueAccessor values, ILabelFormatter labels)
        {
            m_Ctx     = ctx;
            m_Builder = builder;
            m_Values  = values;
            m_Labels  = labels;
        }

        /// <summary>画一个按钮成员并在被点击时调用它。</summary>
        public void Paint(object target, CachedMember member)
        {
            var attr  = member.GetAttr<ButtonAttribute>();
            var label = attr?.ButtonName;
            if (string.IsNullOrEmpty(label)) label = m_Labels.FriendlyName(member.RawName);

            // 约定: 带高度的按钮([Button("同步生成", 50)])= 占满行宽的高按钮;
            // 不带高度的按钮([Button("↓大")])= 文字宽的紧凑键, ButtonGroup 里多个能排进同一行。
            var hasHeight = attr != null && attr.ButtonHeight > 0;
            var height    = hasHeight ? attr.ButtonHeight : DefaultButtonHeight;

            // 表格单元格上下文(见 DrawContext.CellButtonShare):
            //   >0 = 按钮组内平分的共享宽(6 个更新键一排, 各占 1/6 列宽, 不会把均分列撑出滚动条);
            //   <0 = 独立按钮, 撑满所在列宽(如"重置"按钮的观感);
            //   =0 = 不在单元格内, 紧凑自然宽。
            var width  = 0f;
            var expand = hasHeight;
            if (hasHeight == false && m_Ctx.CellButtonShare != 0f)
            {
                if (m_Ctx.CellButtonShare > 0f)
                {
                    width  = m_Ctx.CellButtonShare;
                    expand = false;
                }
                else
                {
                    expand = true;
                }
            }
            else if (hasHeight == false && m_Ctx.InTableCell)
            {
                // 单元格里的普通按钮(如依赖条目里的"同步"): 约定 = 撑满所在格宽,
                // 而不是一个小按钮缩在左边("同步"这类按钮就是通栏的)
                expand = true;
            }

            var spec = DrawButtonSpec.Of(height: height,
                                          width: width,
                                          expandWidth: expand,
                                          tooltip: string.IsNullOrEmpty(attr?.Tooltip) ? null : attr.Tooltip,
                                          // [历史] 注意不能写 attr?.Icon != SdfIconType.None: 可空比较里 null 也"不等于 None",
                                          // 于是 attr 为空时会走进真分支, 下一步取 attr.Icon 就空引用了。
                                          icon: attr != null && attr.Icon != SdfIconType.None
                                              ? EditorIcons.FromSdf(attr.Icon)
                                              : null);

            // GUIColor 只作用于本次按钮: 由后端负责覆盖并在结束后还原(异常也不会污染整帧)。
            var color = m_Labels.ResolveGUIColor(member);
            if (color != null) spec = spec.WithBackground(color.Value);

            m_Builder.Button(label, spec, () =>
            {
                m_Ctx.Mutate(target, () => m_Values.InvokeSafe(target, member.Info));
            });
        }

        /// <summary>[Button] 不指定高度时的默认按钮高度 —— 取按钮的实际高度(锁定, 不反塞)。</summary>
        private static float DefaultButtonHeight => RDTheme.ButtonHeight;
    }
}
