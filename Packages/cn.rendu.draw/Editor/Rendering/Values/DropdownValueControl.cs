using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// [ValueDropdown]: 下拉项由"另一个成员"在运行期给出, 而非由类型推导。
    ///
    /// 它排在注册表第一位 —— 属性显式指定的下拉要压过任何按类型的默认画法。
    /// 取不到候选时退回一个只读文本框, 不静默变成别的控件。
    /// </summary>
    internal sealed class DropdownValueControl : ValueControlBase
    {
        private readonly ValueAccessor m_Values;

        public DropdownValueControl(WidgetBuilder builder, ValueAccessor values) : base(builder)
        {
            m_Values = values;
        }

        public override bool CanHandle(in ValueDrawArgs args)
        {
            return args.Member.GetAttr<ValueDropdownAttribute>() != null;
        }

        public override void Build(in ValueDrawArgs args)
        {
            var dropdown = args.Member.GetAttr<ValueDropdownAttribute>();
            var value    = args.Value;
            var items    = ResolveItems(args.Target, dropdown.ValuesGetter);
            if (items == null || items.Count == 0)
            {
                Builder.TextField(args.Target, args.Member, args.Label, args.Value?.ToString() ?? "", 0f);
                return;
            }

            // Popup 要的就是 string[]; 直接建目标数组, 不先攒一个 List 再 ToArray 抄一遍。
            var texts    = new string[items.Count];
            var selected = -1;
            for (var i = 0; i < items.Count; i++)
            {
                texts[i] = items[i].Text;
                if (selected < 0 && ValueOps.Equal(items[i].Value, args.Value))
                {
                    selected = i;
                }
            }

            Builder.Popup(args.Target, args.Member, args.Label, selected, texts, 0f, args.Value, w =>
            {
                var idx = ((PopupWidget)w).Selected;
                if (idx >= 0 && idx < items.Count && selected != idx)
                {
                    return items[idx].Value;
                }

                return value;
            });
        }

        /// <summary>
        /// 取候选: 成员可以是 IEnumerable, 也可以是单个值;
        /// 元素有 Text/Value 两个属性就按它们取值, 否则整体 ToString 当文本。
        /// 取不到(成员不存在/抛异常/为 null)一律返回 null, 由调用方退回只读文本框。
        /// </summary>
        private List<DropdownItem> ResolveItems(object target, string getter)
        {
            if (target == null || string.IsNullOrEmpty(getter)) return null;

            object result;
            try
            {
                result = m_Values.GetMemberValue(target, getter);
            }
            catch
            {
                return null;
            }

            if (result == null) return null;

            if (!(result is IEnumerable enumerable))
            {
                return new List<DropdownItem>(1)
                {
                    new DropdownItem { Text = result.ToString(), Value = result },
                };
            }

            // 绝大多数候选源是 List/数组: 先按已知条数开表, 省掉一路扩容拷贝。
            var items = new List<DropdownItem>(result is ICollection collection ? collection.Count : 0);

            foreach (var element in enumerable)
            {
                if (element == null)
                {
                    items.Add(new DropdownItem { Text = "(null)", Value = null });
                    continue;
                }

                var accessors = GetAccessors(element.GetType());
                if (accessors != null)
                {
                    items.Add(new DropdownItem
                    {
                        Text  = accessors.Text.GetValue(element, null)?.ToString() ?? "",
                        Value = accessors.Value.GetValue(element, null),
                    });
                }
                else
                {
                    items.Add(new DropdownItem { Text = element.ToString(), Value = element });
                }
            }

            return items;
        }

        /// <summary>
        /// 下拉候选项的 Text/Value 访问器。
        ///
        /// 候选项类型来自业务代码、运行期不变, 按类型缓存一次即可;
        /// 没有这对属性的类型缓存成 null, 避免每帧反复尝试。
        /// </summary>
        private static Accessors GetAccessors(Type type)
        {
            if (s_Accessors.TryGetValue(type, out var cached)) return cached;

            var text  = type.GetProperty("Text");
            var value = type.GetProperty("Value");
            var found = text != null && value != null ? new Accessors(text, value) : null;

            s_Accessors[type] = found;
            return found;
        }

        private static readonly Dictionary<Type, Accessors> s_Accessors = new();

        private sealed class Accessors
        {
            public Accessors(PropertyInfo text, PropertyInfo value)
            {
                Text  = text;
                Value = value;
            }

            public readonly PropertyInfo Text;
            public readonly PropertyInfo Value;
        }

        private sealed class DropdownItem
        {
            public string Text;
            public object Value;
        }
    }
}
