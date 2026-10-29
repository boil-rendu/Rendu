using System;

namespace RD.Draw
{
    /// <summary>枚举以按钮组方式展示。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class EnumToggleButtonsAttribute : Attribute
    {
        /// <summary>
        /// [Flags] 枚举的多选模式: 点某键=把该位取反(多个位可同时点亮);
        /// "全选"成员(值 -1 或恰好覆盖全部已定义位)整体反相 —— 全开时再点=全不选。
        /// 关(默认)为单选: 每次点击把整体值替换成该键的值。
        /// 非 [Flags] 枚举上无效(永远单选)。
        /// </summary>
        public bool MultiSelect { get; set; }
        
        public EnumToggleButtonsAttribute(bool multiSelect = true)
        {
            MultiSelect = multiSelect;
        }
        
    }
}
