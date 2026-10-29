using System;

namespace RD.Draw
{
    /// <summary>仅在编辑模式下显示。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideInPlayModeAttribute : Attribute
    {
    }
}
