using System;

namespace RD.Draw
{
    /// <summary>页签分组。引擎暂无页签容器, 暂按垂直组降级渲染。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class TabGroupAttribute : PropertyGroupAttribute
    {
        public string TabName { get; set; }
        public bool UseFixedHeight { get; set; }

        public override GroupKind Kind => GroupKind.Vertical; // 页签分组暂按垂直组降级

        public TabGroupAttribute(string tabName, bool useFixedHeight = false, int order = 0)
            : base(tabName, order)
        {
            TabName         = tabName;
            UseFixedHeight  = useFixedHeight;
        }
    }
}
