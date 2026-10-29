using System;

namespace RD.Draw
{
    /// <summary>把一个或多个成员分组到带框(可选标题)的盒子中。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class BoxGroupAttribute : PropertyGroupAttribute
    {
        private readonly bool m_ShowLabel;
        private readonly bool m_CenterLabel;

        public override GroupKind Kind => GroupKind.Box;
        public override bool ShowLabel => m_ShowLabel;
        public override bool CenterLabel => m_CenterLabel;

        public BoxGroupAttribute(string groupName = "_DefaultGroup", bool showLabel = true,
                                 bool centerLabel = false, int order = 0)
            : base(groupName, order)
        {
            m_ShowLabel   = showLabel;
            m_CenterLabel = centerLabel;
        }
    }
}
