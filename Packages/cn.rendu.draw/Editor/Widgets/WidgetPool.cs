using System.Collections.Generic;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 控件树节点的对象池(设计决策 4: 树对象复用, 而不是每帧新建)。
    ///
    /// 为什么需要: 面板每次 OnGUI 都要重建整棵树 —— 一次绘制的节点数从几十到几千(一张长表格),
    ///  每帧还要跑 Layout + Repaint 两个 pass, 全部 new 出来就是每帧几千次小分配。
    ///
    /// 复用约定:
    ///   - 借出的节点一定处于"刚构造"状态(见 <see cref="Widget.Reset"/>), 建树方像用新对象一样用;
    ///   - 归还时先递归归还子树、清空子节点表, 再 <see cref="Widget.Reset"/> —— 池里的节点不持有
    ///     字符串 / 宿主对象 / 钩子闭包, 不会把业务数据钉在内存里;
    ///   - 每种类型一个栈、各自有上限: 偶发的大面板(上万行)不会把池撑得失真。
    ///
    /// 加新节点类型时做两件事: 在它的 <see cref="Widget.Reset"/> 里清空自己的字段、
    /// 在 <see cref="Return"/> 的 <c>switch</c> 里登记。忘了登记不会出错, 只是那种节点不再复用。
    /// </summary>
    internal static class WidgetPool
    {
        /// <summary>每种类型最多缓存的节点数: 超出的直接丢弃。</summary>
        private const int MaxPerType = 512;

        /// <summary>
        /// 子节点表最多保留的容量: 一张几百行的表格画完后, 容器节点会带着一个几百槽的数组回池 ——
        /// 512 个这样的容器就是几十万个引用。超过这个数就把容量丢掉, 复用只保小数组。
        /// </summary>
        private const int MaxRetainedChildren = 64;

        /// <summary>借一个节点; 池里没有就新建。返回的节点字段一定是默认值。</summary>
        public static T Rent<T>() where T : Widget, new()
        {
            var pool = Pool<T>.Items;
            if (pool.Count == 0) return new T();

            var node = pool.Pop();

            // 归还时已经 Reset 过, 这里再清一次: 几十次字段写远比一次漏清导致的"上一帧旧值"便宜。
            // 子节点表也再清一次 —— 正常路径上 Release 已经清过, 这条只是兜住将来可能新增的归还入口。
            node.ChildrenOrNull?.Clear();
            node.Reset();
            node.InPool = false;
            return node;
        }

        /// <summary>整棵树归还。调用时机: 渲染与变更回放都结束之后(见 InspectorRenderer.Flush)。</summary>
        public static void Release(Widget root)
        {
            if (root == null) return;
            ReleaseNode(root);
        }

        private static void ReleaseNode(Widget node)
        {
            // 重复归还(例如宿主在 finally 里又还了一次)必须是无操作: 同一实例进池两次,
            // 之后两次 Rent 就会拿到同一个节点。见 Widget.InPool。
            if (node.InPool) return;

            var children = node.ChildrenOrNull;
            if (children != null)
            {
                // 先还子树再清表: Clear 之后就拿不到子节点了
                for (var i = 0; i < children.Count; i++) ReleaseNode(children[i]);
                children.Clear();

                // 长列表的容量不留在池里(见 MaxRetainedChildren)
                if (children.Capacity > MaxRetainedChildren) children.Capacity = 0;
            }

            node.Reset();
            node.InPool = true;
            Return(node);
        }

        /// <summary>按精确类型入池: <see cref="Pool{T}"/> 是泛型静态类, 每类型各一份栈, 无需字典查找。</summary>
        private static void Return(Widget node)
        {
            switch (node)
            {
                // 容器 / 文本
                case ColumnWidget n:          Push(n); break;
                case RowWidget n:             Push(n); break;
                case IndentWidget n:          Push(n); break;
                case ScrollWidget n:          Push(n); break;
                case SpaceWidget n:           Push(n); break;
                case FlexibleSpaceWidget n:   Push(n); break;
                case SeparatorWidget n:       Push(n); break;
                case LabelWidget n:           Push(n); break;
                case KeyValueLabelWidget n:   Push(n); break;
                case HelpBoxWidget n:         Push(n); break;
                case ProgressBarWidget n:     Push(n); break;

                // 可选能力 / 包裹节点
                case PanelHeaderWidget n:     Push(n); break;
                case ColumnSeparatorWidget n: Push(n); break;
                case ColumnResizerWidget n:   Push(n); break;
                case CardWidget n:            Push(n); break;
                case ReorderWidget n:         Push(n); break;
                case ReorderItemWidget n:     Push(n); break;
                case ReorderHandleWidget n:   Push(n); break;
                case StyledScopeWidget n:     Push(n); break;

                // 原生控件
                case ButtonWidget n:          Push(n); break;
                case FoldoutWidget n:         Push(n); break;
                case TextFieldWidget n:       Push(n); break;
                case IntegerFieldWidget n:    Push(n); break;
                case FloatFieldWidget n:      Push(n); break;
                case SliderWidget n:          Push(n); break;
                case ToggleWidget n:          Push(n); break;
                case PopupWidget n:           Push(n); break;
                case ToolbarWidget n:         Push(n); break;
                case ToolbarMultiWidget n:    Push(n); break;
                case EnumFlagsWidget n:       Push(n); break;
                case ObjectFieldWidget n:     Push(n); break;
                case StructFieldWidget n:     Push(n); break;

                // RootWidget 与将来新增的类型: 不入池即可(不会出错, 只是不复用)
                default: break;
            }
        }

        private static void Push<T>(T node) where T : Widget
        {
            var pool = Pool<T>.Items;
            if (pool.Count < MaxPerType) pool.Push(node);
        }

        /// <summary>每个节点类型各自一个栈(泛型静态类天然按类型分表)。</summary>
        private static class Pool<T> where T : Widget
        {
            public static readonly Stack<T> Items = new();
        }
    }
}
