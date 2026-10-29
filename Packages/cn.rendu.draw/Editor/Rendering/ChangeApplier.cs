using System;
using System.Collections.Generic;
using RD.Draw.Editor.Metadata;
using RD.Draw.Editor.Rules;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 渲染后的变更应用器: 把"解释器回填进节点的新值 / 点击"与"产树时记下的旧值 / 动作"对齐,
    /// 执行写回(<see cref="DrawContext.Mutate"/>)与 [OnValueChanged] 回调。
    ///
    /// 产树与解释分离的代价是"写"必须延迟: 建树时控件还没被解释, 不知道用户输入的新值;
    /// 解释器渲染后把新值写回节点字段、把点击写进 <see cref="RenderContext.ClickedButtonKeys"/>,
    /// 本类再读这些回填结果、与产树时记下的旧值比较, 有差异才 Mutate 写回。
    ///
    /// 三类变更:
    ///   - 值型(标量/引用字段): 读节点新值, 与 <see cref="ValueChange.OldValue"/> 比较后写回成员;
    ///   - 状态型(折叠 / 页码 / 拖拽 / 列宽): 读节点新值后走 <see cref="ValueChange.OnValue"/> 回调;
    ///   - 按钮(<see cref="ButtonWidget"/>): 按 <see cref="ButtonAction.Key"/> 匹配点击后执行动作。
    /// </summary>
    internal sealed class ChangeApplier
    {
        private readonly DrawContext m_Ctx;
        private readonly ValueAccessor m_Values;

        public ChangeApplier(DrawContext ctx, ValueAccessor values)
        {
            m_Ctx    = ctx;
            m_Values = values;
        }

        /// <summary>
        /// 应用这一帧攒下的值变更与按钮点击(整棵树渲染完后调用一次)。
        ///
        /// 顺序: 先值、后按钮。按钮可能删掉元素(列表/表格的 ×), 先写值才不会被"删除导致的
        /// 下标位移"带偏 —— 一次事件里两者一般不会同时发生, 但保留模式下一帧可能同时送来。
        /// </summary>
        public void Apply(WidgetBuilder builder, RenderContext render)
        {
            ApplyValueChanges(builder.Changes);
            ApplyButtons(builder.Buttons, render.ClickedButtonKeys);
        }

        //-------------------------------------- 按钮 --------------------------------------

        private static void ApplyButtons(List<ButtonAction> buttons, List<int> clickedKeys)
        {
            if (clickedKeys == null || clickedKeys.Count == 0) return;

            for (var i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (clickedKeys.Contains(button.Key)) button.Action?.Invoke();
            }
        }

        //-------------------------------------- 值变更 --------------------------------------

        private void ApplyValueChanges(List<ValueChange> changes)
        {
            for (var i = 0; i < changes.Count; i++)
            {
                var change = changes[i];

                var newValue = change.ExtractNewValue != null
                    ? change.ExtractNewValue(change.Widget)
                    : ReadValue(change.Widget as ControlWidget);

                if (Equals(change.OldValue, newValue)) continue;

                // 状态型(折叠 / 页码 / 拖拽 / 列宽): 不写成员, 新值交给产树方自己落库。
                if (change.OnValue != null)
                {
                    change.OnValue(newValue);
                    m_Ctx.Driver.RequestRepaint();
                    continue;
                }

                if (change.Member == null || change.Owner == null) continue;

                m_Ctx.Mutate(change.Owner, () => m_Values.Write(change.Member, change.Owner, newValue));

                foreach (var attr in change.Member.GetAttrs<OnValueChangedAttribute>())
                {
                    var method = m_Values.FindMethod(change.Owner, attr.MethodName);
                    if (method != null) m_Values.InvokeSafe(change.Owner, method);
                }

                m_Ctx.Driver.RequestRepaint();
            }
        }

        //-------------------------------------- 从节点读回填结果 --------------------------------------

        /// <summary>
        /// 内置的"从节点读新值": 只覆盖标量控件与折叠头。Popup / Toolbar / ToolbarMulti 这类
        /// "节点存下标或位数组、成员是枚举/字符串"的控件, 由产树方给 <see cref="ValueChange.ExtractNewValue"/>。
        /// </summary>
        private static object ReadValue(ControlWidget widget)
        {
            switch (widget)
            {
                case TextFieldWidget w:    return w.Value;
                case IntegerFieldWidget w: return w.Value;
                case FloatFieldWidget w:   return w.Value;
                case SliderWidget w:       return w.Value;
                case ToggleWidget w:       return w.Value;
                case EnumFlagsWidget w:    return w.Value;
                case ObjectFieldWidget w:  return w.Value;
                case StructFieldWidget w:  return w.Value;
                case FoldoutWidget w:      return w.Expanded;
                default:                   return null;
            }
        }
    }
}
