using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 后端的**元素调和器**: 把引擎的立即模式调用序列落成一棵保留模式的 VisualElement 树。
    ///
    /// 本后端采用保留模式: 把引擎的调用序列落成一棵保留模式的 VisualElement 树。
    /// 做法是**按位置复用**: 进入容器时记下游标, 每取一个元素先看当前位置是不是想要的类型 ——
    /// 是就只更新内容与状态, 不是才换掉; 容器结束时把游标之后多余的子元素删掉。
    /// 于是折叠 / 条件显隐变化时只动受影响的那一段, 正在输入的输入框不会因整树重建而丢焦点。
    ///
    /// 单独成类的理由: 这套机制与"画什么控件"完全无关, 却横切在每一个绘制方法里
    /// (每个控件都要先 <see cref="Take{T}"/> 拿到元素)。留在 <see cref="Driver"/> 里,
    /// 读任何一个控件方法都得先跳到文件另一头看游标怎么走。
    ///
    /// 元素的**外观**由 <see cref="Styler"/> 施加, 不在本类职责内 ——
    /// 但两者必须成对使用: 复用元素的同时刷外观, 否则会残留上一轮的值。
    /// </summary>
    internal sealed class Tree
    {
        /// <summary>施加外观。复用元素时必调, 顺序与取元素绑定。</summary>
        private readonly Styler m_Styler;

        public Tree(Styler styler)
        {
            m_Styler = styler;
        }

        /// <summary>每轮开头归位: 上一轮若被异常打断, 作用域栈会留着半截, 之后整块面板的元素都会取到错的容器。</summary>
        internal void Reset() => m_Scopes.Clear();

        /// <summary>当前所在容器(最内层作用域); 一个作用域都没进时为 null。</summary>
        public VisualElement CurrentContainer =>
            m_Scopes.Count > 0 ? m_Scopes[m_Scopes.Count - 1].Container : null;

        /// <summary>
        /// 当前容器里"刚画完那一个"元素的底边(面板坐标); 还没画过元素时给内容区顶边。
        /// 0 = 还没布局(新建元素的世界坐标全是 0)。
        ///
        /// 只看到游标之前的元素: 游标之后还留着上一轮的元素(末尾由 <see cref="Pop"/> 裁掉),
        /// 按 childCount-1 取会拿到那一个 —— 对"当前写指针"来说位置是错的。
        /// </summary>
        public float CurrentCursorBottom()
        {
            if (m_Scopes.Count == 0) return 0f;

            var scope = m_Scopes[m_Scopes.Count - 1];
            var index = scope.Cursor - 1;
            if (index >= 0 && index < scope.Container.childCount)
            {
                var bottom = scope.Container[index].worldBound.yMax;
                if (bottom > 1f) return bottom;
            }

            var top = scope.Container.worldBound.y + scope.Container.resolvedStyle.paddingTop;
            return top > 1f ? top : 0f;
        }

        /// <summary>
        /// 最近一次 <see cref="Pop"/> 关掉的容器; 没关过时为 null。
        ///
        /// 给"刚画完一块, 想回看它落在哪"的调用方用(列表条目登记拖拽区间即是),
        /// 免得把容器元素再从调用方一路传下来 —— 这个信息在调和器里本来就是现成的。
        /// 只保证"上一次 Pop 的是哪个", 中间再画了别的容器就会被覆盖, 所以必须在 Pop 之后立刻取。
        /// </summary>
        public VisualElement LastPopped { get; private set; }

        private sealed class Scope
        {
            public VisualElement Container;
            public int           Cursor;
        }

        private readonly List<Scope> m_Scopes = new();
        private readonly StringBuilder m_KeyBuilder = new();



        /// <summary>当前容器内取一个元素: 类型相符就复用, 否则替换。key 用它在树里的位置路径。</summary>
        internal T Take<T>(Func<T> create, Action<T> apply) where T : VisualElement
        {
            var scope = m_Scopes[m_Scopes.Count - 1];
            var key   = KeyOf(scope);

            if (scope.Cursor < scope.Container.childCount)
            {
                // 复用判定必须精确类型, 不能用 is T: 行容器取的是 VisualElement, 而 Button / Box / Label
                // 统统都是 VisualElement。用 is 会把上一轮留在这一位置的按钮当成"行"复用 —— 后面画的内容
                // 全变成这个按钮的子元素, 视觉上就是叠在按钮上互相遮挡, 类型也全乱了
                if (scope.Container[scope.Cursor].GetType() == typeof(T))
                {
                    var hit = (T)scope.Container[scope.Cursor];
                    scope.Cursor++;
                    hit.userData = key;
                    m_Styler.ApplyState(hit);
                    apply?.Invoke(hit);
                    return hit;
                }

                // 同一个位置换了别的控件: 掉的不是后半棵树, 只有这一个
                scope.Container.RemoveAt(scope.Cursor);
            }

            var made = create();
            made.userData = key;
            // 插回原位而不是追加到末尾: 游标之后可能还有上一轮留下的元素(末尾由 Pop 裁),
            // 追加会让新元素排到它们后面, 顺序错位
            scope.Container.Insert(scope.Cursor, made);
            scope.Cursor++;
            m_Styler.ApplyState(made);
            apply?.Invoke(made);
            return made;
        }


        internal void PushAsScope(VisualElement container)
        {
            m_Scopes.Add(new Scope { Container = container, Cursor = 0 });
        }

        internal void Pop()
        {
            if (m_Scopes.Count == 0) return;

            // 游标之后的子元素是上次留下来的: 这一次没画到, 说明它们对应的成员已经消失/被隐藏
            var scope = m_Scopes[m_Scopes.Count - 1];
            while (scope.Container.childCount > scope.Cursor)
            {
                scope.Container.RemoveAt(scope.Container.childCount - 1);
            }

            LastPopped = scope.Container;
            m_Scopes.RemoveAt(m_Scopes.Count - 1);
        }

        private string KeyOf(Scope next)
        {
            m_KeyBuilder.Length = 0;
            for (var i = 0; i < m_Scopes.Count; i++)
            {
                if (i > 0) m_KeyBuilder.Append('/');
                m_KeyBuilder.Append(ReferenceEquals(m_Scopes[i], next) ? next.Cursor : m_Scopes[i].Cursor);
            }

            return m_KeyBuilder.ToString();
        }
    }
}
