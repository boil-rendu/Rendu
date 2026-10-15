using System;

namespace RD.Draw
{
    public enum InfoMessageType
    {
        None,
        Info,
        Warning,
        Error,
    }

    public enum TitleAlignments
    {
        Left,
        Centered,
        Right,
        Split,
    }

    public enum ButtonStyle
    {
        Default,
        CompactBox,
        FoldoutButton,
        Box,
    }

    /// <summary>文本对齐方式。</summary>
    public enum TextAlign
    {
        Left,
        Center,
        Right,
    }

    /// <summary>按钮图标与文本的对齐/布局方式。</summary>
    public enum IconAlignment
    {
        LeftOfText,
        RightOfText,
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Overlay,
    }
}
