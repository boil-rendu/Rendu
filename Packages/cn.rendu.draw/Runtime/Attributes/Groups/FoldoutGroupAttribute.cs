using System;

namespace RD.Draw
{
    /// <summary>可折叠分组（默认折叠）。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public sealed class FoldoutGroupAttribute : PropertyGroupAttribute
    {
        private readonly bool m_Expanded;

        public override GroupKind Kind => GroupKind.Foldout;
        public override bool Expanded => m_Expanded;

        public FoldoutGroupAttribute(string groupName, bool expanded = false, int order = 0)
            : base(groupName, order)
        {
            m_Expanded = expanded;
        }
    }
}
