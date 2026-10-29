using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// [InlineEditor]: 不做引用选择器, 直接把值的完整检视界面就地展开。
    ///
    /// 它排在链首 —— 特性显式要求"就地展开", 压过一切按形态的默认处理。
    /// </summary>
    internal sealed class InlineEditorRenderer : MemberRendererBase
    {
        private readonly IMemberDrawHost m_Drawer;

        public InlineEditorRenderer(DrawContext ctx, WidgetBuilder builder, ValueAccessor values,
                                    ILabelFormatter labels, ITypeShape shape, IMemberDrawHost drawer)
            : base(ctx, builder, values, labels, shape)
        {
            m_Drawer = drawer;
        }

        public override bool CanDraw(in MemberDrawArgs args)
        {
            return args.Value.Member.GetAttr<InlineEditorAttribute>() != null
                && Shape.CanInlineEdit(args.Value.Type);
        }

        public override void Draw(in MemberDrawArgs args)
        {
            var target = args.Value.Target;
            var member = args.Value.Member;
            var host   = args.Host;
            var depth  = args.Depth;

            // 主流程已经读过这个成员: 这里再接一次反射 getter 只会多一次装箱。
            var value = args.Value.Value;
            if (value == null)
            {
                Builder.KeyValueLabel(Labels.LabelOrEmpty(member), "(null)");
                return;
            }

            // [HideLabel] 时不画表头, 完全内联
            if (member.GetAttr<HideLabelAttribute>() == null)
            {
                // 默认展开: 折叠状态只在用户显式收起后才存在(读/画/写回见 CollapseState)。
                var expanded = CollapseState.Build(Ctx, target, member.RawName, true, Labels.GetLabel(member), Builder);
                if (expanded == false) return;
            }

            // Unity 对象(MonoBehaviour/ScriptableObject)默认不会被内窥, 这里显式放开。
            if (value is UnityEngine.Object unityObject)
            {
                if (unityObject == null) return; // 已销毁的对象
                m_Drawer.DrawNested(unityObject, host, depth + 1, target, true);
                return;
            }

            m_Drawer.DrawNested(value, host, depth + 1, target, false);
        }

    }
}
