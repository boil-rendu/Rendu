using System;
using RD.Draw.Editor.Metadata;

namespace RD.Draw.Editor.Rendering
{
    /// <summary>
    /// 枚举: 四种画法取决于特性组合 —— [Flags] 走位域多选;[EnumToggleButtons]
    /// (+MultiSelect) 走按钮组; 其余走下拉框。取值与标签都走 <see cref="EnumCatalog"/>。
    /// </summary>
    internal sealed class EnumValueControl : ValueControlBase
    {
        private readonly EnumCatalog m_Enums;

        public EnumValueControl(WidgetBuilder builder, EnumCatalog enums) : base(builder)
        {
            m_Enums = enums;
        }

        public override bool CanHandle(in ValueDrawArgs args) => args.Type.IsEnum;

        public override void Build(in ValueDrawArgs args)
        {
            var type       = args.Type;
            var value      = args.Value;
            var toggleAttr = args.Member.GetAttr<EnumToggleButtonsAttribute>();
            var toggle     = toggleAttr != null;
            var values     = m_Enums.GetValues(type);
            var isFlags    = m_Enums.IsFlags(type);

            if (toggle == false && isFlags)
            {
                Builder.EnumFlagsField(args.Target, args.Member, args.Label, (Enum)(value ?? values.GetValue(0)), 0f);
                return;
            }

            if (toggle)
            {
                var contents = m_Enums.GetLabels(type);

                if (toggleAttr.MultiSelect && isFlags)
                {
                    BuildFlagsToolbar(type, values, contents, value, args);
                    return;
                }

                // 按钮组的首参是"选中项下标", 不是枚举数值。直接传数值(Editor=2 / Model=16 /
                // All=-1 …)会让高亮落在与选中项无关的页签上, 表现为"点这个亮那个"。必须先转下标。
                var current = Array.IndexOf(values, value);
                Builder.Toolbar(args.Target, args.Member, current, contents, 0f, value, w =>
                {
                    var newIdx = ((ToolbarWidget)w).Selected;
                    if (newIdx == current) return value;
                    if (newIdx < 0 || newIdx >= values.Length) return value;
                    return values.GetValue(newIdx);
                });
                return;
            }

            var options    = m_Enums.GetLabels(type);
            var currentIdx = Array.IndexOf(values, value);
            if (currentIdx < 0) currentIdx = 0;
            Builder.Popup(args.Target, args.Member, args.Label, currentIdx, options, 0f, value,
                          w => values.GetValue(((PopupWidget)w).Selected));
        }

        /// <summary>
        /// [Flags] + MultiSelect 的多选页签: 普通位取反; "全选"成员(值 -1 或恰好等于全部
        /// 已定义位的并集)整体反相 —— 全开时再点=全不选, 否则全选; 0 成员(None)点它=全不选。
        /// </summary>
        private void BuildFlagsToolbar(Type type, Array values, string[] contents, object value, in ValueDrawArgs args)
        {
            // 全部已定义正位的并集: "全选"判据与 0 成员的亮灭都靠它(按类型缓存, 见 EnumCatalog)
            var allBits = m_Enums.GetFlagsMask(type);

            var current = value == null ? 0L : Convert.ToInt64(value);

            var states = new bool[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                var bit = Convert.ToInt64(values.GetValue(i));
                if (bit == 0)
                    states[i] = (current & allBits) == 0;               // None: 全不选时亮
                else if (bit < 0 || bit == allBits)
                    states[i] = (current & allBits) == allBits;         // All: 全选时亮
                else
                    states[i] = (current & bit) == bit;                 // 普通位: 该位开时亮
            }

            Builder.ToolbarMulti(args.Target, args.Member, states, contents, 0f, value, w =>
            {
                var clicked = ((ToolbarMultiWidget)w).ClickedIndex;
                if (clicked < 0 || clicked >= values.Length) return value;

                var clickedBit = Convert.ToInt64(values.GetValue(clicked));
                long result;
                if (clickedBit == 0)
                    result = 0L;                                            // None: 全不选
                else if (clickedBit < 0 || clickedBit == allBits)
                    result = (current & allBits) == allBits ? 0L : allBits; // All: 反相
                else
                    result = current ^ clickedBit;                          // 普通位: 取反

                if (result == current) return value;
                return Enum.ToObject(type, result);
            });
        }
    }
}