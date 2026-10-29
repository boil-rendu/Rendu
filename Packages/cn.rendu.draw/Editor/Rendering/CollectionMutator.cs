using System.Collections;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;
using RD.Draw.Editor.Selectors;
using UnityEngine;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 集合的增删改 —— 引擎里**唯一**允许写集合的地方。
    ///
    /// 列表与表格都要能"加一个 / 删一个 / 改一个", 两边的规则是一样的:
    ///   - 写操作必须经 <see cref="DrawContext.Mutate"/>, 宿主才能拍 Undo 快照、标记脏;
    ///   - 新增优先走 [ListDrawerSettings].CustomAddFunction 指定的工厂方法;
    ///   - 引用类型元素一律先弹类型选择器(见 <see cref="TypeSelector"/>),
    ///     创建动作在选完之后发生; AlwaysAddDefaultValue 可跳过;
    ///   - 元素类型由集合的声明类型推出(见 <see cref="ITypeShape.GetElementType"/>)。
    ///
    /// 增删/移动元素的规则统一收敛于此一处。
    /// </summary>
    internal sealed class CollectionMutator
    {
        private readonly DrawContext m_Ctx;
        private readonly ValueAccessor m_Values;
        private readonly ITypeShape m_Shape;

        public CollectionMutator(DrawContext ctx, ValueAccessor values, ITypeShape shape)
        {
            m_Ctx    = ctx;
            m_Values = values;
            m_Shape  = shape;
        }

        /// <summary>
        /// 往列表里加一个元素:
        ///   - [ListDrawerSettings].CustomAddFunction 优先;
        ///   - 引用类型元素(具体类 / 抽象类 / 接口)先弹类型选择器,
        ///     只有一个候选时也弹(能在浮窗里看到声明类型与 None); 选完才创建, 所以创建与
        ///     Mutate 都发生在窗口回调里;
        ///   - 值类型 / 枚举 / string 直接创建; AlwaysAddDefaultValue = 跳过选择器
        ///     (抽象/接口除外: 它们没法直接实例化)。
        /// </summary>
        public void AddElement(object target, CachedMember member, IList list, ListDrawerSettingsAttribute settings)
        {
            if (settings != null && string.IsNullOrEmpty(settings.CustomAddFunction) == false)
            {
                var method = m_Values.FindMethod(target, settings.CustomAddFunction);
                if (method != null)
                {
                    m_Values.InvokeSafe(target, method);
                    return;
                }
            }

            var elemType = m_Shape.GetElementType(member.ValueType);
            if (elemType == null) return;

            var mustPick   = elemType.IsAbstract || elemType.IsInterface;
            var skipPicker = settings != null && settings.AlwaysAddDefaultValue && mustPick == false;
            if (skipPicker == false && TypeSelector.NeedsPicker(elemType))
            {
                // 弹窗贴着"+"所在的位置弹出: 鼠标坐标现成;
                // 绘制回调里 Event.current 为空, 交给选择器窗口自己居中兜底。
                var anchor = Event.current != null && Event.current.isMouse
                    ? GUIUtility.GUIToScreenPoint(Event.current.mousePosition)
                    : (Vector2?)null;

                // 窗口是异步关闭的: target / list 一律闭包捕获, 选完只改这一份数据
                var ctx = m_Ctx;
                TypeSelector.Open(elemType, picked =>
                {
                    if (picked == null)
                    {
                        // 明确选了 "None": 往集合里放 null 是用户的决定, 不是引擎的默认行为
                        ctx.Mutate(target, () => list.Add(null));
                        return;
                    }

                    if (TypeCandidates.TryCreate(picked, out var instance))
                    {
                        ctx.Mutate(target, () => list.Add(instance));
                    }
                }, allowNone: true, screenAnchor: anchor);

                return;
            }

            if (TypeCandidates.TryCreate(elemType, out var created))
            {
                m_Ctx.Mutate(target, () => list.Add(created));
            }
            else if (mustPick)
            {
                DrawLog.Warn($"[Inspector] {elemType.Name} 是抽象类/接口且没有可实例化的实现, 无法新增元素");
            }
        }

        /// <summary>删掉列表里的第 index 个元素。</summary>
        public void RemoveAt(object target, IList list, int index)
        {
            m_Ctx.Mutate(target, () => list.RemoveAt(index));
        }

        /// <summary>改写列表里的第 index 个元素。</summary>
        public void SetAt(object target, IList list, int index, object value)
        {
            m_Ctx.Mutate(target, () => list[index] = value);
        }

        /// <summary>
        /// 把第 from 个元素挪到 to —— <paramref name="to"/> 是**插入位置**(0..Count, 与集合下标同一坐标系),
        /// 不是"挪完之后它待的下标": 源在落点之前时, 抽走源会让落点后面的元素整体前移一格,
        /// 这层换算在这里统一做, 调用方不必关心源在前还是在后。
        ///
        /// 固定长度的集合(数组)直接不挪: RemoveAt/Insert 在数组上抛异常,
        /// 而拖拽是随手动作, 不该为它弹一个异常框。
        /// </summary>
        public void MoveElement(object target, IList list, int from, int to)
        {
            if (list.IsFixedSize || list.IsReadOnly) return;
            if (from < 0 || from >= list.Count) return;

            to = to < 0 ? 0 : to > list.Count ? list.Count : to;
            // from 与 from+1 都表示"原地不动": 前者是落在自己头上, 后者是落在自己下面那条缝
            if (to == from || to == from + 1) return;

            m_Ctx.Mutate(target, () =>
            {
                var item = list[from];
                list.RemoveAt(from);
                list.Insert(to > from ? to - 1 : to, item);
            });
        }

        /// <summary>改写字典里的某个键对应的值。</summary>
        public void SetKey(object target, IDictionary dict, object key, object value)
        {
            m_Ctx.Mutate(target, () => dict[key] = value);
        }

        /// <summary>删掉字典里的某个键。</summary>
        public void RemoveKey(object target, IDictionary dict, object key)
        {
            m_Ctx.Mutate(target, () => dict.Remove(key));
        }
    }
}
