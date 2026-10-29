using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// Unity 复合值类型: 曲线/渐变, 以及 Vector*/Rect/Bounds/LayerMask 等结构体。
    ///
    /// 曲线与渐变单列一档是因为它们是**引用类型**且值为 null 时合法 ——
    /// 后端会在自己那一侧代以空曲线/空渐变, 不能按"值缺失"处理。
    /// </summary>
    internal sealed class CompositeValueControl : ValueControlBase
    {
        private readonly ITypeShape m_Shape;

        public CompositeValueControl(WidgetBuilder builder, ITypeShape shape) : base(builder)
        {
            m_Shape = shape;
        }

        public override bool CanHandle(in ValueDrawArgs args)
        {
            return m_Shape.IsUnityValueBag(args.Type) || m_Shape.HasStructFieldControl(args.Type);
        }

        public override void Build(in ValueDrawArgs args)
        {
            Builder.StructField(args.Target, args.Member, args.Label, args.Value, args.Type, 0f);
        }
    }
}
