using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RD.Inspector.Editor
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
    internal sealed class InspectorSession
    {
        /// <summary>单个宿主对象的状态。</summary>
        private sealed class ObjectState
        {
            public readonly Dictionary<string, bool> ToggleState = new();
            public readonly Dictionary<string, bool> GroupToggleState = new();
            public readonly Dictionary<string, Vector2> ScrollPositions = new();
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
            return state != null && state.ToggleState.TryGetValue(name, out var value) ? value : fallback;
        }

        /// <summary>写入成员折叠状态。</summary>
        public void SetToggle(object owner, string name, bool value)
        {
            var state = GetState(owner, true);
            if (state != null) state.ToggleState[name] = value;
        }

        /// <summary>读取分组展开状态; 没有记录时返回 fallback(分组属性声明的默认值)。</summary>
        public bool GetGroupToggle(object owner, string groupId, bool fallback)
        {
            var state = GetState(owner, false);
            return state != null && state.GroupToggleState.TryGetValue(groupId, out var value) ? value : fallback;
        }

        /// <summary>写入分组展开状态。</summary>
        public void SetGroupToggle(object owner, string groupId, bool value)
        {
            var state = GetState(owner, true);
            if (state != null) state.GroupToggleState[groupId] = value;
        }

        /// <summary>读取表格滚动位置; 没有记录时为零。</summary>
        public Vector2 GetScroll(object owner, string name)
        {
            var state = GetState(owner, false);
            return state != null && state.ScrollPositions.TryGetValue(name, out var value) ? value : Vector2.zero;
        }

        /// <summary>写入表格滚动位置。</summary>
        public void SetScroll(object owner, string name, Vector2 value)
        {
            var state = GetState(owner, true);
            if (state != null) state.ScrollPositions[name] = value;
        }
    }
}
