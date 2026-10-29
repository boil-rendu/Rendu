namespace RD.Draw.Editor
{
    /// <summary>
    /// 声明式控件树的节点基类: 平台无关的单一描述。
    ///
    /// 规则层不再直接驱动 <c>Driver</c>, 而是先产出一棵 <see cref="Widget"/> 树; 渲染器
    /// (<c>Renderer</c>) 消费同一棵树 + 同一份 <c>RDTheme</c> +
    /// 同一份 <c>LayoutPolicy</c>, 再调用 <c>Driver</c> 原语落屏。 "一致"由"同一份描述、
    /// 同一份令牌、同一份规则"从结构上保证, 而不是靠两端代码写得像。
    ///
    /// 它吸收 <c>GroupNode</c>(容器子集)并推广到任意控件节点 —— 树只有一棵, 不另起"结构树"。
    ///
    /// 生命周期: 每次绘制新建一棵树(对象池尚未落地, 见设计文档 §0.1), 渲染完即丢弃;
    /// 节点顺序即渲染顺序。叶子节点占多数, 因此子节点表按需分配(见 <see cref="ChildrenOrNull"/>)。
    /// </summary>
    internal abstract class Widget
    {
        private System.Collections.Generic.List<Widget> m_Children;

        /// <summary>
        /// 已分配的子节点表; <c>null</c> = 这棵子树还没有子节点。
        ///
        /// 叶子节点(标签 / 按钮 / 输入框)占绝大多数, 每帧为它们各分配一个 <c>List</c>
        /// 是白白的热路径开销 —— 所以列表按需创建, 读的一方用 <see cref="ChildrenOrNull"/> 判空。
        /// </summary>
        public System.Collections.Generic.List<Widget> ChildrenOrNull => m_Children;

        /// <summary>有序子节点(渲染顺序即列表顺序); 第一次访问时才分配。</summary>
        public System.Collections.Generic.List<Widget> Children => m_Children ??= new();

        /// <summary>挂一个子节点(不经过 <see cref="Children"/> 的读方, 避免误分配)。</summary>
        public void Add(Widget child) => Children.Add(child);

        /// <summary>
        /// 是否已经在池里(池内部使用)。
        ///
        /// 它让 <see cref="WidgetPool.Release"/> 可以被重复调用而不出错: 借用方若在 finally 里
        /// 归还、外层又归还了一次, 同一实例被塞进池两次的话, 两次 <see cref="WidgetPool.Rent{T}"/>
        /// [历史] 会拿到同一个节点 —— 那是最难查的一类"节点莫名其妙被改"。
        /// </summary>
        internal bool InPool;

        /// <summary>
        /// 把自身字段恢复成"刚构造"的状态, 供对象池复用(见 <see cref="WidgetPool"/>)。
        ///
        /// 子类必须**逐个清空自己声明的字段**(包括解释器回填的输出字段)。漏一个的后果很隐蔽:
        /// 复用这个节点的那一帧会带着上一帧的旧值 —— 比如上一帧的拖拽落点、上一帧的列宽增量。
        /// 清空同时负责断开引用(字符串 / 宿主对象 / 钩子闭包), 所以池里的节点不会钉住业务数据。
        ///
        /// 子节点不在这里清: 归还时由池先递归归还子树、再清空 <see cref="ChildrenOrNull"/>。
        /// </summary>
        public virtual void Reset()
        {
        }
    }

    /// <summary>
    /// 控件分支基类: 按钮/折叠/字段/滑条等"原生控件"节点都从这里派生。
    /// <see cref="IControlRenderer"/> 按它判别"该控件由哪个可插拔实现渲染"(原生控件差异的隔离边界)。
    /// </summary>
    internal abstract class ControlWidget : Widget
    {
    }

    /// <summary>
    /// 整棵树的根: 只承载子节点, **自身不画任何容器**。
    ///
    /// 根如果直接用一个 <see cref="ColumnWidget"/>, 解释器就会在整块面板外面多开一层纵向组 ——
    ///  里多一层布局组、 里多一层 VisualElement, 都会让"撑满"的语义偏一档。
    /// 根只当挂载点, 内容原样接到宿主的容器里。
    /// </summary>
    internal sealed class RootWidget : Widget
    {
    }
}
