using UnityEngine;

namespace RD.Draw.Editor
{
    /// <summary>
    /// 一次按钮绘制的要求。宽度语义: <see cref="ExpandWidth"/> 为 true 时 <see cref="Width"/> 无效,
    /// 为 false 时按钮按 <see cref="Width"/> 定宽 —— 走 style.width。
    /// </summary>
    public struct DrawButtonSpec
    {
        public float   Width;
        public float   Height;
        public bool    ExpandWidth;
        public string  Tooltip;
        public Texture Icon;

        /// <summary>
        /// [GUIColor] 的着色。语义是"滤镜"而不是"底色": 底纹、文字、图标一起乘这个颜色
        /// (按 style.color + 染色底 施加), 观感一致 ——
        /// 只染底纹会出现"暗绿的底 + 纯白的字", 与参考界面对不上。
        /// </summary>
        public Color   Background;

        public bool    HasBackground;

        /// <summary>
        /// 无底纹按钮(纯字形键): 列表/表格头里的 + 、分页箭头、条目行的 × 用它。
        ///
        /// 这类键在参考界面里是"工具栏字形"而不是按键: 画一个按钮框会把旁边的计数与值框
        /// 挤窄几个像素, 一排看下去像打了补丁; 无底纹后它们只占字形宽度, 悬停时才提亮。
        /// </summary>
        public bool    Frameless;

        public static DrawButtonSpec Of(float width = 0f, float height = 0f, bool expandWidth = true,
                                         string tooltip = null, Texture icon = null)
        {
            return new DrawButtonSpec
            {
                Width         = width,
                Height        = height,
                ExpandWidth   = expandWidth,
                Tooltip       = tooltip,
                Icon          = icon,
                HasBackground = false,
                Frameless     = false,
            };
        }

        public DrawButtonSpec WithFrameless(bool frameless = true)
        {
            Frameless = frameless;
            return this;
        }

        public DrawButtonSpec WithBackground(Color color)
        {
            Background    = color;
            HasBackground = true;
            return this;
        }

        public DrawButtonSpec WithHeight(float height)
        {
            Height = height;
            return this;
        }
    }
}