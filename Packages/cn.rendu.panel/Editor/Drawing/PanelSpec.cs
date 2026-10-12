using UnityEngine;

namespace RD.Panel.Editor
{
    /// <summary>文本水平对齐。与 UnityEngine.TextAnchor 无关: 后者是 IMGUI 的概念。</summary>
    public enum PanelAlign
    {
        Left   = 0,
        Center = 1,
        Right  = 2,
    }

    /// <summary>提示框的信息等级, 对应 Unity IMGUI 的 MessageType。</summary>
    public enum PanelMessageKind
    {
        None    = 0,
        Info    = 1,
        Warning = 2,
        Error   = 3,
    }

    /// <summary>
    /// 一次文本绘制的要求: 字体样式 + 宽高约束。后端自行映射成自己的样式体系
    /// (IMGUI 走缓存的 GUIStyle, UIToolkit 走 class/style 属性)。
    /// </summary>
    public struct PanelTextSpec
    {
        public PanelAlign Align;
        public bool       Bold;
        public bool       Mini;
        public bool       Wrap;
        public bool       RichText;
        public float      Width;
        public float      Height;

        public static PanelTextSpec Of(PanelAlign align = PanelAlign.Left, bool bold = false, bool mini = false,
                                       bool wrap = false, bool richText = false, float width = 0f, float height = 0f)
        {
            return new PanelTextSpec
            {
                Align    = align,
                Bold     = bold,
                Mini     = mini,
                Wrap     = wrap,
                RichText = richText,
                Width    = width,
                Height   = height,
            };
        }
    }

    /// <summary>
    /// 一次按钮绘制的要求。宽度语义: <see cref="ExpandWidth"/> 为 true 时 <see cref="Width"/> 无效,
    /// 为 false 时按钮按 <see cref="Width"/> 定宽 —— IMGUI 用 GUILayoutOption, UIToolkit 用 style.width。
    /// </summary>
    public struct PanelButtonSpec
    {
        public float      Width;
        public float      Height;
        public bool       ExpandWidth;
        public string     Tooltip;
        public Texture    Icon;
        public Color      Background;
        public bool       HasBackground;

        public static PanelButtonSpec Of(float width = 0f, float height = 0f, bool expandWidth = true,
                                         string tooltip = null, Texture icon = null)
        {
            return new PanelButtonSpec
            {
                Width        = width,
                Height       = height,
                ExpandWidth  = expandWidth,
                Tooltip      = tooltip,
                Icon         = icon,
                HasBackground = false,
            };
        }

        public PanelButtonSpec WithBackground(Color color)
        {
            Background     = color;
            HasBackground  = true;
            return this;
        }

        public PanelButtonSpec WithHeight(float height)
        {
            Height = height;
            return this;
        }
    }
}
