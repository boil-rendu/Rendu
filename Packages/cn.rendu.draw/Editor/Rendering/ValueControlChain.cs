using System.Collections.Generic;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 值画法的注册表: 顺序即优先级, 第一个 CanHandle 为真的获胜, 全都不接管时用兜底画法。
    ///
    /// 它是"哪种类型用哪个控件"这条知识的**唯一**归属地: 读这一段就知道全部优先级, 加类型只改这一段。
    ///
    /// 注册表随每次绘制新建(建树器也是), 因此可以安全地持有 builder 引用;
    /// 枚举缓存与取值器是进程级服务, 从 EngineServices 注入后跨帧复用。
    /// </summary>
    internal sealed class ValueControlChain : DrawChain<IValueControl, ValueDrawArgs, Nothing>
    {
        private readonly IValueControl m_Fallback;

        public ValueControlChain(WidgetBuilder builder, DrawContext ctx, EnumCatalog enums, ValueAccessor values,
                                 ITypeShape shape)
            : base(BuildChain(builder, ctx, enums, values, shape))
        {
            // 兜底不进列表: 它没有"能否处理"的判断, 由这里在全部落空时显式调用。
            // 剩下的类型(char 等)走文本框, 编辑时靠 Convert.ChangeType 改回原类型。
            m_Fallback = new TextValueControl(builder, ctx, values);
        }

        /// <summary>
        /// 建链: **顺序即优先级**, 逐档都有理由(改动前请读):
        ///   1. [ValueDropdown] 属性显式指定下拉 —— 压过任何按类型的默认画法;
        ///   2. 枚举 —— 必须早于数值档(TypeCode 判不出枚举的底类型);
        ///   3. bool;
        ///   4. 字符串(带 [FilePath]/[FolderPath] 的路径选择按钮);
        ///   5. Unity 复合值(曲线/渐变/Vector*/Rect/Bounds/LayerMask);
        ///   6. 整型;
        ///   7. 浮点;
        ///   8. Unity 对象引用 —— 单独一档: 它是引用而非值, 走的是对象选择器。
        /// 5/6/7 与 8 互斥(复合值与数值都不是 UnityEngine.Object), 顺序调整在当前
        /// 实现下等价; 但语义上"引用选择器"仍是最特殊的一档, 放最后读起来最顺。
        /// </summary>
        private static IValueControl[] BuildChain(WidgetBuilder builder, DrawContext ctx, EnumCatalog enums,
                                                  ValueAccessor values, ITypeShape shape)
        {
            return new IValueControl[]
            {
                new DropdownValueControl(builder, values),
                new EnumValueControl(builder, enums),
                new BoolValueControl(builder),
                new TextValueControl(builder, ctx, values),
                new CompositeValueControl(builder, shape),
                new IntegralValueControl(builder),
                new FloatingValueControl(builder),
                new UnityObjectValueControl(builder),
            };
        }

        /// <inheritdoc />
        protected override bool CanHandle(IValueControl stage, in ValueDrawArgs args)
        {
            return stage.CanHandle(args);
        }

        /// <inheritdoc />
        protected override Nothing Handle(IValueControl stage, in ValueDrawArgs args)
        {
            stage.Build(args);
            return Nothing.Instance;
        }

        /// <inheritdoc />
        protected override Nothing Fallback(in ValueDrawArgs args)
        {
            m_Fallback.Build(args);
            return Nothing.Instance;
        }
    }
}
