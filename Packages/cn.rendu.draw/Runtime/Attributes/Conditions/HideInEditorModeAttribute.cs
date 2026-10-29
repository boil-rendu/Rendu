using System;

namespace RD.Draw
{
    /// <summary>仅在播放模式下显示。</summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class HideInEditorModeAttribute : Attribute
    {
    }
}
