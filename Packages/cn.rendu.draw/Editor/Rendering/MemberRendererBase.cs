using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 成员渲染链上各档的共享骨架: 上下文 + 取值 + 标签 + 形态分类。
    ///
    /// 每一档只回答两件事: "我什么时候接管"(<see cref="CanDraw"/>) 与 "怎么画"
    /// (<see cref="Draw"/>)。两者都要用到的公共依赖(后端、会话、取值器、标签、形态)
    /// 由本基类统一持有, 免得每个档各写一遍构造与字段。
    ///
    /// 依赖是**显式注入**的: 各档只拿自己用得上的那几个(由子类构造函数挑选),
    /// 不出现"为了将来可能用到"的多余依赖(接口隔离)。
    /// </summary>
    internal abstract class MemberRendererBase : IMemberRenderer
    {
        /// <summary>本次绘制的上下文(后端、会话、诊断、单元格作用域)。</summary>
        protected readonly DrawContext Ctx;

        /// <summary>本次绘制的建树器(产节点 + 记录变更)。</summary>
        protected readonly WidgetBuilder Builder;

        /// <summary>值的读写与成员查找。</summary>
        protected readonly ValueAccessor Values;

        /// <summary>标签与文案规则。</summary>
        protected readonly ILabelFormatter Labels;

        /// <summary>类型形态分类。</summary>
        protected readonly ITypeShape Shape;

        protected MemberRendererBase(DrawContext ctx, WidgetBuilder builder, ValueAccessor values,
                                     ILabelFormatter labels, ITypeShape shape)
        {
            Ctx     = ctx;
            Builder = builder;
            Values  = values;
            Labels  = labels;
            Shape   = shape;
        }

        /// <summary>后端句柄(画所有东西都经由它)。</summary>
        protected Driver Drv => Ctx.Driver;

        /// <summary>外观/交互状态栈([Color]/[ReadOnly]/[LabelWidth]); 经 Driver 直接访问。</summary>
        protected Driver Stk => Ctx.Driver;

        /// <summary>本次绘制的状态会话(折叠/滚动/分页)。</summary>
        protected HostSession Session => Ctx.Session;

        /// <summary>这一档是否接管该成员(链上第一个返回 true 的获胜)。</summary>
        public abstract bool CanDraw(in MemberDrawArgs args);

        /// <summary>画出这个成员。</summary>
        public abstract void Draw(in MemberDrawArgs args);
    }
}
