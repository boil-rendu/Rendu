using System;

namespace RD.Draw
{
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
