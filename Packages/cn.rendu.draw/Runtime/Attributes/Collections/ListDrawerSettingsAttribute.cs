using System;

namespace RD.Draw
{
    /// <summary>列表绘制选项。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ListDrawerSettingsAttribute : Attribute
    {
        public bool ShowPaging { get; set; }
        public bool ShowItemCount { get; set; }
        public bool ShowFoldout { get; set; }
        public bool DefaultExpandedState { get; set; }
        public bool ShowIndexLabels { get; set; }
        public bool IsReadOnly { get; set; }
        public bool HideAddButton { get; set; }
        public bool HideRemoveButton { get; set; }

        /// <summary>
        /// 条目左侧是否画拖拽手柄并允许按住换序, 默认开。
        ///
        /// 关掉它就等于"这列数据有先后意义的顺序由别处管": 手柄整列不画,
        /// 而不是画一个按不动的假手柄 —— 看得见却拖不动比没有更让人困惑。
        /// 只读列表同样不画。
        /// </summary>
        public bool DraggableItems { get; set; } = true;

        /// <summary>
        /// 点"+"时不出类型选择器, 直接按元素类型创建实例。
        ///
        /// 语义: 添加的是元素类型本身的实例, 不是 <c>default(T)</c>(引用类型即 null) ——
        /// 静默往集合里塞 null 会把空引用留到运行时的遍历里。
        /// 元素类型是抽象类/接口时仍然会弹选择器: 它没法直接实例化。
        /// </summary>
        public bool AlwaysAddDefaultValue { get; set; }

        public int NumberOfItemsPerPage { get; set; }
        public bool PagingHasValue { get; set; }
        public string CustomAddFunction { get; set; }
        public string CustomRemoveIndexFunction { get; set; }
        public string CustomRemoveElementFunction { get; set; }
        public string OnBeginListElementGUI { get; set; }
        public string OnEndListElementGUI { get; set; }
        public string OnTitleBarGUI { get; set; }

        public ListDrawerSettingsAttribute()
        {
            NumberOfItemsPerPage = 5;
        }
    }
}
