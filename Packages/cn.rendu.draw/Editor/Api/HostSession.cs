using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 一次检视会话的编辑期状态(成员折叠状态、分组展开状态、表格滚动位置)。
    ///
    /// 状态一律按宿主对象弱引用分桶(ConditionalWeakTable), 桶内再按"成员名 / 分组路径"索引:
    ///   1. 对象被回收后整桶随之消失, 不会像早期实现那样在进程级字典里留下永不释放的键;
    ///   2. 分桶判定走"引用相等", 不会像身份哈希被复用那样, 把上一个对象的状态错读到新对象上;
    ///   3. 同一窗口里两个同类型对象(例如列表中的两行)各自持有独立状态, 互不传染 ——
    ///      这对分组折叠尤其要紧, 因为分组路径在同类型的所有实例上都是同一个字符串。
    ///
    /// 分组键(GroupID 路径)与成员键(成员名)是两个维度, 因此分桶后各留一张表。
    ///
    /// 会话由宿主持有(InspectorEditor 各持一份, 窗口之间彼此隔离);
    /// DrawObject 在绘制期间挂载、退出时还原, 未显式传入时用默认会话兜底(也便于脱机测试)。
    /// </summary>
    internal sealed class HostSession
    {
        /// <summary>
        /// 单个宿主对象的状态。
        ///
        /// 每类状态就是一个 <see cref="Slot{T}"/>, 新增一类状态只需要加一行字段 + 一对读写方法。
        /// </summary>
        private sealed class ObjectState
        {
            public readonly Slot<bool> Toggle = new();
            public readonly Slot<bool> GroupToggle = new();
            public readonly Slot<Vector2> Scroll = new();
            public readonly Slot<int> Page = new();
            public readonly Slot<float> ContentWidth = new();
            public readonly Slot<float> ColumnWidth = new();
            public readonly Slot<float> ContentTop = new();
        }

        /// <summary>一类状态的"键 → 值"表; 没有记录时由调用方给兜底值。</summary>
        private sealed class Slot<T>
        {
            private readonly Dictionary<string, T> m_Values = new();

            public T Get(string key, T fallback)
            {
                return m_Values.TryGetValue(key, out var value) ? value : fallback;
            }

            public void Set(string key, T value)
            {
                m_Values[key] = value;
            }
        }

        /// <summary>宿主对象 -> 它的状态。键被回收时条目自动移除。</summary>
        private readonly ConditionalWeakTable<object, ObjectState> m_ObjectStates = new();

        /// <summary>取状态桶; create 为 false 时只查不建, 避免纯读取路径留下空桶。</summary>
        private ObjectState GetState(object owner, bool create)
        {
            if (owner == null) return null;
            if (m_ObjectStates.TryGetValue(owner, out var state)) return state;
            return create ? m_ObjectStates.GetValue(owner, _ => new ObjectState()) : null;
        }

        /// <summary>读取成员折叠状态; 没有记录时返回 fallback(成员默认展开)。</summary>
        public bool GetToggle(object owner, string name, bool fallback)
        {
            var state = GetState(owner, false);
            return state == null ? fallback : state.Toggle.Get(name, fallback);
        }

        /// <summary>写入成员折叠状态。</summary>
        public void SetToggle(object owner, string name, bool value)
        {
            GetState(owner, true)?.Toggle.Set(name, value);
        }

        /// <summary>读取分组展开状态; 没有记录时返回 fallback(分组属性声明的默认值)。</summary>
        public bool GetGroupToggle(object owner, string groupId, bool fallback)
        {
            var state = GetState(owner, false);
            return state == null ? fallback : state.GroupToggle.Get(groupId, fallback);
        }

        /// <summary>写入分组展开状态。</summary>
        public void SetGroupToggle(object owner, string groupId, bool value)
        {
            GetState(owner, true)?.GroupToggle.Set(groupId, value);
        }

        /// <summary>读取表格滚动位置; 没有记录时为零。</summary>
        public Vector2 GetScroll(object owner, string name)
        {
            var state = GetState(owner, false);
            return state == null ? Vector2.zero : state.Scroll.Get(name, Vector2.zero);
        }

        /// <summary>写入表格滚动位置。</summary>
        public void SetScroll(object owner, string name, Vector2 value)
        {
            GetState(owner, true)?.Scroll.Set(name, value);
        }

        /// <summary>读取列表分页索引; 没有记录时为零(第一页)。</summary>
        public int GetPage(object owner, string name)
        {
            var state = GetState(owner, false);
            return state == null ? 0 : state.Page.Get(name, 0);
        }

        /// <summary>写入列表分页索引。</summary>
        public void SetPage(object owner, string name, int value)
        {
            GetState(owner, true)?.Page.Set(name, value);
        }

        /// <summary>
        /// 读取表格量到的可用宽; 没有记录时为 0。
        ///
        /// 表格自己开滚动区时存的是**数据行的宽**(滚动区里量到的, 已净掉盒子 padding 与滚动条),
        /// 没开滚动区时存的是面板给的宽(表格盒的内缩还得按层估)。
        ///
        /// 布局尚未结算出真实宽时, 表格排版必须用"上一帧量到的值"—— 这里就是那个跨帧的存处。
        /// </summary>
        public float GetContentWidth(object owner, string name)
        {
            var state = GetState(owner, false);
            return state == null ? 0f : state.ContentWidth.Get(name, 0f);
        }

        /// <summary>写入表格量到的可用宽。</summary>
        public void SetContentWidth(object owner, string name, float width)
        {
            GetState(owner, true)?.ContentWidth.Set(name, width);
        }

        /// <summary>读取用户拖拽过的表格列宽; 没有记录时为零(用声明值 / 均分值)。</summary>
        public float GetColumnWidth(object owner, string key)
        {
            var state = GetState(owner, false);
            return state == null ? 0f : state.ColumnWidth.Get(key, 0f);
        }

        /// <summary>
        /// 写入用户拖拽过的表格列宽。
        ///
        /// 存在会话里而不是 <c>TableColumn</c> 上: 列定义按元素类型缓存、所有同类型的表格共用一份,
        /// 宽度写进去会串到另一张表(甚至另一个对象)上。按宿主对象分桶正好是"这张表的这一列"。
        /// </summary>
        public void SetColumnWidth(object owner, string key, float width)
        {
            GetState(owner, true)?.ColumnWidth.Set(key, width);
        }

        /// <summary>
        /// 宿主视口宽(滚动区视口的宽): 0 = 宿主没给。
        ///
        /// 表格按它均分列宽, 而不是按"根节点量到的宽": 后者会被内容自己撑宽 ——
        /// 定宽的表头/单元格抬高面板的最小宽 → 面板内容宽跟着涨 → 根节点量到的宽再涨 →
        /// 列宽再按新宽重算, 形成"越撑越宽"的死循环(实测能撑到几千像素)。
        /// 视口宽由宿主在滚动区外给定, 与内容无关, 拿它当基准就没有这个反馈环。
        /// </summary>
        public float ViewportWidth { get; private set; }

        /// <summary>写入宿主视口宽(宿主在 EndScrollView 之后量滚动区矩形得到); 传 0 表示"没有视口宽"。</summary>
        public void SetViewportWidth(float width)
        {
            ViewportWidth = width > 0f ? width : 0f;
        }

        /// <summary>
        /// 读取表格滚动区那一处的纵坐标(窗口坐标); 没有记录时为零。
        ///
        /// 与 <see cref="GetContentWidth"/> 同样的道理:  GetRect 只在 Repaint 事件给出可信位置,
        /// 排版必须用"上一帧量到的值" —— 当场用会让 Layout 与 Repaint 两个 pass 算出不同的滚动区高度,
        ///  随后报控件数与布局不一致。
        /// </summary>
        public float GetContentTop(object owner, string name)
        {
            var state = GetState(owner, false);
            return state == null ? 0f : state.ContentTop.Get(name, 0f);
        }

        /// <summary>写入表格滚动区那一处的纵坐标(见 <see cref="GetContentTop"/>)。</summary>
        public void SetContentTop(object owner, string name, float top)
        {
            GetState(owner, true)?.ContentTop.Set(name, top);
        }

        /// <summary>
        /// 内容区的可用下沿(窗口坐标): 0 = 宿主没给。
        ///
        /// 表格拿它减去"当前布局位置的纵坐标", 得到数据区还能占多高 —— 数据区底边因此能贴着
        /// 表格盒的下沿, 窗口拉高时跟着拉长。宿主的滚动区里内容高度没有上界,
        /// 这是唯一能知道"还剩多少高"的途径。
        ///
        /// 宿主报的必须是**折算过自身滚动位置**的值: 内容滚下去多少, 这个下沿就跟着减多少,
        /// 于是"还剩多少高"与滚动位置无关 —— 否则一滚滚轮, 表格就会跟着长/缩。
        /// </summary>
        public float ContentBottom { get; private set; }

        /// <summary>写入内容区可用下沿(见 <see cref="ContentBottom"/>); 传 0 表示"没有这个信息"。</summary>
        public void SetContentBottom(float bottom)
        {
            ContentBottom = bottom > 0f ? bottom : 0f;
        }
    }
}
