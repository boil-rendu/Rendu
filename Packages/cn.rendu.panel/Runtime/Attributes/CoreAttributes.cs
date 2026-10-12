using System;

namespace RD.Panel
{
    /// <summary>修改 Inspector 中显示的名称。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class LabelTextAttribute : Attribute
    {
        public string Text { get; set; }

        public LabelTextAttribute(string text)
        {
            Text = text;
        }
    }

    /// <summary>不显示属性名标签。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideLabelAttribute : Attribute
    {
    }

    /// <summary>在 Inspector 中显示该字段/属性（含非序列化成员）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class ShowInInspectorAttribute : Attribute
    {
    }

    /// <summary>隐藏“引用类型”的选择器（复制/引用选择框）。</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HideReferenceObjectPickerAttribute : Attribute
    {
    }

    /// <summary>标记：该成员交由 Inspector 序列化器处理（参与 Inspector 显示，并写入 SerializedScriptableObject 的 JSON 数据）。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class InspectorSerializeAttribute : Attribute
    {
    }

    /// <summary>只读，不可在 Inspector 中编辑。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }

    /// <summary>隐藏 MonoBehaviour 脚本体（Script 栏）。</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class HideMonoScriptAttribute : Attribute
    {
    }

    /// <summary>控制成员在 Inspector 中的显示顺序。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class PropertyOrderAttribute : Attribute
    {
        public double Order { get; set; }

        public PropertyOrderAttribute(double order)
        {
            Order = order;
        }
    }

    /// <summary>以字符串形式显示成员值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class DisplayAsStringAttribute : Attribute
    {
        public bool Overflow { get; set; }

        /// <summary>最大显示字符数(0 表示不限制)。</summary>
        public int MaxCharCount { get; set; }

        /// <summary>文本对齐方式。</summary>
        public TextAlign Alignment { get; set; }

        /// <summary>对齐是否作用于整个属性标签/布局。</summary>
        public bool AlignLabel { get; set; }

        public DisplayAsStringAttribute(bool overflow = true)
        {
            Overflow = overflow;
        }

        public DisplayAsStringAttribute(bool overflow, int maxCharCount, TextAlign alignment, bool alignLabel)
        {
            Overflow = overflow;
            MaxCharCount = maxCharCount;
            Alignment = alignment;
            AlignLabel = alignLabel;
        }
    }

    /// <summary>支持在 Inspector 中搜索成员。</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class SearchableAttribute : Attribute
    {
        public bool RecursiveFilter { get; set; }

        public SearchableAttribute()
        {
            RecursiveFilter = true;
        }
    }
}
