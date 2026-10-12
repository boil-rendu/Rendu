using System;

namespace RD.Panel
{
    /// <summary>把一个或多个成员分组到可折叠的盒子中。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class BoxGroupAttribute : PropertyGroupAttribute
    {
        public bool ShowLabel { get; set; }
        public bool CenterLabel { get; set; }

        public BoxGroupAttribute(string groupName = "_DefaultGroup", bool showLabel = true,
                                 bool centerLabel = false, int order = 0)
            : base(groupName, order)
        {
            ShowLabel   = showLabel;
            CenterLabel = centerLabel;
        }
    }

    /// <summary>可折叠分组（默认折叠）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class FoldoutGroupAttribute : PropertyGroupAttribute
    {
        public bool Expanded { get; set; }

        public FoldoutGroupAttribute(string groupName, bool expanded = false, int order = 0)
            : base(groupName, order)
        {
            Expanded = expanded;
        }
    }

    /// <summary>垂直排列分组。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class VerticalGroupAttribute : PropertyGroupAttribute
    {
        public float PaddingTop { get; set; }
        public float PaddingBottom { get; set; }

        public VerticalGroupAttribute(string path, int order = 0)
            : base(path, order)
        {
        }

        public VerticalGroupAttribute(string path, float paddingTop, float paddingBottom)
            : base(path)
        {
            PaddingTop = paddingTop;
            PaddingBottom = paddingBottom;
        }
    }

    /// <summary>水平排列分组。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class HorizontalGroupAttribute : PropertyGroupAttribute
    {
        public float Width { get; set; }

        public HorizontalGroupAttribute(string path = "_DefaultGroup", int order = 0, float width = 0f)
            : base(path, order)
        {
            Width = width;
        }
    }

    /// <summary>将多个按钮排在同一行（同组名的按钮一行）。</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ButtonGroupAttribute : PropertyGroupAttribute
    {
        public ButtonGroupAttribute(string name = "_DefaultGroup", int order = 0)
            : base(name, order)
        {
        }

        public ButtonGroupAttribute(int order)
            : base("_DefaultGroup", order)
        {
        }
    }

    /// <summary>
    /// 分组属性基类，语义如下:
    /// <list type="bullet">
    /// <item>GroupID 为分组标识, 支持 "父/子" 路径, 用路径表达分组嵌套;</item>
    /// <item>GroupName 只是 GroupID 的最后一段(仅用于显示);</item>
    /// <item>Order 决定同级分组的先后;</item>
    /// <item>HideWhenChildrenAreInvisible 默认 true —— 组内成员全部不可见时整组隐藏;</item>
    /// <item>同类型且同 GroupID 的分组属性会被合并为同一个组。</item>
    /// </list>
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class PropertyGroupAttribute : Attribute
    {
        /// <summary>分组标识, 支持 "父/子" 路径。</summary>
        public string GroupID { get; set; }

        /// <summary>分组显示名: GroupID 的最后一段。</summary>
        public string GroupName
        {
            get
            {
                if (string.IsNullOrEmpty(GroupID)) return "";
                var index = GroupID.LastIndexOf('/');
                return index < 0 ? GroupID : GroupID.Substring(index + 1);
            }
        }

        /// <summary>同级分组的排序权重。</summary>
        public float Order { get; set; }

        /// <summary>组内成员全部不可见时是否整组隐藏(默认 true)。</summary>
        public bool HideWhenChildrenAreInvisible { get; set; } = true;

        /// <summary>可见性变化是否播放动画(默认 true)。</summary>
        public bool AnimateVisibility { get; set; } = true;

        /// <summary>可解析的可见性表达式。</summary>
        public string VisibleIf { get; set; }

        public PropertyGroupAttribute(string groupID, float order = 0f)
        {
            GroupID = groupID;
            Order   = order;
        }
    }

    /// <summary>标题组（带横线的大标题分组）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TitleGroupAttribute : PropertyGroupAttribute
    {
        public TitleAlignments Alignment { get; set; }
        public bool HorizontalLine { get; set; }
        public bool BoldTitle { get; set; }
        public bool IndentChildren { get; set; }

        public TitleGroupAttribute(string groupName, TitleAlignments alignment = TitleAlignments.Left,
                                   bool horizontalLine = true, bool boldTitle = true, bool indentChildren = false)
            : base(groupName)
        {
            Alignment = alignment;
            HorizontalLine = horizontalLine;
            BoldTitle = boldTitle;
            IndentChildren = indentChildren;
        }
    }

    /// <summary>页签分组。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TabGroupAttribute : PropertyGroupAttribute
    {
        public string TabName { get; set; }
        public bool UseFixedHeight { get; set; }

        public TabGroupAttribute(string tabName, bool useFixedHeight = false, int order = 0)
            : base(tabName, order)
        {
            TabName = tabName;
            UseFixedHeight = useFixedHeight;
        }
    }
}
