using System.Collections;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 集合 / 嵌套对象: 展开成子成员, 带增删改。
    ///
    /// 它排在第二位 —— 集合/嵌套对象没有"单值控件"可言, 必须展开。
    /// 具体怎么展开(列表 / 字典 / 字符串集合 / 嵌套对象)交给
    /// <see cref="CollectionRenderer"/>, 本档只做"分流"。
    /// </summary>
    internal sealed class ContainerRenderer : MemberRendererBase
    {
        private readonly CollectionRenderer m_Collections;
        private readonly IMemberDrawHost m_Drawer;

        public ContainerRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values,
                                 ILabelFormatter labels, ITypeShape shape, CollectionRenderer collections,
                                 IMemberDrawHost drawer)
            : base(ctx, builder, values, labels, shape)
        {
            m_Collections = collections;
            m_Drawer      = drawer;
        }

        public override bool CanDraw(in MemberDrawArgs args)
        {
            return Shape.IsContainer(args.Value.Type);
        }

        public override void Draw(in MemberDrawArgs args)
        {
            var target = args.Value.Target;
            var member = args.Value.Member;
            var host   = args.Host;
            var depth  = args.Depth;

            // 主流程已经读过这个成员; 这里再读一次等于每帧多跑一遍反射 getter。
            var value = args.Value.Value;
            if (value == null)
            {
                Builder.KeyValueLabel(Labels.LabelOrEmpty(member), "(null)");
                return;
            }

            // 运行时形态分流: CanDraw 只看声明类型(能不能展开), 具体展成列表/字典/字符串集合
            // 要看值实际实现了哪个接口 —— 声明类型是 IEnumerable 时无从区分, 这不是"看值挑控件"。
            if (value is IDictionary dict)
            {
                m_Collections.RenderDictionary(target, member, dict);
                return;
            }

            if (value is IList list)
            {
                m_Collections.RenderList(target, member, list, host, depth);
                return;
            }

            if (Shape.IsContainer(value.GetType()) && value is IEnumerable)
            {
                m_Collections.RenderStringSet(target, member, (IEnumerable)value);
                return;
            }

            // 嵌套用户对象
            if (Labels.ShouldDrawLabel(member))
            {
                Builder.Label(Labels.GetLabel(member), RDTheme.CountedHeader);
            }

            m_Drawer.DrawNested(value, host, depth + 1, target, false);
        }
    }
}
