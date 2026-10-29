using System;
using System.Collections.Generic;
using UnityEditor;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;
using RD.Draw.Editor.Tables;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 绘制流程的**编排者**: 把"根对象"变成一串绘制调用, 并且是整条流水线的组合根。
    /// 它只做三件事:
    ///   1. 递归遍历(带深度上限与自引用剪断);
    ///   2. 把"画一个成员"这件事路由到对应的协作者;
    ///   3. 实现 <see cref="IMemberDrawHost"/> —— 各渲染器之间互相回调时依赖的窄接口。
    ///
    /// 它不是"渲染器"的一员, 所以方法用 <c>Draw*</c>(与 <see cref="IMemberDrawHost"/> 契约同名),
    /// 与渲染层的 <c>Render*</c> / <c>Paint*</c> 区分开(命名约定见 <see cref="ValueMemberRenderer"/>)。
    ///
    /// 各关注点的画法都归各自的类:
    ///   - 分组 → <see cref="GroupRenderer"/>
    ///   - 成员 → <see cref="MemberRenderChain"/>(链上各档见 Rendering/MemberRenderers/ 目录)
    ///   - 值   → <see cref="ValueControlChain"/>(链上各档见 Rendering/Values/ 目录)
    ///   - 集合 → <see cref="CollectionRenderer"/>; 表格 → <see cref="TableRenderer"/>
    ///   - 批注 → <see cref="AnnotationPainter"/>; 按钮 → <see cref="ButtonPainter"/>
    ///
    /// 状态在 <see cref="DrawContext"/> 里, 随绘制调用往下传; 元数据/规则/取值来自注入的协作者。
    /// </summary>
    internal sealed class InspectorRenderer : IMemberDrawHost
    {
        private const int MaxDepth = 24;

        /// <summary>
        /// 正在绘制的对象(剪断 [InlineEditor] 自引用环)。
        /// 这是**遍历**的状态, 不是绘制上下文: 只有递归本身关心它, 所以挂在编排者身上,
        /// 不再让 DrawContext 多背一项职责。
        /// </summary>
        private readonly HashSet<object> m_Drawing = new(ReferenceComparer.Instance);

        private readonly DrawContext m_Ctx;
        private readonly MemberCollector m_Members;
        private readonly GroupResolver m_Groups;
        private readonly ValueAccessor m_Values;

        private readonly Widget m_Root;
        private readonly WidgetBuilder m_Builder;
        private readonly ChangeApplier m_ChangeApplier;

        private readonly AnnotationPainter m_AnnotationPainter;
        private readonly ValueControlChain m_ValueControls;
        private readonly CollectionMutator m_CollectionMutator;
        private readonly ListPager m_Pager;
        private readonly TableRenderer m_TableRenderer;
        private readonly CollectionRenderer m_CollectionRenderer;
        private readonly ButtonPainter m_ButtonPainter;
        private readonly MemberRenderChain m_MemberRenderers;
        private readonly ValueMemberRenderer m_ValueMemberRenderer;
        private readonly GroupRenderer m_GroupRenderer;

        private InspectorRenderer(DrawContext ctx, EngineServices services)
        {
            m_Ctx     = ctx;
            m_Members = services.Members;
            m_Groups  = services.Groups;
            m_Values  = services.Values;

            // 产树基础设施: 根节点 / 建树器 / 渲染后的变更应用器。
            // 根用 RootWidget(只挂子节点、自身不画容器), 免得整块面板外面多一层纵向组。
            m_Root          = new RootWidget();
            m_Builder       = new WidgetBuilder(m_Root);
            m_ChangeApplier = new ChangeApplier(ctx, services.Values);

            // 装配顺序即依赖顺序: 先建"没有回环"的那些, 再建彼此回调的。
            m_ValueControls     = new ValueControlChain(m_Builder, ctx, services.Enums, m_Values, services.Shape);
            m_AnnotationPainter       = new AnnotationPainter(ctx, m_Builder, services.Visibility, services.Labels);
            m_CollectionMutator = new CollectionMutator(ctx, m_Values, services.Shape);
            m_Pager             = new ListPager(ctx);

            // 表格要能"把单元格里的成员画出来", 集合要能"把列表里的元素画出来" ——
            // 两者都通过窄接口 IMemberDrawHost 回到本渲染器, 不直接依赖彼此。
            m_TableRenderer      = new TableRenderer(ctx, m_Builder, m_Values, services.Labels, services.Tables,
                                                     services.Visibility, m_CollectionMutator, m_Pager,
                                                     services.Diagnostics, this);
            m_CollectionRenderer = new CollectionRenderer(ctx, m_Builder, m_Values, services.Labels, services.Shape,
                                                          m_CollectionMutator, m_TableRenderer, m_Pager, this);
            m_ButtonPainter     = new ButtonPainter(ctx, m_Builder, m_Values, services.Labels);

            m_MemberRenderers     = new MemberRenderChain(BuildMemberChain(ctx, services));
            m_ValueMemberRenderer = new ValueMemberRenderer(ctx, m_Builder, services.Labels, services.Visibility,
                                                            m_Values, m_AnnotationPainter, m_MemberRenderers);
            m_GroupRenderer       = new GroupRenderer(ctx, m_Builder, services.Visibility, services.States, this);
        }

        /// <summary>
        /// 装配一次绘制所需的全部协作者 —— 本类是这条流水线的**组合根**。
        ///
        /// 之所以做成工厂而不是公开构造函数: 装配有严格的依赖顺序(先建无回环的, 再建彼此回调的),
        /// 而且中途要把 <c>this</c> 作为 <see cref="IMemberDrawHost"/> 发给表格、集合与分组渲染器。
        /// 顺序或回环一旦被外部改坏, 表现是运行时空引用而不是编译错误 —— 私有构造 + 单一入口
        /// 让"只有一种正确的装配方式"成为结构上的保证。
        /// </summary>
        public static InspectorRenderer Create(DrawContext ctx, EngineServices services)
        {
            return new InspectorRenderer(ctx, services);
        }

        /// <summary>
        /// 成员渲染链: **顺序即优先级**, 链上第一个 <c>CanDraw</c> 为真的获胜。
        ///
        /// 每一档为什么在这个位置(改动前请读):
        ///   1. [InlineEditor] —— 显式要求内联展开, 必须压过任何"按类型默认"的画法;
        ///   2. 容器(集合 / 可展开的普通类型)—— 会展开成子成员, 不是一行输入框能表达的;
        ///   3. [DisplayAsString] —— 显式要求按字符串显示;
        ///   4. [ProgressBar] —— 同样是显式特性, 只影响画法, 不改变"是不是一行";
        ///   5. 可编辑值(兜底)—— 交给值控件链按类型挑控件。
        ///
        /// 3 与 4 都是显式特性, 彼此顺序等价; 这里按"读起来与特性书写顺序一致"排列。
        /// 兜底那档必须垫底: 它对任何成员都返回 true, 放到前面会把 1~4 全部吃掉。
        /// </summary>
        private IMemberRenderer[] BuildMemberChain(DrawContext ctx, EngineServices services)
        {
            return new IMemberRenderer[]
            {
                new InlineEditorRenderer(ctx, m_Builder, m_Values, services.Labels, services.Shape, this),
                new ContainerRenderer(ctx, m_Builder, m_Values, services.Labels, services.Shape,
                                      m_CollectionRenderer, this),
                new DisplayAsStringRenderer(ctx, m_Builder, m_Values, services.Labels, services.Shape),
                new ProgressBarRenderer(ctx, m_Builder, m_Values, services.Labels, services.Shape),
                new EditableValueRenderer(ctx, m_Builder, m_Values, services.Labels, services.Shape,
                                          services.Visibility, m_ValueControls),
            };
        }

        //-------------------------------------- 主流程 --------------------------------------

        /// <summary>绘制根对象(始终允许绘制: EditorWindow 自绘, 或 Editor 检视组件)。</summary>
        public void DrawRoot(object target, EditorWindow host)
        {
            try
            {
                DrawRecursive(target, host, 0, null, true);
            }
            finally
            {
                // 建树中途抛异常也要把已建好的部分落屏: 全空的面板比"半棵树"更难判断出了什么事。
                Flush();
            }
        }

        /// <summary>
        /// 把这一帧攒下的树交给解释器落屏, 再把交互结果(点击 / 新值 / 折叠态)应用回数据。
        ///
        /// 这是"产树 → 解释"的唯一收口: 建树阶段一次都不碰后端, 变更加数据也一律推迟到
        /// 渲染之后 —— 建树时控件还没被解释, 根本不知道用户输入了什么。
        ///
        /// finally 里把树还给对象池: 变更回放要用节点里的新值, 所以释放必须排在它之后;
        /// 同时清掉 builder 的变更/按钮表 —— 它们持有节点与动作闭包, 不清就会拖到下一帧。
        ///
        /// 本方法是一次性的: 树已归还, 同一个 <see cref="InspectorRenderer"/> 不能再画第二遍
        /// (每个 <c>Engine.DrawObject</c> 都新建一个实例, 生命周期本来就只覆盖一帧)。
        /// </summary>
        private void Flush()
        {
            try
            {
                var render = new RenderContext(m_Ctx);
                m_Ctx.Renderer.Render(m_Root, render);
                m_ChangeApplier.Apply(m_Builder, render);
            }
            finally
            {
                m_Builder.Changes.Clear();
                m_Builder.Buttons.Clear();
                WidgetPool.Release(m_Root);
            }
        }

        private void DrawRecursive(object target, EditorWindow host, int depth, object parent,
                                   bool allowUnityObject = false)
        {
            if (target == null || depth > MaxDepth) return;
            // 默认不内窥 UnityEngine.Object(只允许宿主窗口自绘), [InlineEditor] 显式展开时传 allowUnityObject
            if (allowUnityObject == false && target is UnityEngine.Object unityObject &&
                ReferenceEquals(unityObject, host) == false)
            {
                return;
            }

            var type = target.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(string)) return;

            if (parent != null && ReferenceEquals(target, parent)) return;
            // [InlineEditor] 可能构成自引用, 用"正在绘制"集合剪断环
            if (m_Drawing.Contains(target)) return;

            m_Drawing.Add(target);
            try
            {
                var members = m_Members.Get(type);
                RenderMembers(target, members, host, depth);
            }
            finally
            {
                m_Drawing.Remove(target);
            }
        }

        private void RenderMembers(object target, CachedMember[] members, EditorWindow host, int depth)
        {
            if (members == null || members.Length == 0) return;

            var indent = depth > 0;
            if (indent)
            {
                m_Builder.BeginIndent();
            }

            // 根节点代表"未分组"区域, 永远可见, 因此不存在"未分组成员被跳过"的情况。
            var root = m_Groups.Get(target.GetType(), members);
            m_GroupRenderer.Render(root, target, host, depth, true);

            if (indent)
            {
                m_Builder.EndIndent();
            }
        }

        //-------------------------------------- IMemberDrawHost --------------------------------------

        /// <summary>按成员种类分发到对应的绘制路径。</summary>
        public void DrawMember(object target, CachedMember member, EditorWindow host, int depth)
        {
            switch (member.Kind)
            {
                case MemberKind.MethodButton:
                    m_ButtonPainter.Paint(target, member);
                    break;
                case MemberKind.MethodOnInspectorGUI:
                    m_Values.InvokeSafe(target, member.Info);
                    break;
                default:
                    m_ValueMemberRenderer.Render(target, member, host, depth, false);
                    break;
            }
        }

        /// <summary>绘制一个成员的值(表格单元格内用, 标签由表头承担)。</summary>
        public void DrawValueMember(object target, CachedMember member, EditorWindow host, int depth, bool inTableCell)
        {
            m_ValueMemberRenderer.Render(target, member, host, depth, inTableCell);
        }

        /// <summary>把一个对象当作子对象展开绘制(嵌套对象 / [InlineEditor])。</summary>
        public void DrawNested(object value, EditorWindow host, int depth, object parent, bool allowUnityObject)
        {
            DrawRecursive(value, host, depth, parent, allowUnityObject);
        }
    }
}
