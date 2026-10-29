using System;

namespace RD.Draw
{
    /// <summary>字典绘制选项。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DictionaryDrawerSettingsAttribute : Attribute
    {
        public string KeyLabel { get; set; }
        public string ValueLabel { get; set; }
        public bool IsReadOnly { get; set; }
        public DictionaryDisplayOptions DisplayMode { get; set; }

        public DictionaryDrawerSettingsAttribute(string keyLabel = null, string valueLabel = null)
        {
            KeyLabel = keyLabel;
            ValueLabel = valueLabel;
        }
    }
}
