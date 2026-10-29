using System;

namespace RD.Draw
{
    /// <summary>
    /// 分组属性基类。渲染语义(Kind / 标题显隐 / 折叠默认态 / 标题样式)以虚成员声明,
    /// 由各派生特性按自身语义重写 —— 引擎的 DescribeGroup 只做搬运, 不再按特性类型 switch。
    /// 其它语义:
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

        /// <summary>
        /// 分组的固定宽度(像素, 0 = 不锁宽, 由内容/伸缩决定)。横排父容器里给列定宽用,
        /// 如 <c>[VerticalGroup("行/列", Width = 380f)]</c>。宽度按 style.width 生效。
        /// </summary>
        public float Width { get; set; }

        /// <summary>该分组的渲染容器类型(默认竖排; 派生特性按语义重写)。</summary>
        public virtual GroupKind Kind => GroupKind.Vertical;

        /// <summary>组标题是否显示(仅 Box / Title 容器会画标题, 其余容器忽略此值)。</summary>
        public virtual bool ShowLabel => true;

        /// <summary>折叠组的默认展开态(仅 Foldout 容器读取)。</summary>
        public virtual bool Expanded => false;

        /// <summary>组标题居中(仅 Box 容器读取)。</summary>
        public virtual bool CenterLabel => false;

        /// <summary>标题下的分隔线(仅 Title 容器读取)。</summary>
        public virtual bool HorizontalLine => true;

        /// <summary>标题加粗(仅 Title 容器读取)。</summary>
        public virtual bool BoldTitle => true;

        /// <summary>标题对齐(仅 Title 容器读取)。</summary>
        public virtual TitleAlignment Alignment => TitleAlignment.Left;

        public PropertyGroupAttribute(string groupID, float order = 0f)
        {
            GroupID = groupID;
            Order   = order;
        }
    }
}
