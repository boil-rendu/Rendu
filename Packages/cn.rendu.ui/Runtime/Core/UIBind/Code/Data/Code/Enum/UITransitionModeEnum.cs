using RD.Draw;
using RD.Core;

namespace RD.Client
{
    /// <summary>
    /// 界面过渡（打开/关闭）动效模式
    /// </summary>
    [LabelText("过度")]
    public enum UITransitionModeEnum
    {
        /// <summary>立即：无过渡</summary>
        [LabelText("立即")]
        Instant,

        /// <summary>淡入淡出</summary>
        [LabelText("淡入淡出")]
        Fade,

        /// <summary>淡入</summary>
        [LabelText("淡入")]
        FadeIn,

        /// <summary>淡出</summary>
        [LabelText("淡出")]
        FadeOut,
    }
}
