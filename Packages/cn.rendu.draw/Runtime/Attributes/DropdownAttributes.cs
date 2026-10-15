using System;

namespace RD.Draw
{
    /// <summary>下拉框值列表单项。</summary>
    public class ValueDropdownItem<T>
    {
        public string Text { get; set; }
        public T Value { get; set; }

        public ValueDropdownItem()
        {
        }

        public ValueDropdownItem(string text, T value)
        {
            Text = text;
            Value = value;
        }
    }

    /// <summary>下拉框值列表。</summary>
    public class ValueDropdownList<T> : System.Collections.Generic.List<ValueDropdownItem<T>>
    {
        public void Add(string text, T value)
        {
            Add(new ValueDropdownItem<T>(text, value));
        }

        public void Add(T value)
        {
            Add(new ValueDropdownItem<T>(value?.ToString() ?? "", value));
        }
    }

    /// <summary>下拉选择框：从指定成员/方法获取候选值。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class ValueDropdownAttribute : Attribute
    {
        public string ValuesGetter { get; set; }
        public object Value { get; set; }
        public bool ExcludeDuplicates { get; set; }
        public bool HideValuesInChildTree { get; set; }
        public bool AppendNextDrawer { get; set; }
        public bool IsUniqueList { get; set; }
        public bool DoubleClickToConfirm { get; set; }

        public ValueDropdownAttribute(string valuesGetter)
        {
            ValuesGetter = valuesGetter;
        }

        public ValueDropdownAttribute(string valuesGetter, object value)
        {
            ValuesGetter = valuesGetter;
            Value = value;
        }
    }
}
