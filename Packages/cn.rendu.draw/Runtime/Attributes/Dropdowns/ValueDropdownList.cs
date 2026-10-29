using System;

namespace RD.Draw
{
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
}
