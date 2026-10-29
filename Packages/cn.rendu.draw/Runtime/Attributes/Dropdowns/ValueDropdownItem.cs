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
}
