using System.Collections.Generic;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 一次渲染的上下文: 底层 <see cref="DrawContext"/>(Driver / 会话 / 变更回调 / Mutate / 单元格作用域)
    /// + 交互结果回填。
    ///
    /// 与 <see cref="DrawContext"/> 的分工: <see cref="DrawContext"/> 是"产树 + 解释"共用的底层上下文;
    /// 本类是"解释器视角"的窄视图, 并额外承载"渲染器只负责填、产树方负责消费"的交互结果。
    ///
    /// 交互结果分两类:
    ///   - 字段新值 / 折叠态 / 选中项 —— 直接写回控件节点的字段(Value / Expanded / Selected);
    ///   - 按钮点击 —— 写回 <see cref="ClickedButtonKeys"/>(引擎给的稳定 Key), 由产树方匹配动作。
    /// </summary>
    internal sealed class RenderContext
    {
        public RenderContext(DrawContext ctx)
        {
            Ctx = ctx;
        }

        /// <summary>底层绘制上下文(Driver / Session / 回调 / Mutate / 单元格作用域)。</summary>
        public DrawContext Ctx { get; }

        /// <summary>本帧被点击的按钮 Key(渲染器填, 产树方消费后清空)。</summary>
        public List<int> ClickedButtonKeys { get; } = new();
    }
}
